using System;
using _GAME_.Scripts.GlobalVariables;
using SoundlightInteractive.Interfaces.Entities;

namespace _GAME_.Scripts.Entities.CashEntity
{
    [Serializable]
    public class CashEntity : IExtendedEntity<DataType, CashEntity>
    {
        public DataType dataType { get; set; }
        public CashEntity data { get; set; }
        public bool isChanged { get; set; }

        public float currentCashAmount { get; set; }
    }
}