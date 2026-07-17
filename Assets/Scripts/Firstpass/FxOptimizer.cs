using UnityEngine;

public class FxOptimizer : MonoBehaviour
{
	public Texture[] Textures;

	public static FxOptimizer I;

	private void Start()
	{
		I = this;
	}

	public void DestroFx(GameObject fx)
	{
		Object.Destroy(fx);
	}
}
