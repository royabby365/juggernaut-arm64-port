using System.Linq;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class PlayerBonuses : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText BonusLabel;

	public BagBonus[] Bonuses;

	private static readonly System.Tuple<ServerData.Skill.TypeE, string, int> EmptyBonus = System.Tuple.Create(ServerData.Skill.TypeE.Unknown, string.Empty, 0);

	private static readonly Vector3 CenterPosition = new Vector3(112f, -42f, 0f);

	private static readonly Vector3 LeftPosition = new Vector3(4f, -42f, 0f);

	private static readonly Vector3 RightPosition = new Vector3(220f, -42f, 0f);

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger.AddListener(Globals.MsgPlayerSkillChanged, MySkillChanged));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void MySkillChanged()
	{
		System.Tuple<ServerData.Skill.TypeE, string, int>[] array = SingletonT<ServerData>.I.GetAllPutOn().Where(Extensions.ItemSkillIsBonus).Select(delegate(ServerData.Item item)
		{
			ServerData.SkillInfo itemSkillInfo = item.GetItemSkillInfo();
			return System.Tuple.Create(itemSkillInfo.Skill.Type, itemSkillInfo.Skill.Title, itemSkillInfo.Current);
		})
			.ToArray();
		BonusLabel.ShowOrHide(array.Length > 0);
		if (array.Length > 2 && Globals.IsDebugBuild)
		{
			Debug.Log("[==== Unappropriate Bonus count: {0} ====]".Fmt(array.Length));
		}
		if (array.Length >= 2 && array[0].Item1 == array[1].Item1)
		{
			array = new System.Tuple<ServerData.Skill.TypeE, string, int>[1] { System.Tuple.Create(array[0].Item1, array[0].Item2, array[0].Item3 + array[1].Item3) };
		}
		for (int num = 0; num < Bonuses.Length; num++)
		{
			Bonuses[num].SetBonus((num >= array.Length) ? EmptyBonus : array[num]);
		}
		if (array.Length == 1)
		{
			Bonuses[0].transform.localPosition = CenterPosition;
		}
		else if (array.Length > 1)
		{
			Bonuses[0].transform.localPosition = LeftPosition;
			Bonuses[1].transform.localPosition = RightPosition;
		}
	}
}
