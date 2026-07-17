using UnityEngine;

public class TutorialFatalityCursor : MonoBehaviour
{
	public Transform Cursor;

	public Transform AnimatedPoint;

	private void Update()
	{
		Cursor.position = AnimatedPoint.position;
	}
}
