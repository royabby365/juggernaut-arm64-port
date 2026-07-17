using UnityEngine;

public class TutorialSliceAttackCursor : MonoBehaviour
{
	public Transform Cursor;

	public Transform LeftSliceAnimatedPoint;

	public Transform RightSliceAnimatedPoint;

	public bool IsLeft;

	private void Update()
	{
		if (IsLeft)
		{
			Cursor.position = LeftSliceAnimatedPoint.position;
		}
		else
		{
			Cursor.position = RightSliceAnimatedPoint.position;
		}
	}
}
