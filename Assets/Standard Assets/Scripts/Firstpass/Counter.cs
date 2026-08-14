using UnityEngine;

public class Counter : MonoBehaviour
{
	public enum BankMoneyTypeE
	{
		Gold,
		Diamonds,
		Real
	}

	private bool _bonus;

	private float _count;

	public BankMoneyTypeE MoneyType;

	public SpriteText Text;

	public float Count
	{
		get
		{
			return _count;
		}
		set
		{
			_count = value;
			UpdateText();
		}
	}

	public bool IsBonus
	{
		get
		{
			return _bonus;
		}
		set
		{
			_bonus = value;
			Transform transform = base.transform.FindChildByName("icon", includeInactive: true);
			if (transform != null && _bonus)
			{
				transform.GoToHell();
			}
			UpdateText();
		}
	}

	public void UpdateText()
	{
		string format;
		string format2;
		string text;
		float num;
		if (UnityApi.GetUseOpenFeintPurchases())
		{
			format = "{1} {0}";
			format2 = "{1} {0}% {2}";
			text = ((MoneyType != BankMoneyTypeE.Real) ? string.Empty : "C");
			num = ((MoneyType != BankMoneyTypeE.Real) ? _count : Globals.OpenFeintBankRealToCoins(_count));
		}
		else if (UnityApi.UseGameClub())
		{
			format = "{1} {0}";
			format2 = "{1} {0}% {2}";
			text = ((MoneyType != BankMoneyTypeE.Real) ? string.Empty : "KRW");
			num = ((MoneyType != BankMoneyTypeE.Real) ? _count : _count);
		}
		else
		{
			format = "{0}{1}";
			format2 = "{0}{1}% {2}";
			text = ((MoneyType != BankMoneyTypeE.Real) ? string.Empty : "$");
			num = ((MoneyType != BankMoneyTypeE.Real) ? _count : _count);
		}
		if (IsBonus)
		{
			Text.Text_ = format2.Fmt(text, num, SingletonT<ServerData>.I.GameSettings.BankFree);
		}
		else
		{
			Text.Text_ = format.Fmt(text, num);
		}
	}
}
