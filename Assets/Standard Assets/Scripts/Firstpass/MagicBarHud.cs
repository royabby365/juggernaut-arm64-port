using System.Collections;
using UnityEngine;
using Yarx;

public class MagicBarHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private GameObject _highlightPrefab;

	private GameObject _highlight;

	public SpriteButton MagicUseButton;

	public Sprite[] ManaBalls;

	public Color BallsInactive = new Color32(63, 63, 63, 63);

	public Color BallsInactiveHalf = new Color32(63, 63, 63, 16);

	public Color BallsActive = Color.gray;

	private Color BallsActiveHalf = new Color32(128, 128, 128, 64);

	private GuiRoot.GuiType _currentGuiState = GuiRoot.GuiType.None;

	private int _prevMana;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<Player>.AddListener(Globals.MsgPersonManaChanged, PersonManaChangend));
		_subscriptions.Add(Messenger<GuiRoot.GuiType, GuiRoot.GuiType>.AddListener(Globals.MsgGuiSwitchToPre, OnGuiChanged));
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
		SetManaBalls();
		MagicUseButton.SetSelected();
	}

	private void SetBattleAlpha()
	{
		SetManaBalls();
		MagicUseButton.SetUnselected();
	}

	private void PersonManaChangend(Player p)
	{
		int mana = p.Mana;
		if (_prevMana >= 10 && mana >= 10)
		{
			return;
		}
		_prevMana = mana;
		if (mana >= 10)
		{
			MagicUseButton.SetActive();
			if (_highlight == null)
			{
				_highlight = (GameObject)Object.Instantiate(_highlightPrefab);
				_highlight.transform.parent = MagicUseButton.transform;
				_highlight.transform.localPosition = new Vector3(0f, 0f, 50f);
				StartCoroutine("ShowHighlightOnce", _highlight);
			}
		}
		else
		{
			MagicUseButton.SetInactive();
		}
		SetManaBalls();
	}

	private IEnumerator ShowHighlightOnce(GameObject highlight)
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

	private void SetManaBalls()
	{
		for (int i = 0; i < ManaBalls.Length; i++)
		{
			if (_currentGuiState == GuiRoot.GuiType.BattleHud)
			{
				ManaBalls[i].Tint_ = ((i >= _prevMana) ? BallsInactive : BallsActive);
			}
			else
			{
				ManaBalls[i].Tint_ = ((i >= _prevMana) ? BallsInactiveHalf : BallsActiveHalf);
			}
		}
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
	}

	private IEnumerator KillHighlight(GameObject highlight)
	{
		float time = 0f;
		Vector3 scale = Vector3.one * 200f;
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
		_highlightPrefab = Util.Resource<GameObject>("_z_prefabs/magic_highlite_button");
	}
}
