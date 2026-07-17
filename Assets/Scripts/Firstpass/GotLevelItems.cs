using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarx;

public class GotLevelItems : MonoBehaviour
{
	private const int ShopSlotsPitch = 300;

	private const float MaxSpeed = 4000f;

	private const float DefaultFriction = 4000f;

	private const float UnpenetrateTime = 0.4f;

	private const float LongestTime = 2f;

	private CompositeDisposable _subscriptions;

	public GameObject ShopItemProto;

	public Transform ItemsRoot;

	public Collider SliceCollider;

	public Transform RightVertical;

	private readonly List<LevelItem> _shopList = new List<LevelItem>();

	private int _newRootPosition;

	public AnimationCurve DecelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private int _minScrollLoc;

	private int _maxScrollLoc;

	private float _scrollSpeed;

	private Vector3 _scrollBegin;

	private bool _deceleration;

	private float _startTime;

	private float _stopTime;

	private Vector3 _startPos;

	private Vector3 _stopPos;

	private SpriteGui _gui;

	private void Awake()
	{
		_gui = base.transform.GetSpriteGui();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int, int, string>.AddListener(Globals.MsgPlayerLevelChanged, OnPlayerLevelChanged));
		_gui.MoveBegin += GuiOnMoveBegin;
		_gui.MoveEnd += GuiOnMoveEnd;
		_gui.Move += GuiOnMove;
	}

	private void OnDisable()
	{
		_gui.MoveBegin -= GuiOnMoveBegin;
		_gui.MoveEnd -= GuiOnMoveEnd;
		_gui.Move -= GuiOnMove;
		_subscriptions.Dispose();
	}

	private void OnBagRefreshFinished()
	{
		foreach (LevelItem shop in _shopList)
		{
			shop.UpdateCompare();
		}
	}

	private void GuiOnMove(Vector3 begin, Vector3 end)
	{
		if (!(_gui == null) && _gui.CheckCollider(SliceCollider, begin))
		{
			float x = (end - begin).x;
			x *= Camera2D.Scale;
			_scrollSpeed = x / Time.deltaTime;
			Vector3 localPosition = ItemsRoot.localPosition;
			ItemsRoot.localPosition = new Vector3(localPosition.x + (float)x.RoundToInt(), localPosition.y, localPosition.z);
		}
	}

	private void GuiOnMoveEnd(Vector3 vector3)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.GotLevelNewItems)
		{
			return;
		}
		Vector3 localPosition = ItemsRoot.localPosition;
		_startTime = Time.time;
		_startPos = localPosition;
		_deceleration = true;
		if (localPosition.x < (float)_minScrollLoc)
		{
			_stopPos = new Vector3(_minScrollLoc, localPosition.y, localPosition.z);
			_stopTime = _startTime + 0.4f;
		}
		else if (localPosition.x > (float)_maxScrollLoc)
		{
			_stopPos = new Vector3(_maxScrollLoc, localPosition.y, localPosition.z);
			_stopTime = _startTime + 0.4f;
		}
		else if (!_scrollSpeed.Eqv(0f))
		{
			float a = Mathf.Abs(_scrollSpeed);
			float num = Mathf.Sign(_scrollSpeed);
			a = Mathf.Min(a, 4000f);
			float num2 = ((!(num < 0f)) ? Mathf.Abs((float)_maxScrollLoc - localPosition.x) : Mathf.Abs(localPosition.x - (float)_minScrollLoc));
			float num3 = a / 4000f;
			float num4 = Mathf.Round(a * num3 - 4000f * num3 * num3 / 2f);
			if (num4 > num2)
			{
				num3 /= num4 / num2;
				num4 = num2;
			}
			_stopTime = _startTime + num3;
			_stopPos = localPosition + new Vector3(num * num4, 0f, 0f);
		}
		else
		{
			_deceleration = false;
		}
		_scrollSpeed = 0f;
	}

	private void GuiOnMoveBegin(Vector3 begin)
	{
		if (!(_gui == null) && _gui.CheckCollider(SliceCollider, begin))
		{
			_deceleration = false;
		}
	}

	private void Update()
	{
		if (_deceleration)
		{
			float time = Time.time;
			if (time <= _stopTime)
			{
				float num = _stopTime - _startTime;
				float num2 = time - _startTime;
				ItemsRoot.localPosition = Vector3.Lerp(_startPos, _stopPos, DecelerationCurve.Evaluate(num2 / num));
			}
			else
			{
				ItemsRoot.localPosition = _stopPos;
				_deceleration = false;
			}
		}
	}

	private void UpdateMinMax(int count)
	{
		_minScrollLoc = -count * 300 + Camera2D.ScreenWidth;
		_maxScrollLoc = 0;
		if (count * 300 < Camera2D.ScreenWidth)
		{
			int num = (Camera2D.ScreenWidth - count * 300) / 2;
			_maxScrollLoc += num;
			_minScrollLoc -= num;
			Vector3 localPosition = ItemsRoot.localPosition;
			ItemsRoot.localPosition = new Vector3(_minScrollLoc, localPosition.y, localPosition.z);
		}
	}

	private void Start()
	{
	}

	public void Init()
	{
		InitShopGoodsForLevel(SingletonT<ServerData>.I.PlayerParams.Level - 1, SingletonT<ServerData>.I.PlayerParams.Level);
	}

	private void OnPlayerLevelChanged(int old, int @new, string reason)
	{
		InitShopGoodsForLevel(old, @new);
	}

	private void InitShopGoodsForLevel(int old, int @new)
	{
		if (HudMk1.Instance == null)
		{
			return;
		}
		CleanShop();
		List<ServerData.ShopGood> diffList = GetDiffList(old, @new);
		diffList.Sort((ServerData.ShopGood l, ServerData.ShopGood r) => ServerData.ShopGoodPriority(l).CompareTo(ServerData.ShopGoodPriority(r)));
		foreach (ServerData.ShopGood item in diffList)
		{
			AddOneShopGoodItem(item);
		}
		RefreshShopGoodItems();
	}

	private void RefreshShopGoodItems()
	{
		int num = 0;
		foreach (LevelItem shop in _shopList)
		{
			int num2 = num++ * 300;
			shop.transform.localPosition = Vector3.zero;
			shop.transform.localPosition += num2 * Vector3.right;
		}
		UpdateMinMax(num);
		RightVertical.localPosition = new Vector3(num * 300, 0f, 0f);
	}

	private void AddOneShopGoodItem(ServerData.ShopGood inShopGood)
	{
		LevelItem levelItem = Utils.Instaniate<LevelItem>(ShopItemProto);
		_shopList.Add(levelItem);
		levelItem.transform.parent = base.transform;
		levelItem.transform.SetLayerRecursively(base.transform);
		levelItem.ShopGood = inShopGood;
	}

	private void CleanShop()
	{
		foreach (LevelItem shop in _shopList)
		{
			shop.Eliminate();
		}
		_shopList.Clear();
	}

	public static List<ServerData.ShopGood> GetDiffList(int old, int @new)
	{
		List<ServerData.ShopGood> list = new List<ServerData.ShopGood>(SingletonT<ServerData>.I.GetShopGoodsNew(old + 1, @new).ToArray());
		ServerData.ShopGood[] array = list.ToArray();
		foreach (ServerData.ShopGood shopGood in array)
		{
			if (shopGood.Item.IsElixirType() && shopGood.GetPrice(ServerData.MoneyType.TypeE.Diamond) > 0)
			{
				list.Remove(shopGood);
			}
		}
		return list;
	}
}
