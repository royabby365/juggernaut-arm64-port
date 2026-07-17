using UnityEngine;

internal class StartMenuSimple : MonoBehaviour
{
	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 150f, 50f), "CONTINUE"))
		{
			GameObject.Find("__main_menu").GetComponent<MainMenu>().GoToFromStartMenuToMainMap();
		}
	}
}
