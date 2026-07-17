using System.Collections.Generic;
using UnityEngine;

public class TutorialCastMagicCursor : MonoBehaviour
{
	private Dictionary<ServerData.Skill.TypeE, Transform> _gestures;

	public Transform Cursor;

	public Transform GestureDark;

	public Transform GestureFire;

	public Transform GestureIce;

	public Transform GestureLighting;

	public ServerData.Skill.TypeE CurrentGesture = ServerData.Skill.TypeE.MagicDark;

	private void Start()
	{
		_gestures = new Dictionary<ServerData.Skill.TypeE, Transform>();
		_gestures[ServerData.Skill.TypeE.MagicDark] = GestureDark;
		_gestures[ServerData.Skill.TypeE.MagicFire] = GestureFire;
		_gestures[ServerData.Skill.TypeE.MagicIce] = GestureIce;
		_gestures[ServerData.Skill.TypeE.MagicElectro] = GestureLighting;
	}

	private void Update()
	{
		Cursor.position = _gestures[CurrentGesture].position;
	}
}
