using UnityEngine;

public class CoinMoving : MonoBehaviour
{
	public int xAmount = 4;

	public void MoveMe(Rect amount)
	{
		Vector3 localPosition = base.transform.localPosition;
		base.transform.localPosition = new Vector3((amount.width / 2f).CeilToInt() + xAmount, localPosition.y, localPosition.z);
	}
}
