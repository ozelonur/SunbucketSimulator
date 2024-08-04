namespace SoundlightInteractive.Interfaces.Entities
{
    public interface IExtendedEntity<TDataType, T>
    {
        TDataType dataType { get; set; }
        T data { get; set; }

        bool isChanged { get; set; }
    }
}