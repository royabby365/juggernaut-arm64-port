using System;

namespace Yarx
{

public class BooleanDisposable : IDisposable
{
	public bool IsDisposed { get; private set; }

	public void Dispose()
	{
		IsDisposed = true;
	}
}
}
