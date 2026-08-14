using UnityEngine;

public abstract class BattleBehaviourBase : MonoBehaviour
{
	private void Update()
	{
		DoUpdate(Time.deltaTime);
	}

	protected abstract void DoUpdate(float deltaTime);
}
