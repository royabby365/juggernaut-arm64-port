using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Yarx;

public class MagicBookSchool : MonoBehaviour
{
	private const int MaxProgressGaugeScale = 77;

	private const string ActivSmallIcon = "{0}_32x36";

	private const string InactivSmallIcon = "{0}_32x36_bw";

	private const string SpellIconFmt = "{0}_{1}";

	private CompositeDisposable _subscriptions;

	public ServerData.Skill.TypeE School;

	public SpriteButton ImproveButton;

	public Sprite SmallIcon;

	public SpriteText UsageProgressText;

	public Transform UsageProgressGauge;

	public SpriteText ImprovePrice;

	public SpriteText SpellName;

	public SpriteText SpellDescription;

	public Sprite SpellIconFrame;

	public Sprite SpellIcon;

	private readonly Color _inactiveSmallIconColor = new Color32(91, 65, 35, byte.MaxValue);

	private readonly Color _inactiveSpellIconColor = new Color32(128, 128, 128, 128);

	private SpriteText _buttonLabel;

	private ServerData.Spell _current;

	private ServerData.Spell _next;

	private int _price;

	private string _skillUsageFormat;

	private string _skillPowerFormat;

	private Color _fireColor = new Color32(134, 44, 2, 240);

	private Color _iceColor = new Color32(13, 70, 134, 240);

	private Color _darkColor = new Color32(73, 23, 101, 240);

	private Color _lightningColor = new Color32(9, 93, 123, 240);

	private void Awake()
	{
		_buttonLabel = ImproveButton.transform.GetComponentInChildren<SpriteText>();
		_skillUsageFormat = SingletonT<ServerData>.I.GetPhrase(UsageProgressText.Phrase_);
		UsageProgressText.Phrase_ = ServerData.PhrasesE.Custom;
		_skillPowerFormat = SingletonT<ServerData>.I.GetPhrase(SpellDescription.Phrase_);
		SpellDescription.Phrase_ = ServerData.PhrasesE.Custom;
	}

	public void Init()
	{
		SetSpell();
		RefreshGauge();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.Skill.TypeE, int>.AddListener(Globals.MsgSpellUsedCountChanged, OnSpellUsedCountChanged));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgNewPersInited, SetSpell));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	public bool ButtonPressed()
	{
		if (_current != null)
		{
			int currentUsage = GetCurrentUsage();
			int points = _current.Points;
			if (currentUsage < points)
			{
				Messenger.Invoke(Globals.MsgMagicBookNeedMoreUsings);
				return false;
			}
		}
		int playerFundsCount = ServerData.MoneyType.TypeE.Skull.GetPlayerFundsCount();
		if (playerFundsCount < _price)
		{
			Messenger.Invoke(Globals.MsgMagicBookNeedMoreSkulls);
			return false;
		}
		ServerData.MoneyType.TypeE.Skull.ChangePlayerFundsCount(-_price);
		SingletonT<ServerData>.I.MySpells.Add(_next.Id);
		Messenger.Invoke(Globals.MsgPlayerSpellBuyed, _next);
		if (_current != null)
		{
			SingletonT<ServerData>.I.MySpells.Remove(_current.Id);
		}
		ResetCurrentUsage();
		SetSpell();
		return true;
	}

	private void SetSpell()
	{
		_current = SingletonT<ServerData>.I.GetMyMaxSpell(School);
		_next = GetNextSpell(_current);
		RefreshGauge();
		SpellName.Text_ = GetSpellTitle(_current);
		SpellName.NamedColorE_ = GetSpellTextColor();
		SpellIconFrame.Tint_ = GetFrameColor(_current);
		SpellDescription.Text_ = GetSpellDescription(_current);
		SpellDescription.NamedColorE_ = GetDescriptionTextColor();
		SpellIcon.SpriteName_ = GetSpellIconName(_current);
		if (_next == null)
		{
			SetMaxRemoveButton();
			_price = -1;
		}
		else
		{
			_price = GetSpellPrice(_next);
			ImprovePrice.Text_ = ((_price != 0) ? _price.ToString(CultureInfo.InvariantCulture) : string.Empty);
		}
		_buttonLabel.Phrase_ = ((!(_current == null)) ? ServerData.PhrasesE.ButtonImprove : ServerData.PhrasesE.ButtonBuy);
	}

	private string GetSpellTitle(ServerData.Spell current)
	{
		if (current == null)
		{
			current = GetFirstSpell();
		}
		return current.Title;
	}

	private FontManager.ColorE GetDescriptionTextColor()
	{
		if (_current == null)
		{
			return FontManager.ColorE.MagicBookInactiveText;
		}
		return FontManager.ColorE.OldTextBrown;
	}

	private FontManager.ColorE GetSpellTextColor()
	{
		if (_current == null)
		{
			return FontManager.ColorE.MagicBookInactiveText;
		}
		return School switch
		{
			ServerData.Skill.TypeE.MagicIce => FontManager.ColorE.MagicIceMonotone, 
			ServerData.Skill.TypeE.MagicFire => FontManager.ColorE.MagicFireMonotone, 
			ServerData.Skill.TypeE.MagicDark => FontManager.ColorE.MagicDarkMonotone, 
			ServerData.Skill.TypeE.MagicElectro => FontManager.ColorE.MagicLightningMonotone, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private ServerData.Spell GetNextSpell(ServerData.Spell current)
	{
		if (current != null)
		{
			return current.NextSpell;
		}
		return GetFirstSpell();
	}

	private ServerData.Spell GetFirstSpell()
	{
		ServerData.Spell spell = null;
		foreach (ServerData.Spell value in SingletonT<ServerData>.I._spells.Values)
		{
			if (value.SkillType == School && (spell == null || value.PowerK < spell.PowerK))
			{
				spell = value;
			}
		}
		return spell;
	}

	private string GetSpellIconName(ServerData.Spell current)
	{
		string spellNameStem = GetSpellNameStem();
		int b = ((!(current == null)) ? current.Level : 0);
		b = Mathf.Max(3, b);
		SpellIcon.Tint_ = ((!(current == null)) ? Color.gray : _inactiveSpellIconColor);
		return "{0}_{1}".Fmt(spellNameStem, b);
	}

	private string GetSpellDescription(ServerData.Spell current)
	{
		if (current == null)
		{
			current = GetFirstSpell();
		}
		return _skillPowerFormat.Fmt(10f * current.PowerK);
	}

	private Color GetFrameColor(ServerData.Spell current)
	{
		if (current == null)
		{
			return _inactiveSmallIconColor;
		}
		return School switch
		{
			ServerData.Skill.TypeE.MagicIce => _iceColor, 
			ServerData.Skill.TypeE.MagicFire => _fireColor, 
			ServerData.Skill.TypeE.MagicDark => _darkColor, 
			ServerData.Skill.TypeE.MagicElectro => _lightningColor, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static int GetSpellPrice(ServerData.Spell next)
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

	private void SetMaxRemoveButton()
	{
		ImproveButton.transform.GoToHell();
		ImprovePrice.transform.parent.GoToHell();
	}

	private void RefreshGauge()
	{
		SetGaugeActive(_current != null);
		if (_current != null)
		{
			int currentUsage = GetCurrentUsage();
			int points = _current.Points;
			SetProgressGauge(currentUsage, points);
		}
	}

	private int GetCurrentUsage()
	{
		return SingletonT<ServerData>.I.PlayerParams.SpellsUsedCount.ContainsKey(School) ? SingletonT<ServerData>.I.PlayerParams.SpellsUsedCount[School] : 0;
	}

	private void ResetCurrentUsage()
	{
		if (SingletonT<ServerData>.I.PlayerParams.SpellsUsedCount.ContainsKey(School))
		{
			SingletonT<ServerData>.I.PlayerParams.SpellsUsedCount[School] = 0;
		}
	}

	private void SetProgressGauge(int current, int max)
	{
		if (max < 1)
		{
			UsageProgressText.Text_ = string.Empty;
			UsageProgressGauge.localScale = new Vector3(77f, 1f, 1f);
			return;
		}
		float num = Mathf.Clamp01((float)current / (float)max);
		UsageProgressText.Text_ = _skillUsageFormat.Fmt(current, max);
		int num2 = (num * 77f).RoundToInt();
		UsageProgressGauge.localScale = new Vector3(num2, 1f, 1f);
		UsageProgressGauge.GetComponent<Renderer>().material.mainTextureScale = new Vector2(num2, 1f);
	}

	private void SetGaugeActive(bool setActive)
	{
		string spellNameStem = GetSpellNameStem();
		SmallIcon.SpriteName_ = ((!setActive) ? "{0}_32x36_bw" : "{0}_32x36").Fmt(spellNameStem);
		SmallIcon.Tint_ = ((!setActive) ? _inactiveSmallIconColor : Color.gray);
		if (!setActive)
		{
			SetProgressGauge(0, 1);
			UsageProgressText.Text_ = string.Empty;
		}
	}

	private string GetSpellNameStem()
	{
		return School switch
		{
			ServerData.Skill.TypeE.MagicIce => "ice", 
			ServerData.Skill.TypeE.MagicFire => "fire", 
			ServerData.Skill.TypeE.MagicDark => "dark", 
			ServerData.Skill.TypeE.MagicElectro => "lightning", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void OnSpellUsedCountChanged(ServerData.Skill.TypeE spell, int i)
	{
		if (spell == School)
		{
			RefreshGauge();
		}
	}
}
