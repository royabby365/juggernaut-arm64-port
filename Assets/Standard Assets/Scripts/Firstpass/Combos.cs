using UnityEngine;

public class Combos : MonoBehaviour
{
	public static readonly string Front = "front";

	public static readonly string Left = "left";

	public static readonly string Right = "right";

	public static readonly string Question = "question";

	public static readonly string Active = "active";

	public static readonly string Disabled = "disabled";

	public static readonly string Big = "big";

	private Combo[] _combos = new Combo[7];

	public int combosSpan = 72;

	public void SetCombo(int index, string direction, string status)
	{
		_combos[index].SetCombo(direction, status);
	}

	private void Start()
	{
		for (int i = 0; i < _combos.Length; i++)
		{
			GameObject gameObject = GameObject.Find("combo_" + i);
			Vector3 localPosition = gameObject.transform.localPosition;
			gameObject.transform.localPosition = new Vector3(localPosition.x, -combosSpan * (6 - i), localPosition.z);
			_combos[i] = gameObject.GetComponent<Combo>();
		}
	}
}
