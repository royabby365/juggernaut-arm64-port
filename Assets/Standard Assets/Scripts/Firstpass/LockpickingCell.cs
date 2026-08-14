using System.Collections;
using UnityEngine;

public class LockpickingCell : SpriteButton
{
	public enum StateE
	{
		Showed,
		Hidden,
		Inactive,
		Opened
	}

	private int _cellIdx;

	private float _fadeInOutTick;

	private AnimationCurve _outCurve;

	private AnimationCurve _inCurveLean;

	private AnimationCurve _inCurve;

	private AnimationCurve _outToZeroCurve;

	public Sprite CellIco;

	public Sprite Halo;

	public Transform Bg;

	public StateE State = StateE.Inactive;

	private Color Green = Color.green;

	private Color Orange = new Color(1f, 0.5f, 0f, 1f);

	private Color Red = Color.red;

	public int CellIdx => _cellIdx;

	private void Awake()
	{
		SetIcon(0);
		_inCurve = base.transform.root.GetComponentInChildren<LockpickingHud>().InCurve;
		_inCurveLean = base.transform.root.GetComponentInChildren<LockpickingHud>().InCurveLean;
		_outCurve = base.transform.root.GetComponentInChildren<LockpickingHud>().OutCurve;
		_outToZeroCurve = base.transform.root.GetComponentInChildren<LockpickingHud>().OutToZero;
		_fadeInOutTick = base.transform.root.GetComponentInChildren<LockpickingHud>().FadeInOutTick;
		SetClear();
		Init();
	}

	private void Start()
	{
	}

	public void SetClear()
	{
		Halo.Tint_ = new Color(1f, 1f, 1f, 0f);
	}

	internal void SetCell(int iconN)
	{
		_cellIdx = iconN;
		State = StateE.Hidden;
		Close();
	}

	public void SetGreen()
	{
		Halo.Tint_ = Green;
	}

	public void SetOrange()
	{
		Halo.Tint_ = Orange;
	}

	public void SetRed()
	{
		Halo.Tint_ = Red;
	}

	public void Show()
	{
		if (State != StateE.Opened)
		{
			if (State == StateE.Hidden)
			{
				Open();
			}
			State = StateE.Showed;
		}
	}

	public void Hide()
	{
		if (State != StateE.Opened)
		{
			SetActive();
			if (State == StateE.Showed)
			{
				Close();
			}
			State = StateE.Hidden;
		}
	}

	public void OpenForever()
	{
		SetInactive();
		if (State == StateE.Hidden)
		{
			Open();
		}
		State = StateE.Opened;
	}

	private void Open()
	{
		StartCoroutine("OpenCoro");
	}

	private void Close()
	{
		StartCoroutine("CloseCoro");
	}

	public void CloseForever(float sec)
	{
		StartCoroutine("CloseForeverCoro", sec);
	}

	private void SetIcon(int index)
	{
		if (index == 0)
		{
			CellIco.SpriteName_ = "closed";
		}
		else
		{
			CellIco.SpriteName_ = $"item_{index:00}";
		}
	}

	private IEnumerator OpenCoro()
	{
		float start = Time.time;
		while (Time.time < start + _fadeInOutTick)
		{
			float d = _outCurve.Evaluate((Time.time - start) / _fadeInOutTick);
			CellIco.transform.localScale = new Vector3(d, d, 1f);
			yield return null;
		}
		SetIcon(_cellIdx);
		start = Time.time;
		while (Time.time < start + _fadeInOutTick)
		{
			float d2 = _inCurve.Evaluate((Time.time - start) / _fadeInOutTick);
			CellIco.transform.localScale = new Vector3(d2, d2, 1f);
			yield return null;
		}
		CellIco.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	private IEnumerator CloseCoro()
	{
		float start = Time.time;
		while (Time.time < start + _fadeInOutTick)
		{
			float d = _outCurve.Evaluate((Time.time - start) / _fadeInOutTick);
			CellIco.transform.localScale = new Vector3(d, d, 1f);
			yield return null;
		}
		SetIcon(0);
		start = Time.time;
		while (Time.time < start + _fadeInOutTick)
		{
			float d2 = _inCurveLean.Evaluate((Time.time - start) / _fadeInOutTick);
			CellIco.transform.localScale = new Vector3(d2, d2, 1f);
			yield return null;
		}
		CellIco.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	private IEnumerator CloseForeverCoro(float sec)
	{
		float start = Time.time;
		while (Time.time < start + sec)
		{
			float d = _outToZeroCurve.Evaluate((Time.time - start) / _fadeInOutTick);
			CellIco.transform.localScale = new Vector3(d, d, 1f);
			yield return null;
		}
	}
}
