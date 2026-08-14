using UnityEngine;
using Yarx;

public class ItemButton : SpriteButton
{
	private const float InactivePosZ = 1000f;

	private const string Resurrection = "resurrection";

	private const string Poison = "poisons";

	private const string Heal = "heals";

	private const string Crits = "crits";

	private int _frameCount;

	private readonly CompositeDisposable _disposables = new CompositeDisposable();

	public Transform pressed;

	public Transform icon;

	public Transform digits;

	public Transform cooldown;

	public bool IsInHell => base.transform.IsInHell();

	public override void SetActive()
	{
		base.SetActive();
		icon.gameObject.SetActive(true);
		cooldown.localScale = new Vector3(1f, 0f, 1f);
		base.transform.localPosition = Vector3.zero;
		base.transform.parent.GetComponent<ItemsBar>().RearrangePotions();
	}

	private void GoTohell()
	{
		base.transform.GoToHell();
		base.transform.parent.GetComponent<ItemsBar>().RearrangePotions();
	}

	public override void Left()
	{
		base.Left();
		pressed.gameObject.SetActive(false);
	}

	public override void Entered()
	{
		base.Entered();
		pressed.gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		InitCount();
		_disposables.Add(Messenger<ServerData.Item.ElixirTypeE, int, int>.AddListener(Globals.MsgElixirCooldownChanged, OnElixirCooldownChanged));
		_disposables.Add(Messenger<ServerData.Item.ElixirTypeE>.AddListener(Globals.MsgElixirCountChanged, OnElixirCountChanged));
		_disposables.Add(Messenger<int>.AddListener(Globals.MsgResurrectionCountChanged, OnResurrectionCountChanged));
		_disposables.Add(Messenger.AddListener(Globals.MsgFightStarted, OnStartFight));
	}

	private void OnStartFight()
	{
		InitCount();
	}

	private void OnDisable()
	{
		_disposables.Dispose();
	}

	private void Awake()
	{
		Init();
		pressed.gameObject.SetActive(false);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void InitCount()
	{
		int myItemCount = 0;
		if (base.name == "heals")
		{
			myItemCount = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Heal);
		}
		else if (base.name == "crits")
		{
			myItemCount = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Critical);
		}
		else if (base.name == "poisons")
		{
			myItemCount = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Poison);
		}
		SetMyItemCount(myItemCount);
	}

	private void OnItemAdded(ServerData.Item item)
	{
		OnItemChanged();
	}

	private void HandlerOnItemRemoved(ServerData.Item item)
	{
		OnItemChanged();
	}

	private void OnItemChanged()
	{
		int num = -1;
		if (base.name == "heals")
		{
			num = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Heal);
		}
		else if (base.name == "crits")
		{
			num = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Critical);
		}
		else if (base.name == "poisons")
		{
			num = SingletonT<ServerData>.I.GetPlayerElixirsCount(ServerData.Item.ElixirTypeE.Poison);
		}
		if (num >= 0)
		{
			SetMyItemCount(num);
		}
	}

	private void OnElixirCountChanged(ServerData.Item.ElixirTypeE type)
	{
		if (base.name == "heals" && type == ServerData.Item.ElixirTypeE.Heal)
		{
			SetMyItemCount(type.GetElixirCount());
		}
		else if (base.name == "crits" && type == ServerData.Item.ElixirTypeE.Critical)
		{
			SetMyItemCount(type.GetElixirCount());
		}
		else if (base.name == "poisons" && type == ServerData.Item.ElixirTypeE.Poison)
		{
			SetMyItemCount(type.GetElixirCount());
		}
	}

	private void OnResurrectionCountChanged(int count)
	{
		if (base.name == "resurrection")
		{
			digits.parent.gameObject.SetActiveRecursivelyMk1(setActive: false);
			SetMyItemCount(count);
		}
	}

	private void SetMyItemCount(int count)
	{
		digits.GetComponent<ItemDigits>().SetItemDigits(count);
		if (count < 1)
		{
			GoTohell();
		}
		else
		{
			SetActive();
		}
	}

	private void OnElixirCooldownChanged(ServerData.Item.ElixirTypeE type, int current, int max)
	{
		Utils.Log("EXLIXIRCOOLDOWN", type, current, max);
		if (base.name == "heals" && type == ServerData.Item.ElixirTypeE.Heal)
		{
			SetMyCooldown(current, max);
		}
		else if (base.name == "crits" && type == ServerData.Item.ElixirTypeE.Critical)
		{
			SetMyCooldown(current, max);
		}
		else if (base.name == "poisons" && type == ServerData.Item.ElixirTypeE.Poison)
		{
			SetMyCooldown(current, max);
		}
	}

	private void SetMyCooldown(int current, int max)
	{
		if (current == 0)
		{
			SetActive();
		}
		cooldown.localScale = new Vector3(1f, (max <= 0) ? 0f : ((float)current / (float)max), 1f);
	}
}
