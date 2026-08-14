public class SingletonT<T> where T : new()
{
	private static T _i;

	public static T I
	{
		get
		{
			if (_i == null)
			{
				_i = new T();
			}
			return _i;
		}
	}
}
