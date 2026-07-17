using System;
using UnityEngine;
using Yarx;

public class TutorialArrow : MonoBehaviour
{
	public enum Direction
	{
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW
	}

	private CompositeDisposable _subscriptions;

	public Sprite Arrow;

	private string _prefix = "tut_arrow_";

	private string _suffixN = "n";

	private string _suffixW = "w";

	private string _suffixNW = "nw";

	public void SetDirection(Direction dir)
	{
		switch (dir)
		{
		case Direction.N:
			Arrow.QuadMirror = Quad.Mirror.None;
			Arrow.SpriteName_ = _prefix + _suffixN;
			break;
		case Direction.NE:
			Arrow.QuadMirror = Quad.Mirror.Horizontal;
			Arrow.SpriteName_ = _prefix + _suffixNW;
			break;
		case Direction.E:
			Arrow.QuadMirror = Quad.Mirror.Horizontal;
			Arrow.SpriteName_ = _prefix + _suffixW;
			break;
		case Direction.SE:
			Arrow.QuadMirror = Quad.Mirror.Both;
			Arrow.SpriteName_ = _prefix + _suffixNW;
			break;
		case Direction.S:
			Arrow.QuadMirror = Quad.Mirror.Vertical;
			Arrow.SpriteName_ = _prefix + _suffixN;
			break;
		case Direction.SW:
			Arrow.QuadMirror = Quad.Mirror.Vertical;
			Arrow.SpriteName_ = _prefix + _suffixNW;
			break;
		case Direction.W:
			Arrow.QuadMirror = Quad.Mirror.None;
			Arrow.SpriteName_ = _prefix + _suffixW;
			break;
		case Direction.NW:
			Arrow.QuadMirror = Quad.Mirror.None;
			Arrow.SpriteName_ = _prefix + _suffixNW;
			break;
		default:
			throw new ArgumentOutOfRangeException("dir");
		}
		Arrow.Refresh();
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
	}

	private void Start()
	{
	}
}
