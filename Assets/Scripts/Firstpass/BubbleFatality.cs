using System;
using UnityEngine;

public class BubbleFatality : MonoBehaviour
{
	private IDisposable _handler;

	private int _counter;

	private bool _dead;

	private float _moveDistance = 0.5f;

	private float _moveSpeed = 0.5f;

	private void Start()
	{
		_handler = Messenger<Battle.StateE>.AddListener(Globals.MsgBattleStateChanged, delegate(Battle.StateE _)
		{
			switch (_)
			{
			case Battle.StateE.ShowFinishDialog:
			case Battle.StateE.ShowFightResult:
				Die();
				break;
			case Battle.StateE.PlayerTime:
				if (this != null && !_dead)
				{
					_counter++;
					if (_counter == 3)
					{
						Die();
					}
				}
				break;
			}
		});
	}

	private void OnDisable()
	{
		Utils.Dispose(ref _handler);
	}

	private void Update()
	{
		if (!_dead)
		{
			Battle battle = Globals.Battle;
			if (!(battle == null) && _moveDistance > 0f)
			{
				Vector3 position = base.transform.position;
				float num = Time.deltaTime * _moveSpeed;
				_moveDistance -= num;
				base.transform.position = base.transform.position + new Vector3(0f, num, 0f);
			}
		}
	}

	public void Die()
	{
		_dead = true;
		UnityEngine.Object.Destroy(base.gameObject);
		Utils.Dispose(ref _handler);
	}

	public static void Create()
	{
		if (!(Globals.Battle.FatalityBubblePrefab == null) && !Globals.ForceDontSpawnResurrection)
		{
			Transform transform = Globals.Enemy.transform.FindChildByName("pos_head", includeInactive: true);
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Globals.Battle.FatalityBubblePrefab, transform.transform.position, Quaternion.identity);
			gameObject.AddComponent<BubbleFatality>();
			Messenger.Invoke(Globals.MsgResurrectionSpawned);
		}
	}
}
