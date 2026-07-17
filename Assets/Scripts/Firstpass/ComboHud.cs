using System.Collections;
using UnityEngine;
using Yarx;

public class ComboHud : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	private GameObject _hightlightPrefab;

	public SpriteText AttackNumber;

	public Sprite NextAttack;

	public Sprite ComboIndicator;

	private string _fmt = "attack_{0}_current_retina";

	public static string FrontAttk = "front";

	public static string LeftAttk = "right";

	public static string RightAttk = "left";

	public static string Combo = "all";

	private GameObject _highlight;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<AttackE, int, int>.AddListener(Globals.MsgGuiBattle_NextCombo, OnNextCombo));
		_subscriptions.Add(Messenger<bool>.AddListener(Globals.MsgGuiBattle_ComboAllowed, OnComboAllowed));
	}

	private void OnComboAllowed(bool allowed)
	{
		if (allowed)
		{
			NextAttack.SpriteName_ = _fmt.Fmt(Combo);
			ComboIndicator.ClipVertical(1f);
			AttackNumber.Text_ = string.Empty;
			_highlight = (GameObject)Object.Instantiate(_hightlightPrefab);
			_highlight.transform.parent = NextAttack.transform.parent;
			_highlight.transform.localPosition = new Vector3(0f, 0f, 50f);
			StartCoroutine("ShowHighlight", _highlight);
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

	private void OnNextCombo(AttackE attackE, int count, int max)
	{
		SetAttackNumber(count);
		SetNextAttack(attackE);
		SetComboCooling(count, max);
		if (_highlight != null)
		{
			GameObject highlight = _highlight;
			_highlight = null;
			StartCoroutine("KillHighlight", highlight);
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		_hightlightPrefab = Util.Resource<GameObject>("_z_prefabs/combo_button_highlight");
	}

	private void SetAttackNumber(int n)
	{
		AttackNumber.Text_ = string.Empty;
	}

	private void SetNextAttack(AttackE direction)
	{
		switch (direction)
		{
		case AttackE.Left:
			NextAttack.SpriteName_ = _fmt.Fmt(LeftAttk);
			break;
		case AttackE.Right:
			NextAttack.SpriteName_ = _fmt.Fmt(RightAttk);
			break;
		case AttackE.Forward:
			NextAttack.SpriteName_ = _fmt.Fmt(FrontAttk);
			break;
		default:
			Utils.Log("SetNextAttack unknown", direction);
			break;
		}
	}

	private void SetComboCooling(int current, int max)
	{
		float fraction = Mathf.Clamp01((float)current / (float)max);
		ComboIndicator.ClipVertical(fraction);
	}
}
