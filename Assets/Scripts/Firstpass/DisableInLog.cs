using UnityEngine;

internal class DisableInLog : MonoBehaviour
{
	private void OnDisable()
	{
		Utils.Log("DisableInLog", base.name);
	}
}
