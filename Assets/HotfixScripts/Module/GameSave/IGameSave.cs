namespace GameSave
{
    public enum DataType
    {
        FileData,
        Default,
        Bag,
        Character
    }
    public interface IGameSave
    {
        public DataType dataType { get; }
        public string SaveData();
        public void LoadData(string json);
    }
}
