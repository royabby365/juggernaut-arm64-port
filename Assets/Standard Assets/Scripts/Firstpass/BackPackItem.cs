using UnityEngine;

public class BackPackItem : MonoBehaviour
{
	public Transform on;

	public Transform off;

	public void SetOn()
	{
		on.ShowOrHide(show: true);
		off.ShowOrHide(show: false);
	}

	public void SetOff()
	{
		on.ShowOrHide(show: false);
		off.ShowOrHide(show: true);
	}
}
