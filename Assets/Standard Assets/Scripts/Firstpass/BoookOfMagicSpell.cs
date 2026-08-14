using System.Collections.Generic;
using UnityEngine;
using Yarx;

public class BoookOfMagicSpell : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public SpriteText ButtonLable;

	public SpriteText Price;

	public SpriteText SpellName;

	public Transform BuyButton;

	public Transform Icon;

	public Transform IconDisabled;

	public void SetSpell(ServerData.Spell current, ServerData.Spell next)
	{
		IconDisabled.gameObject.SetActive(current == null);
		Icon.gameObject.SetActive(current != null);
		SpellName.Text_ = ((!(current == null)) ? current.Title : string.Empty);
		if (next == null)
		{
			SetMaxRemoveInterface();
			return;
		}
		int spellPrice = GetSpellPrice(next);
		Price.Text_ = spellPrice.ToString();
		ButtonLable.Phrase_ = ((!(current == null)) ? ServerData.PhrasesE.ButtonImprove : ServerData.PhrasesE.ButtonBuy);
	}

	public static int GetSpellPrice(ServerData.Spell next)
	{
		foreach (KeyValuePair<ServerData.MoneyType, int> item in next.Price)
		{
			if (item.Key.Code == ServerData.MoneyType.ZeroSkull.Code)
			{
				return item.Value;
			}
		}
		return 0;
	}

	private void Awake()
	{
		BuyButton.name = "button_" + base.name;
		BuyButton.GetComponent<SpriteButton>().Init(12, 0);
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void SetMaxRemoveInterface()
	{
		Price.gameObject.SetActive(false);
		Price.transform.parent.gameObject.SetActive(false);
		BuyButton.GetComponent<SpriteButton>().SetInactive();
		BuyButton.GetComponent<SpriteButton>().UnregisterMe();
		ButtonLable.Phrase_ = ServerData.PhrasesE.ButtonMax;
	}
}
