using SoundlightInteractive.EventSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections;

namespace DefaultNamespace
{
    public class CleanActor : Actor
    {
        [Header("Configurations")] 
        [SerializeField] private Texture2D dirtMaskBase;
        [SerializeField] private Texture2D brush;
        [SerializeField] private float calculationInterval = 1.0f; // Interval in seconds

        private Camera _mainCamera;
        private Texture2D _templateDirtMask;
        private MeshRenderer _renderer;
        private NativeArray<Color32> _dirtMaskPixels;
        private NativeArray<Color32> _brushPixels;

        private int _totalPixels;
        private int _cleanedPixelsCount;
        private bool _isDirty;
        private bool _calculationStopped;

        private void Start()
        {
            _mainCamera = Camera.main;
            _renderer = GetComponent<MeshRenderer>();
            CreateTexture();
            _totalPixels = _templateDirtMask.width * _templateDirtMask.height;

            // Start the coroutine to calculate cleaned percentage at intervals
            StartCoroutine(CalculateCleanedPercentageRoutine());
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    Vector2 textureCoord = hit.textureCoord;

                    int pixelX = (int)(textureCoord.x * _templateDirtMask.width);
                    int pixelY = (int)(textureCoord.y * _templateDirtMask.height);

                    int pixelXOffset = Mathf.Clamp(pixelX - (brush.width / 2), 0, _templateDirtMask.width - brush.width);
                    int pixelYOffset = Mathf.Clamp(pixelY - (brush.height / 2), 0, _templateDirtMask.height - brush.height);

                    ApplyBrushJob job = new ApplyBrushJob
                    {
                        dirtMaskPixels = _dirtMaskPixels,
                        brushPixels = _brushPixels,
                        brushWidth = brush.width,
                        maskWidth = _templateDirtMask.width,
                        pixelXOffset = pixelXOffset,
                        pixelYOffset = pixelYOffset
                    };

                    JobHandle handle = job.Schedule(brush.width * brush.height, 64);
                    handle.Complete();

                    _templateDirtMask.SetPixels32(_dirtMaskPixels.ToArray());
                    _templateDirtMask.Apply();

                    // Mark as dirty for recalculation
                    _isDirty = true;
                }
            }
        }

        private void OnDestroy()
        {
            if (_dirtMaskPixels.IsCreated) _dirtMaskPixels.Dispose();
            if (_brushPixels.IsCreated) _brushPixels.Dispose();
        }

        private void CreateTexture()
        {
            _templateDirtMask = new Texture2D(dirtMaskBase.width, dirtMaskBase.height, TextureFormat.RGBA32, false);
            _templateDirtMask.SetPixels32(dirtMaskBase.GetPixels32());
            _templateDirtMask.Apply();

            _dirtMaskPixels = new NativeArray<Color32>(dirtMaskBase.GetPixels32(), Allocator.Persistent);
            _brushPixels = new NativeArray<Color32>(brush.GetPixels32(), Allocator.Persistent);

            _renderer.material.SetTexture("_DirtMask", _templateDirtMask);
        }

        private IEnumerator CalculateCleanedPercentageRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(calculationInterval);

                if (_isDirty && !_calculationStopped)
                {
                    CalculateCleanedPercentage();

                    if (_calculationStopped)
                    {
                        break;
                    }
                }
            }
        }

        private void CalculateCleanedPercentage()
        {
            int cleanedPixelCount = 0;

            for (int i = 0; i < _dirtMaskPixels.Length; i++)
            {
                if (_dirtMaskPixels[i].r == 0 && _dirtMaskPixels[i].g == 0 && _dirtMaskPixels[i].b == 0)
                {
                    cleanedPixelCount++;
                }
            }

            float cleanedPercentage = (float)cleanedPixelCount / _totalPixels * 100;
            Debug.Log($"Cleaned Area: {cleanedPercentage:F2}%");

            if (cleanedPercentage >= 100f)
            {
                _calculationStopped = true;
                Debug.Log("Cleaning completed. Calculation stopped.");
            }
        }

        public override void ResetActor()
        {
            // Implement reset logic here if necessary
        }

        public override void InitializeActor()
        {
            // Implement initialization logic here if necessary
        }

        [BurstCompile]
        private struct ApplyBrushJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<Color32> dirtMaskPixels;
            [ReadOnly] public NativeArray<Color32> brushPixels;
            public int brushWidth;
            public int maskWidth;
            public int pixelXOffset;
            public int pixelYOffset;

            public void Execute(int index)
            {
                int x = index % brushWidth;
                int y = index / brushWidth;
                int brushIndex = y * brushWidth + x;
                int maskIndex = (pixelYOffset + y) * maskWidth + (pixelXOffset + x);

                if (maskIndex < dirtMaskPixels.Length && brushIndex < brushPixels.Length)
                {
                    Color32 pixelDirt = brushPixels[brushIndex];
                    Color32 pixelDirtMask = dirtMaskPixels[maskIndex];

                    dirtMaskPixels[maskIndex] = new Color32(0, (byte)(pixelDirtMask.g * pixelDirt.g / 255), 0, 255);
                }
            }
        }
    }
}
