using System;

namespace Yarx;

public class MutableDisposable : IDisposable
{
	private readonly object _lock = new object();

	private IDisposable _disposable;

	private bool _disposed;

	public IDisposable Disposable
	{
		get
		{
			return _disposable;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			bool disposed;
			lock (_lock)
			{
				disposed = _disposed;
				if (!_disposed)
				{
					if (_disposable != null)
					{
						_disposable.Dispose();
					}
					_disposable = value;
				}
			}
			if (disposed)
			{
				value.Dispose();
			}
		}
	}

	public void Dispose()
	{
		lock (_lock)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (Disposable != null)
				{
					Disposable.Dispose();
					_disposable = null;
				}
			}
		}
	}
}
