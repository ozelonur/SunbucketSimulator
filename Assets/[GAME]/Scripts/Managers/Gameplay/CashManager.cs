using _GAME_.Scripts.Entities.CashEntity;
using _GAME_.Scripts.GlobalVariables;
using _GAME_.Scripts.Managers.Generic;
using Sirenix.OdinInspector;
using SoundlightInteractive.Manager;
using SoundlightInteractive.Utils;
using UnityEngine;

namespace _GAME_.Scripts.Managers.Gameplay
{
    public class CashManager : Manager<CashManager>
    {
        private const float InitialMoney = 10000;
        [ReadOnly, SerializeField] private float currentMoney;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                AddMoney(1000);
            }


            if (Input.GetKeyDown(KeyCode.S))
            {
                StatusCode statusCode = SubtractCash(1000);

                if (statusCode == StatusCode.NotEnoughMoney)
                {
                    SIDebug.LogError("There is no enough money for this subtraction!");
                }
            }
        }

        public override void ResetActor()
        {
            // Implement reset logic if needed
        }

        public override void InitializeActor()
        {
            // Implement initialization logic if needed
        }

        private CashEntity GetOrCreateEntity()
        {
            CashEntity entity = EntityManager<DataType, CashEntity>.Get(DataType.Cash);

            if (entity == null || !entity.isChanged)
            {
                SIDebug.Log("Entity is null or unchanged! Creating and saving as new entity...");
                entity = new CashEntity { currentCashAmount = InitialMoney };
                EntityManager<DataType, CashEntity>.Save(DataType.Cash, entity, true);
            }

            return entity;
        }

        public void AddMoney(float amount)
        {
            CashEntity entity = GetOrCreateEntity();
            entity.currentCashAmount += amount;
            EntityManager<DataType, CashEntity>.Save(DataType.Cash, entity, true);

            currentMoney = entity.currentCashAmount;
        }

        public StatusCode SubtractCash(float amount)
        {
            CashEntity entity = GetOrCreateEntity();

            if (entity.currentCashAmount - amount < 0)
            {
                return StatusCode.NotEnoughMoney;
            }

            entity.currentCashAmount -= amount;
            EntityManager<DataType, CashEntity>.Save(DataType.Cash, entity, true);

            currentMoney = entity.currentCashAmount;

            return StatusCode.Successful;
        }

        public float GetCurrentCash()
        {
            CashEntity entity = EntityManager<DataType, CashEntity>.Get(DataType.Cash);
            return entity is { isChanged: true } ? entity.currentCashAmount : InitialMoney;
        }
    }
}