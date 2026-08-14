using System.Collections;
using UnityEngine;
using Yarx;
using Yarx.Collections;

public class RageBarHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private GameObject _highlightPrefab;

	private GameObject _highlight1;

	private GameObject _highlight2;

	private GameObject _highlight3;

	private int _prevCount;

	public BattleHudButton Rage1;

	public BattleHudButton Rage2;

	public BattleHudButton Rage3;

	public Sprite[] RageBalls;

	public Color RageBallsInactive = new Color32(120, 120, 120, 33);

	public Color RageBallsInactiveHalf = new Color32(120, 120, 120, 16);

	public Color RageBallsHalf = new Color32(128, 128, 128, 64);

	private Color RageBallsFull = Color.gray;

	private System.Tuple<bool, bool, bool> _sbuttonsState;

	private GuiRoot.GuiType _currentGuiState = GuiRoot.GuiType.None;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgRageSpheresCountChanged, OnRageSpheresChanged));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnGuiChanged));
		if (HudMk1.Instance != null)
		{
			HudMk1.Instance.Release += OnRageButtonRelease;
			HudMk1.Instance.DragEndWithButton += OnRageButtonRelease;
		}
	}

	private void OnRageButtonRelease(SpriteButton spriteButton)
	{
		if (!Globals.ForceDontProcessRageButtons && spriteButton.name.Contains("use_rage"))
		{
			Rage1.SetSelected();
			Rage2.SetSelected();
			Rage3.SetSelected();
		}
	}

	private void OnGuiChanged(GuiRoot.GuiType from, GuiRoot.GuiType to)
	{
		_currentGuiState = to;
		if (to == GuiRoot.GuiType.BattleHud)
		{
			SetBattleAlpha();
		}
		if (to == GuiRoot.GuiType.EnemyTurn)
		{
			SetHalfAlpha();
		}
	}

	private void SetHalfAlpha()
	{
		SetRageballs(_prevCount);
		Rage1.SetSelected();
		Rage2.SetSelected();
		Rage3.SetSelected();
	}

	private void SetBattleAlpha()
	{
		SetRageballs(_prevCount);
		Rage1.SetUnselected();
		Rage2.SetUnselected();
		Rage3.SetUnselected();
	}

	private void OnRageSpheresChanged(int count)
	{
		if (count >= 1)
		{
			Rage1.SetActive();
			if (_highlight1 == null && _prevCount < 1)
			{
				_highlight1 = (GameObject)Object.Instantiate(_highlightPrefab);
				_highlight1.transform.parent = Rage1.transform;
				_highlight1.transform.localPosition = new Vector3(0f, 0f, 50f);
				StartCoroutine("ShowHighlight", _highlight1);
			}
		}
		else
		{
			Rage1.SetInactive();
		}
		if (count >= 4)
		{
			Rage2.SetActive();
			if (_highlight2 == null && _prevCount < 4)
			{
				_highlight2 = (GameObject)Object.Instantiate(_highlightPrefab);
				_highlight2.transform.parent = Rage2.transform;
				_highlight2.transform.localPosition = new Vector3(0f, 0f, 50f);
				StartCoroutine("ShowHighlight", _highlight2);
			}
		}
		else
		{
			Rage2.SetInactive();
		}
		if (count >= 10)
		{
			Rage3.SetActive();
			if (_highlight3 == null && _prevCount < 10)
			{
				_highlight3 = (GameObject)Object.Instantiate(_highlightPrefab);
				_highlight3.transform.parent = Rage3.transform;
				_highlight3.transform.localPosition = new Vector3(0f, 0f, 50f);
				StartCoroutine("ShowHighlight", _highlight3);
			}
		}
		else
		{
			Rage3.SetInactive();
		}
		SetRageballs(count);
		_prevCount = count;
	}

	private IEnumerator ShowHighlight(GameObject highlight)
	{
		float time = 0f;
		Vector3 scale = Vector3.one * 200f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(Vector3.zero, scale, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = scale;
		yield return new WaitForSeconds(2f);
		time = 0f;
		while (time < 0.5f)
		{
			highlight.transform.localScale = Vector3.Lerp(scale, Vector3.zero, time / 0.5f);
			time += Time.deltaTime;
			yield return null;
		}
		highlight.transform.localScale = Vector3.zero;
		Object.Destroy(highlight);
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		_highlightPrefab = Util.Resource<GameObject>("_z_prefabs/combo_button_highlight");
	}

	private void SetRageballs(int current)
	{
		for (int i = 0; i < RageBalls.Length; i++)
		{
			if (_currentGuiState != GuiRoot.GuiType.BattleHud)
			{
				RageBalls[i].Tint_ = ((i >= current) ? RageBallsInactiveHalf : RageBallsHalf);
			}
			else
			{
				RageBalls[i].Tint_ = ((i >= current) ? RageBallsInactive : RageBallsFull);
			}
		}
	}
}
