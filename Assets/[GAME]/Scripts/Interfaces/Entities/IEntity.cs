namespace DefaultNamespace
{
    public interface IEntity<TDataType>
    {
        TDataType dataType { get; set; }
        float count { get; set; }
    }
}