using UnityEngine;

public class ValueIconMoving : MonoBehaviour
{
	public void MoveMe(Rect amount)
	{
		Vector3 localPosition = base.transform.localPosition;
		base.transform.localPosition = new Vector3(amount.width.CeilToInt() + 6, localPosition.y, localPosition.z);
	}
}
