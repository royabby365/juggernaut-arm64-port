using System;
using System.Collections;
using UnityEngine;
using Yarx;

public class ItemsHud : MonoBehaviour
{
	private const int Pitch = 90;

	private CompositeDisposable _subscriptions;

	private Sprite[] _iconSprites;

	private GameObject _highlightPrefab;

	private GameObject _highlightHeal;

	private GameObject _highlightPoison;

	private GameObject _highlightCrit;

	public Sprite Life;

	public SpriteText LifeCount;

	public Sprite Crit;

	public SpriteText CritCount;

	public Sprite Poison;

	public SpriteText PoisonCount;

	private void Awake()
	{
		_iconSprites = new Sprite[3] { Life, Crit, Poison };
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.Item.ElixirTypeE, int, int>.AddListener(Globals.MsgElixirCooldownChanged, OnElixirCooldownChanged));
		_subscriptions.Add(Messenger<ServerData.Item.ElixirTypeE, int>.AddListener(Globals.MsgElixirCountChanged, OnElixirCountChanged));
	}

	private void OnElixirCountChanged(ServerData.Item.ElixirTypeE elixirTypeE, int count)
	{
		int num = ((count <= 0) ? 2048 : 0);
		switch (elixirTypeE)
		{
		case ServerData.Item.ElixirTypeE.Heal:
		{
			Vector3 localPosition3 = Life.transform.parent.transform.localPosition;
			Life.transform.parent.transform.localPosition = new Vector3(num, localPosition3.y, localPosition3.z);
			LifeCount.Text_ = count.ToString();
			break;
		}
		case ServerData.Item.ElixirTypeE.Critical:
		{
			Vector3 localPosition2 = Crit.transform.parent.transform.localPosition;
			Crit.transform.parent.transform.localPosition = new Vector3(num, localPosition2.y, localPosition2.z);
			CritCount.Text_ = count.ToString();
			break;
		}
		case ServerData.Item.ElixirTypeE.Poison:
		{
			Vector3 localPosition = Poison.transform.parent.transform.localPosition;
			Poison.transform.parent.transform.localPosition = new Vector3(num, localPosition.y, localPosition.z);
			PoisonCount.Text_ = count.ToString();
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("elixirTypeE");
		case ServerData.Item.ElixirTypeE.None:
		case ServerData.Item.ElixirTypeE.Key:
		case ServerData.Item.ElixirTypeE.Skull:
		case ServerData.Item.ElixirTypeE.Scarab:
			break;
		}
		RearrangeIcons();
	}

	private void RearrangeIcons()
	{
		int num = 0;
		Sprite[] iconSprites = _iconSprites;
		foreach (Sprite sprite in iconSprites)
		{
			if (sprite.transform.parent.transform.localPosition.x.Eqv(0f))
			{
				num++;
			}
		}
		float num2 = 0f - ((float)(num * 90) / 2f - 45f);
		Sprite[] iconSprites2 = _iconSprites;
		foreach (Sprite sprite2 in iconSprites2)
		{
			if (sprite2.transform.parent.transform.localPosition.x.Eqv(0f))
			{
				Vector3 localPosition = sprite2.transform.parent.transform.localPosition;
				sprite2.transform.parent.transform.localPosition = new Vector3(localPosition.x, num2, localPosition.z);
				num2 += 90f;
			}
		}
	}

	private void OnElixirCooldownChanged(ServerData.Item.ElixirTypeE elixirTypeE, int current, int max)
	{
		float num = Mathf.Clamp01((float)current / (float)max);
		num = ((current == 0) ? 0f : ((current != max) ? (1f - num) : 1f));
		switch (elixirTypeE)
		{
		case ServerData.Item.ElixirTypeE.None:
			break;
		case ServerData.Item.ElixirTypeE.Heal:
			Life.ClipVertical(num);
			if (num == 1f)
			{
				_highlightHeal = (GameObject)UnityEngine.Object.Instantiate(_highlightPrefab);
				_highlightHeal.transform.parent = Life.transform;
				_highlightHeal.transform.localPosition = new Vector3(0f, Life.Height / 2, 50f);
				StartCoroutine("ShowHighlight", _highlightHeal);
			}
			break;
		case ServerData.Item.ElixirTypeE.Critical:
			Crit.ClipVertical(num);
			if (num == 1f)
			{
				_highlightCrit = (GameObject)UnityEngine.Object.Instantiate(_highlightPrefab);
				_highlightCrit.transform.parent = Crit.transform;
				_highlightCrit.transform.localPosition = new Vector3(0f, Crit.Height / 2, 50f);
				StartCoroutine("ShowHighlight", _highlightCrit);
			}
			break;
		case ServerData.Item.ElixirTypeE.Poison:
			Poison.ClipVertical(num);
			if (num == 1f)
			{
				_highlightPoison = (GameObject)UnityEngine.Object.Instantiate(_highlightPrefab);
				_highlightPoison.transform.parent = Poison.transform;
				_highlightPoison.transform.localPosition = new Vector3(0f, Poison.Height / 2, 50f);
				StartCoroutine("ShowHighlight", _highlightPoison);
			}
			break;
		case ServerData.Item.ElixirTypeE.Key:
		case ServerData.Item.ElixirTypeE.Skull:
		case ServerData.Item.ElixirTypeE.Scarab:
			break;
		default:
			throw new ArgumentOutOfRangeException("elixirTypeE");
		}
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
		_highlightPrefab = Util.Resource<GameObject>("_z_prefabs/combo_button_highlight");
	}

	private IEnumerator ShowHighlight(GameObject highlight)
	{
		float time = 0f;
		Vector3 scale = Vector3.one * 150f;
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
		UnityEngine.Object.Destroy(highlight);
	}
}
