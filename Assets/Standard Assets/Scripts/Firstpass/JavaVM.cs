using System;
using System.Runtime.InteropServices;

public class JavaVM
{
	private static bool _jniAvailable = true;

	[DllImport("jni")]
	private static extern int _DestroyJavaVM();

	[DllImport("jni")]
	private static extern int _AttachCurrentThread();

	[DllImport("jni")]
	private static extern int _DetachCurrentThread();

	[DllImport("jni")]
	private static extern int _GetEnv(int version);

	[DllImport("jni")]
	private static extern int _AttachCurrentThreadAsDaemon();

	public static int DestroyJavaVM()
	{
		if (!_jniAvailable) return -1;
		try { return _DestroyJavaVM(); }
		catch (DllNotFoundException) { _jniAvailable = false; return -1; }
		catch (EntryPointNotFoundException) { _jniAvailable = false; return -1; }
	}

	public static int AttachCurrentThread()
	{
		if (!_jniAvailable) return -1;
		try { return _AttachCurrentThread(); }
		catch (DllNotFoundException) { _jniAvailable = false; return -1; }
		catch (EntryPointNotFoundException) { _jniAvailable = false; return -1; }
	}

	public static int DetachCurrentThread()
	{
		if (!_jniAvailable) return -1;
		try { return _DetachCurrentThread(); }
		catch (DllNotFoundException) { _jniAvailable = false; return -1; }
		catch (EntryPointNotFoundException) { _jniAvailable = false; return -1; }
	}

	public static int GetEnv(int version)
	{
		if (!_jniAvailable) return -1;
		try { return _GetEnv(version); }
		catch (DllNotFoundException) { _jniAvailable = false; return -1; }
		catch (EntryPointNotFoundException) { _jniAvailable = false; return -1; }
	}

	public static int AttachCurrentThreadAsDaemon()
	{
		if (!_jniAvailable) return -1;
		try { return _AttachCurrentThreadAsDaemon(); }
		catch (DllNotFoundException) { _jniAvailable = false; return -1; }
		catch (EntryPointNotFoundException) { _jniAvailable = false; return -1; }
	}
}
