using UnityEngine;

internal class FromAssetBundle : MonoBehaviour
{
	public string Path;

	public GameObject Proto;

	private bool _inDestroy;

	private void OnDestroy()
	{
		if (_inDestroy)
		{
			return;
		}
		_inDestroy = true;
		Object[] array = Object.FindObjectsOfType(typeof(FromAssetBundle));
		for (int i = 0; i < array.Length; i++)
		{
			FromAssetBundle fromAssetBundle = (FromAssetBundle)array[i];
			if (fromAssetBundle.Path == Path)
			{
				return;
			}
		}
		SingletonT<ResourcesManager>.I.RemoveAssetBundleAndDestroyAll(Path);
		Path = null;
	}
}
