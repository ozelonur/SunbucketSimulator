using System.Collections.Generic;
using SoundlightInteractive.Interfaces.Entities;
using SoundlightInteractive.SecureSave;
using SoundlightInteractive.Utils;

namespace _GAME_.Scripts.Managers.Generic
{
    public class EntityManager<TDataType, TEntity> where TEntity : IExtendedEntity<TDataType, TEntity>, new()
    {
        private static readonly Dictionary<TDataType, TEntity> _transientData = new();
        private static readonly Dictionary<TDataType, SecureDataManager<TEntity>> _persistentEntityManagers = new();


        public static TEntity Get(TDataType type)
        {
            return _transientData.TryGetValue(type, out TEntity entity) ? entity : GetFromPersistentData(type);
        }

        public static void Save(TDataType dataType, TEntity entity, bool isPersistent = false)
        {
            UpdateTransientData(dataType,entity);

            if (isPersistent)
            {
                SaveAsPersistent(dataType,entity);
            }
        }

        private static TEntity GetFromPersistentData(TDataType dataType)
        {
            SecureDataManager<TEntity> dataManager = GetOrCreateDataManager(dataType);
            TEntity entity = dataManager.Get();

            _transientData[dataType] = entity;

            return entity;
        }

        private static void SaveAsPersistent(TDataType dataType, TEntity entity)
        {
            entity.isChanged = true;
            SecureDataManager<TEntity> dataManager = GetOrCreateDataManager(dataType);
            dataManager.Save(entity);
            SIDebug.Log($"<color=green>{dataType} Saved Successfully!");
        }

        private static SecureDataManager<TEntity> GetOrCreateDataManager(TDataType dataType)
        {
            if (_persistentEntityManagers.TryGetValue(dataType, out SecureDataManager<TEntity> dataManager))
            {
                return dataManager;
            }

            dataManager = new SecureDataManager<TEntity>(dataType.ToString());
            _persistentEntityManagers.Add(dataType, dataManager);

            return dataManager;
        }

        private static void UpdateTransientData(TDataType type, TEntity entity)
        {
            _transientData[type] = entity;
        }
    }
}