using System.Collections;
using UnityEngine;
using Yarx;

public class CellAnimation : MonoBehaviour
{
	private CompositeDisposable _subscriptions;

	public int countH;

	public int countCells;

	public float oneFrameSec = 0.1f;

	public int cellW;

	public int cellH;

	public int texW;

	public int texH;

	public Vector2 shift = Vector2.zero;

	public bool Colorize;

	public Color tint;

	public Color[] tints;

	private Vector2 _shiftNorm;

	private float _uvx;

	private float _uvy;

	private Mesh _mesh;

	private void SetUv(int n)
	{
		n %= countCells;
		int num = n % countH;
		int num2 = n / countH;
		if (_mesh == null)
		{
			_mesh = base.transform.GetComponent<MeshFilter>().mesh;
		}
		_mesh.SetUV(new Rect((float)num * _uvx + _shiftNorm.x, (float)num2 * _uvy + _shiftNorm.y, _uvx, _uvy));
	}

	private void Awake()
	{
		_uvx = (float)cellW / (float)texW;
		_uvy = (float)cellH / (float)texH;
		_shiftNorm = new Vector2(shift.x / (float)texW, shift.y / (float)texH);
		SetUv(0);
	}

	private void DoSetTint()
	{
		if (Colorize)
		{
			_mesh.SetTint(tint);
		}
	}

	private IEnumerator OnBecameVisible()
	{
		for (int i = 0; i < 2; i++)
		{
			yield return null;
			DoSetTint();
		}
	}

	private void OnBecameInvisible()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		StartCoroutine("Animate");
		_subscriptions.Add(Messenger<int>.AddListener(Globals.MsgShamansCountChanged, Handler));
	}

	private void Handler(int i)
	{
		if (base.name.Contains("spell_cell"))
		{
			int a = Mathf.Min(i, tints.Length);
			a = Mathf.Max(a, 0);
			if (i != a && Globals.IsDebugBuild)
			{
				Debug.LogWarning("Shamans changed:{0} and colors count {1} mismatch".Fmt(i, tints.Length));
			}
			tint = ((tints.Length <= 0) ? tint : tints[a]);
			DoSetTint();
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		_subscriptions.Dispose();
	}

	private IEnumerator Animate()
	{
		int n = 0;
		while (true)
		{
			SetUv(n++);
			yield return new WaitForSeconds(oneFrameSec);
		}
	}
}
