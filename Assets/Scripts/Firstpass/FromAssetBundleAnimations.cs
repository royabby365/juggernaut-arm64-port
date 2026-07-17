using UnityEngine;

internal class FromAssetBundleAnimations : MonoBehaviour
{
	internal string AnimationsAssetBundlePath;

	private bool _inDestroy;

	private void OnDestroy()
	{
		if (_inDestroy)
		{
			return;
		}
		_inDestroy = true;
		if (AnimationsAssetBundlePath == null)
		{
			return;
		}
		bool flag = true;
		Object[] array = Object.FindObjectsOfType(typeof(FromAssetBundleAnimations));
		for (int i = 0; i < array.Length; i++)
		{
			FromAssetBundleAnimations fromAssetBundleAnimations = (FromAssetBundleAnimations)array[i];
			if (!(fromAssetBundleAnimations == this) && fromAssetBundleAnimations.AnimationsAssetBundlePath == AnimationsAssetBundlePath)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			SingletonT<ResourcesManager>.I.RemoveAssetBundleAndDestroyAll(AnimationsAssetBundlePath);
		}
		AnimationsAssetBundlePath = null;
	}
}
