using System;

namespace Yarx;

internal sealed class AnonymousDisposable : IDisposable
{
	private readonly Action _dispose;

	private bool _isDisposed;

	public AnonymousDisposable(Action dispose)
	{
		_dispose = dispose;
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			_isDisposed = true;
			_dispose();
		}
	}
}
