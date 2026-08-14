using System.Collections;
using UnityEngine;
using Yarx;

public class LockCell : SpriteButton
{
	public enum StateE
	{
		Showed,
		Hidden,
		Inactive,
		Opened
	}

	private const int CellH = 86;

	private const int CellW = 86;

	private const int CellBgH = 100;

	private const int CellBgW = 100;

	private const float TexSq = 512f;

	private CompositeDisposable _subscriptions;

	public Transform CellIco;

	public Transform Bg;

	public Transform Halo;

	private Color NormalColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private Color OverColor = new Color(0.8f, 0.8f, 0.8f, 1f);

	public Color InactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);

	private Color Green = Color.green;

	private Color Orange = new Color(1f, 0.5f, 0f, 1f);

	private Color Red = Color.red;

	public StateE State = StateE.Inactive;

	private static readonly Vector2 CellBgZero = new Vector2(400f, 400f);

	private int _cellIdx;

	private float _fadeInOutTick;

	private AnimationCurve _inCurve;

	private AnimationCurve _inCurveLean;

	private AnimationCurve _outCurve;

	private AnimationCurve _outToZeroCurve;

	public int CellIdx => _cellIdx;

	public void SetCell(int n)
	{
		_cellIdx = n;
		State = StateE.Hidden;
		Close();
		SetBg();
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

	private IEnumerator OpenCoro()
	{
		float start = Time.time;
		while (Time.time < start + _fadeInOutTick)
		{
			float d = _outCurve.Evaluate((Time.time - start) / _fadeInOutTick);
			CellIco.transform.localScale = new Vector3(d, d, 1f);
			yield return null;
		}
		SetUv(_cellIdx);
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
		SetUv(0);
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
			Bg.transform.localScale = new Vector3(d, d, 1f);
			yield return null;
		}
	}

	public override void Entered()
	{
		base.Entered();
	}

	public override void Left()
	{
		base.Left();
	}

	public override void SetSelected()
	{
		base.SetSelected();
	}

	public override void SetUnselected()
	{
		base.SetUnselected();
	}

	public void SetGreen()
	{
		Halo.SetTint(Green);
	}

	public void SetOrange()
	{
		Halo.SetTint(Orange);
	}

	public void SetRed()
	{
		Halo.SetTint(Red);
	}

	public void SetClear()
	{
		Halo.SetTint(new Color(1f, 1f, 1f, 0f));
	}

	private void Awake()
	{
		SetUv(0);
		_inCurve = base.transform.root.GetComponent<Lockpicking>().InCurve;
		_inCurveLean = base.transform.root.GetComponent<Lockpicking>().InCurveLean;
		_outCurve = base.transform.root.GetComponent<Lockpicking>().OutCurve;
		_outToZeroCurve = base.transform.root.GetComponent<Lockpicking>().OutToZero;
		_fadeInOutTick = base.transform.root.GetComponent<Lockpicking>().FadeInOutTick;
		SetClear();
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		StopAllCoroutines();
	}

	private void SetUv(int n)
	{
		Mesh mesh = CellIco.GetComponent<MeshFilter>().mesh;
		mesh.SetUV(GetUvRect(n));
	}

	public Rect GetUvRect()
	{
		return GetUvRect(CellIdx);
	}

	private Rect GetUvRect(int n)
	{
		n = ((n < 27) ? n : (n + 3));
		if (n > 33 && Globals.IsDebugBuild)
		{
			Debug.LogError("Out of index max 33, got: " + n);
		}
		int num = n % 6;
		int num2 = n / 6;
		float left = (float)(num * 85) / 512f;
		float top = (float)(num2 * 85) / 512f;
		return new Rect(new Rect(left, top, 0.16796875f, 0.16796875f));
	}

	private void SetBg()
	{
		Vector2 cellBgZero = CellBgZero;
		float left = cellBgZero.x / 512f;
		Vector2 cellBgZero2 = CellBgZero;
		float top = cellBgZero2.y / 512f;
		Mesh mesh = Bg.GetComponent<MeshFilter>().mesh;
		mesh.SetUV(new Rect(left, top, 25f / 128f, 25f / 128f));
	}
}
