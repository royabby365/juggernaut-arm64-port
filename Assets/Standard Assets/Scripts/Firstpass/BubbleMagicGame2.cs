using UnityEngine;

public class BubbleMagicGame2 : MonoBehaviour
{
	internal float Speed;

	internal float A;

	internal Vector3 Direction;

	private bool _destroyed;

	private void Update()
	{
		if (Speed > 0f)
		{
			base.transform.Translate(new Vector3(0f, Speed * Time.deltaTime, 0f), Space.Self);
			Speed -= A * Time.deltaTime;
		}
		else if (!Globals.ForceWeakMagicNoTimeLimit)
		{
			Destroy();
		}
	}

	internal void Destroy()
	{
		if (!_destroyed)
		{
			_destroyed = true;
			Object.Destroy(base.gameObject);
		}
	}
}
