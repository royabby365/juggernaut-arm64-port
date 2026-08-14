public interface ISaveLoad<T>
{
	T Load(int index);

	void Save(int index, T o);

	void Clear(int index);
}
