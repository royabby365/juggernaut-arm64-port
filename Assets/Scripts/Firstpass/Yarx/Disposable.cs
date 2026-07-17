using System;

namespace Yarx;

public static class Disposable
{
	public static IDisposable Empty { get; private set; }

	static Disposable()
	{
		Empty = new AnonymousDisposable(delegate
		{
		});
	}

	public static IDisposable ToDisposable(this Action cancel)
	{
		if (cancel == null)
		{
			throw new ArgumentNullException("cancel");
		}
		return Create(cancel);
	}

	public static IDisposable Create(Action dispose)
	{
		if (dispose == null)
		{
			throw new ArgumentNullException("dispose");
		}
		return new AnonymousDisposable(dispose);
	}
}
