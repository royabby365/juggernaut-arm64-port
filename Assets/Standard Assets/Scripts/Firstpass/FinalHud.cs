using UnityEngine;
using Yarx;

public class FinalHud : MonoBehaviour
{
	private const float DefaultFriction = 4000f;

	private const float UnpenetrateTime = 0.3f;

	private CompositeDisposable _subscriptions;

	private bool _deceleration;

	private float _startTime;

	private float _stopTime;

	private Vector3 _startPos;

	private Vector3 _stopPos;

	private int _minScrollLoc;

	private int _maxScrollLoc;

	private float _scrollSpeed;

	public Transform ScrollRoot;

	public SpriteText MainText;

	public Sprite Icon;

	public BoxCollider Collider;

	public AnimationCurve DecelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Transform BgTop;

	public Transform BgTileRoot;

	public Transform BgBottom;

	private void Start()
	{
		ScrollRoot.localPosition = new Vector3(ScrollRoot.localPosition.x, Camera2D.ScreenHeight / 2 - 10, ScrollRoot.localPosition.z);
		_minScrollLoc = ScrollRoot.localPosition.y.RoundToInt();
		Init();
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
				Vector3 localPosition = Vector3.Lerp(_startPos, _stopPos, DecelerationCurve.Evaluate(num2 / num)).RoundToInt();
				ScrollRoot.localPosition = localPosition;
			}
			else
			{
				ScrollRoot.localPosition = _stopPos;
				_deceleration = false;
			}
		}
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.MoveBegin += GuiOnMoveBegin;
			HudMk1.Instance.MoveEnd += GuiOnMoveEnd;
			HudMk1.Instance.Move += GuiOnMove;
		}
	}

	private void OnDisable()
	{
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.MoveBegin -= GuiOnMoveBegin;
			HudMk1.Instance.MoveEnd -= GuiOnMoveEnd;
			HudMk1.Instance.Move -= GuiOnMove;
		}
		_subscriptions.Dispose();
	}

	private void GuiOnMoveBegin(Vector3 begin)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CheckCollider(Collider, begin))
		{
			_deceleration = false;
		}
	}

	private void GuiOnMoveEnd(Vector3 pos)
	{
		if (HudMk1.Instance == null || HudMk1.Instance.CurrentGui.Type != GuiRoot.GuiType.Final)
		{
			return;
		}
		Vector3 localPosition = ScrollRoot.localPosition;
		_startTime = Time.time;
		_startPos = localPosition;
		_deceleration = true;
		if (localPosition.y < (float)_minScrollLoc)
		{
			_stopPos = new Vector3(localPosition.x, _minScrollLoc, localPosition.z);
			_stopTime = _startTime + 0.3f;
		}
		else if (localPosition.y > (float)_maxScrollLoc)
		{
			_stopPos = new Vector3(localPosition.x, _maxScrollLoc, localPosition.z);
			_stopTime = _startTime + 0.3f;
		}
		else if (!_scrollSpeed.Eqv(0f))
		{
			float a = Mathf.Abs(_scrollSpeed);
			float num = Mathf.Sign(_scrollSpeed);
			a = Mathf.Min(a, 4000f);
			float num2 = ((!(num < 0f)) ? Mathf.Abs((float)_maxScrollLoc - localPosition.y) : Mathf.Abs(localPosition.y - (float)_minScrollLoc));
			float num3 = a / 4000f;
			float num4 = Mathf.Round(a * num3 - 4000f * num3 * num3 / 2f);
			if (num4 > num2)
			{
				num3 /= num4 / num2;
				num4 = num2;
			}
			_stopTime = _startTime + num3;
			_stopPos = localPosition + new Vector3(0f, num * num4, 0f);
		}
		else
		{
			_deceleration = false;
		}
		if (Globals.IsDebugBuild)
		{
			Debug.Log("======== min:{0} max:{1} speed:{2} start:{3} stop:{4}".Fmt(_minScrollLoc, _maxScrollLoc, _scrollSpeed, _startPos, _stopPos));
		}
		_scrollSpeed = 0f;
	}

	private void GuiOnMove(Vector3 begin, Vector3 end)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CheckCollider(Collider, begin))
		{
			float y = (end - begin).y;
			y *= Camera2D.Scale;
			_scrollSpeed = y / Time.deltaTime;
			Vector3 localPosition = ScrollRoot.localPosition;
			ScrollRoot.localPosition = new Vector3(localPosition.x, (localPosition.y + y).RoundToEven(), localPosition.z);
		}
	}

	private void OnMsgGuiSwitchToBefore(GuiRoot.GuiType old, GuiRoot.GuiType @new)
	{
	}

	private void Init()
	{
		int num = 96;
		float num2 = MainText.GetBounds().size.y + (float)num;
		Icon.transform.localPosition = new Vector3(Icon.transform.localPosition.x, 0f - num2 - 10f, Icon.transform.localPosition.z);
		_maxScrollLoc = _minScrollLoc + (num2 + (float)Icon.Height).RoundToInt() - (Camera2D.ScreenHeight - 30);
		int num3 = (num2 + (float)Icon.Height).RoundToInt();
		BgTop.localPosition = default(Vector3);
		int num4 = -num;
		if (num3 > 200)
		{
			int num5 = (num3 - 180) / num;
			if ((num3 - 180) % num > 0)
			{
				num5++;
			}
			for (int i = 0; i < num5; i++)
			{
				GameObject gameObject = new GameObject();
				Sprite sprite = gameObject.AddComponent<Sprite>();
				sprite.transform.SetLayerRecursively(BgTileRoot);
				sprite.Origin = Quad.OriginPlace.UpperCenter;
				sprite.SpriteName_ = "parchment_tile";
				sprite.VGutter = 1;
				sprite.Refresh();
				gameObject.transform.parent = BgTileRoot;
				gameObject.transform.localPosition = new Vector3(0f, num4, 0f);
				num4 -= num;
			}
		}
		BgBottom.localPosition = new Vector3(0f, num4, 0f);
	}
}
