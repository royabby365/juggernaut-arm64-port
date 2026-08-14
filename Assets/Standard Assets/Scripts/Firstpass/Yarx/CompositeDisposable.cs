using System;
using System.Collections.Generic;

namespace Yarx
{

public class CompositeDisposable : IDisposable
{
	private readonly List<IDisposable> _disposables;

	private bool _disposed;

	public int Count
	{
		get
		{
			lock (_disposables)
			{
				return _disposables.Count;
			}
		}
	}

	public CompositeDisposable(params IDisposable[] disposables)
	{
		if (disposables == null)
		{
			throw new ArgumentNullException("disposables");
		}
		foreach (IDisposable disposable in disposables)
		{
			if (disposable == null)
			{
				throw new ArgumentOutOfRangeException("disposables");
			}
		}
		_disposables = new List<IDisposable>(disposables);
	}

	public void Dispose()
	{
		lock (_disposables)
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			foreach (IDisposable disposable in _disposables)
			{
				disposable.Dispose();
			}
			_disposables.Clear();
		}
	}

	public IDisposable Add(IDisposable disposable)
	{
		if (disposable == null)
		{
			throw new ArgumentNullException("disposable");
		}
		bool disposed;
		lock (_disposables)
		{
			disposed = _disposed;
			if (!_disposed)
			{
				_disposables.Add(disposable);
			}
		}
		if (disposed)
		{
			disposable.Dispose();
		}
		return this;
	}

	public IDisposable Add(Action cancel)
	{
		return Add(Disposable.Create(cancel));
	}

	public bool Contains(IDisposable item)
	{
		lock (_disposables)
		{
			return _disposables.Contains(item);
		}
	}

	public bool Remove(IDisposable item)
	{
		bool flag;
		lock (_disposables)
		{
			flag = _disposables.Remove(item);
			if (flag)
			{
				item.Dispose();
			}
		}
		return flag;
	}

	public void RemoveAllBut(IDisposable item)
	{
		lock (_disposables)
		{
			IDisposable[] array = _disposables.ToArray();
			foreach (IDisposable disposable in array)
			{
				if (disposable != item && Remove(disposable))
				{
					disposable.Dispose();
				}
			}
		}
	}

	public bool RemoveNotDispose(IDisposable item)
	{
		lock (_disposables)
		{
			return _disposables.Remove(item);
		}
	}
}
}
