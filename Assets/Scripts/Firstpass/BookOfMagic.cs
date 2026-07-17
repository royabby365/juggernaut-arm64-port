using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarx;

public class BookOfMagic : SpriteGui
{
	private CompositeDisposable _subscriptions;

	public SpriteButton bookmarkMagic;

	public SpriteButton bookmarkAchievments;

	public SpriteButton bookmarkBeasts;

	public ManaBar manaBar;

	public SpriteText skullsCount;

	public BoookOfMagicSpell[] Spells;

	public Transform TabMagic;

	public Transform TabAchievments;

	public Transform TabBeasts;

	public Transform LabelDark;

	public Transform LabelFire;

	public Transform LabelLight;

	public Transform LabelIce;

	public string assetPrefix = "book_of_magic";

	public string defaultLoc = "en";

	private void Awake()
	{
		string language = UnityApi.GetLanguage();
		if (language != defaultLoc)
		{
			TabMagic.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			TabAchievments.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			TabBeasts.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			LabelDark.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			LabelFire.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			LabelLight.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
			LabelIce.ChangeLocalizedTexture(assetPrefix, defaultLoc, language);
		}
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
		foreach (SpriteButton value in _buttons.Values)
		{
			value.SetActive();
		}
		bookmarkMagic.SetInactive();
		manaBar.Spell0.SetInactive();
		manaBar.Spell1.SetInactive();
		manaBar.Spell2.SetInactive();
		RegenerateAtlas();
		base.Release += ProcessButtons;
		Init();
	}

	private void Init()
	{
		skullsCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount.ToString();
		RefreshSpells();
	}

	private void ProcessButtons(SpriteButton button)
	{
		if (button.name.Contains("button_spell_"))
		{
			string s = button.name.Substring("button_spell_".Length);
			int num = int.Parse(s);
			ServerData.Spell next = GetNext(num);
			if (next == null && Globals.IsDebugBuild)
			{
				Debug.LogError("!!! we have problems with spell: " + button.name);
			}
			if (Globals.IsDebugBuild)
			{
				Debug.Log($"curr:{next} next:{GetNext(num)}");
			}
			int spellPrice = BoookOfMagicSpell.GetSpellPrice(next);
			if (spellPrice <= SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount)
			{
				SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount -= spellPrice;
				skullsCount.Text_ = SingletonT<ServerData>.I.PlayerParams.MoneySkullsCount.ToString();
				RemoveMySpell(num);
				SingletonT<ServerData>.I.MySpells.Add(GetSpellIdx(next));
				Spells[num].SetSpell(next, GetNext(num));
			}
		}
	}

	private void RemoveMySpell(int n)
	{
		string school = GetSchool(n);
		int level = GetLevel(n);
		ServerData.Spell spell = null;
		foreach (int mySpell in SingletonT<ServerData>.I.MySpells)
		{
			ServerData.Spell spell2 = SingletonT<ServerData>.I._spells[mySpell];
			if (spell2.Level == level && spell2.SchoolName == school)
			{
				spell = spell2;
				break;
			}
		}
		if (spell != null)
		{
			SingletonT<ServerData>.I.MySpells.Remove(spell.Id);
		}
	}

	private int GetSpellIdx(ServerData.Spell sp)
	{
		foreach (KeyValuePair<int, ServerData.Spell> spell in SingletonT<ServerData>.I._spells)
		{
			ServerData.Spell value = spell.Value;
			if (value.Level == sp.Level && value.SchoolName == sp.SchoolName && value.Title == sp.Title)
			{
				return spell.Key;
			}
		}
		return -1;
	}

	private void RefreshSpells()
	{
		for (int i = 0; i < Spells.Length; i++)
		{
			Spells[i].SetSpell(GetMyCurrentSpell(i), GetNext(i));
		}
	}

	private ServerData.Spell GetNext(int n)
	{
		ServerData.Spell myCurrentSpell = GetMyCurrentSpell(n);
		if (myCurrentSpell == null)
		{
			string school = GetSchool(n);
			int level = GetLevel(n);
			IOrderedEnumerable<ServerData.Spell> orderedEnumerable = from sp in SingletonT<ServerData>.I._spells.Values
				where sp.Level == level && sp.SchoolName == school
				orderby sp.PowerK
				select sp;
			IEnumerator<ServerData.Spell> enumerator = orderedEnumerable.GetEnumerator();
			return (orderedEnumerable.Count() <= 0) ? null : orderedEnumerable.First();
		}
		return myCurrentSpell.NextSpell;
	}

	private ServerData.Spell GetMyCurrentSpell(int n)
	{
		string school = GetSchool(n);
		int level = GetLevel(n);
		int num = SingletonT<ServerData>.I.MySpells.FindIndex(delegate(int spi)
		{
			ServerData.Spell spell = SingletonT<ServerData>.I._spells[spi];
			return spell.Level == level && spell.SchoolName == school;
		});
		return (num >= 0) ? SingletonT<ServerData>.I._spells[SingletonT<ServerData>.I.MySpells[num]] : null;
	}

	private string GetSchool(int n)
	{
		return DecodeSchool(n / 3).ToString();
	}

	private int GetLevel(int n)
	{
		return DecodeLevel(n % 3);
	}

	private int DecodeSchool(int n)
	{
		switch (n)
		{
		case 0:
			return 3;
		case 1:
			return 1;
		case 2:
			return 2;
		case 3:
			return 4;
		default:
			if (Globals.IsDebugBuild)
			{
				Debug.LogError("Wrong My spell type [0-3]: " + n);
			}
			return 1;
		}
	}

	private int DecodeLevel(int n)
	{
		return 3 - n;
	}

	private void Update()
	{
		ProcessRayCast();
	}
}
