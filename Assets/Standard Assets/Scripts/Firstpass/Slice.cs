using System.Collections.Generic;
using UnityEngine;

public class Slice : MonoBehaviour
{
	private float _time;

	private int _stage;

	private float _xScale;

	private List<float> _stageTime = new List<float> { 0.2f, 1f, 1.2f };

	public Sprite Sprite;

	private void Start()
	{
		Sprite.ClipVertical(0f);
		_xScale = Sprite.transform.localScale.x;
	}

	private void Update()
	{
		switch (_stage)
		{
		case 0:
			Sprite.ClipVertical(Mathf.Lerp(0f, 1f, _time / _stageTime[_stage]));
			if (_time > _stageTime[_stage])
			{
				Sprite.ClipVertical(1f);
				_stage++;
			}
			break;
		case 1:
			if (_time > _stageTime[_stage])
			{
				_stage++;
			}
			break;
		case 2:
			Sprite.transform.localScale = new Vector3(_xScale * Mathf.Lerp(1f, 0f, (_time - _stageTime[_stage - 1]) / (_stageTime[_stage] - _stageTime[_stage - 1])), Sprite.transform.localScale.y, Sprite.transform.localScale.z);
			if (_time > _stageTime[_stage])
			{
				Sprite.transform.localScale = new Vector3(0f, Sprite.transform.localScale.y, Sprite.transform.localScale.z);
				Object.Destroy(base.gameObject);
			}
			break;
		}
		_time += Time.deltaTime;
	}

	public void SetSlice(float angle)
	{
		if (!(HudMk1.Instance == null))
		{
			base.transform.parent = HudMk1.Instance.transform;
			Vector3 vector = Camera2D.Scale * Globals.MainMenu.Battle.BattleCameraController.GetEnemyScreenSpacePivot();
			float num = 20f;
			Vector3 vector2 = new Vector3(Mathf.Cos(Time.realtimeSinceStartup * Random.value) * num, Mathf.Sin(Time.realtimeSinceStartup * Random.value) * num, 0f);
			base.transform.localPosition = new Vector3(vector.x - (float)(Camera2D.ScreenWidth / 2), vector.y - (float)(Camera2D.ScreenHeight / 2), 300f) + vector2;
			base.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
		}
	}
}
