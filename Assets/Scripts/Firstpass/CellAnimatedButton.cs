using System.Collections;
using UnityEngine;
using Yarx;

public class CellAnimatedButton : SpriteButton
{
	private CompositeDisposable _subscriptions;

	public LocationLogic.ChestOnLocation Chest;

	public int countH;

	public int countCells;

	public float oneFrameSec = 0.2f;

	public int cellW;

	public int cellH;

	public int texW;

	public int texH;

	public Vector2 shift = Vector2.zero;

	public bool PingPong = true;

	private Vector2 _shiftNorm;

	private float _uvx;

	private float _uvy;

	private Mesh _mesh;

	private void SetUv(int n)
	{
		if (PingPong)
		{
			n %= 2 * (countCells - 1);
			n = ((n >= countCells) ? (2 * (countCells - 1) - n) : n);
		}
		else
		{
			n %= countCells;
		}
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

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		StartCoroutine("Animate");
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
