namespace Assets.Plugins.GameCode.Data;

public interface ISaveLoadData<T> where T : class
{
	bool SaveData(string path, T data);

	T LoadData(byte[] data);
}
