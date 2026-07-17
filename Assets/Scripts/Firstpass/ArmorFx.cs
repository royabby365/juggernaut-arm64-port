using UnityEngine;

internal class ArmorFx : MonoBehaviour
{
	private GameObject _fx;

	public string FxName;

	private void Start()
	{
		if (FxName == null || !(SingletonT<Fxs>.I.NewFx(FxName, base.transform.position, base.transform.rotation, base.transform, forceDraw: true) != null))
		{
			Object.Destroy(this);
		}
	}
}
