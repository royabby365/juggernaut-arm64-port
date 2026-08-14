using UnityEngine;

public abstract class SpriteButton : MonoBehaviour
{
	protected bool _active;

	protected bool _selected;

	protected float _longPressInterval = 9999f;

	private float _lastAlfa = 1f;

	private SpriteGui _spriteGui;

	public string ClickSound;

	protected float _horizontalPadding;

	protected float _verticalPadding;

	protected Collider _collider;

	public float LongPressInterval
	{
		get
		{
			return _longPressInterval;
		}
		protected set
		{
			_longPressInterval = value;
		}
	}

	public bool Inited { get; private set; }

	public float Width
	{
		get
		{
			Sprite component = base.transform.GetComponent<Sprite>();
			if (component != null)
			{
				return component.Width;
			}
			return 0f;
		}
	}

	public float Height
	{
		get
		{
			Sprite component = base.transform.GetComponent<Sprite>();
			if (component != null)
			{
				return component.Height;
			}
			return 0f;
		}
	}

	public bool Active => _active;

	public bool Selected
	{
		get
		{
			return _selected;
		}
		protected set
		{
			_selected = value;
		}
	}

	public string Name => base.name;

	public virtual void ResetScale()
	{
	}

	public virtual void UnregisterMe()
	{
		if (!(_spriteGui == null))
		{
			_spriteGui.UnregisterButton(this);
		}
	}

	public virtual void Clicked()
	{
	}

	public virtual void Entered()
	{
	}

	public virtual void Left()
	{
	}

	public virtual void Released()
	{
	}

	public void SetAlpha(float alpha)
	{
		if (_lastAlfa.Eqv(alpha))
		{
			return;
		}
		_lastAlfa = alpha;
		Color color = ((!(alpha <= 0.9f)) ? new Color(1f, 1f, 1f, 1f) : new Color(0.3f, 0.3f, 0.3f, alpha));
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		if (componentsInChildren != null)
		{
			MeshFilter[] array = componentsInChildren;
			foreach (MeshFilter meshFilter in array)
			{
				meshFilter.mesh.SetTint(color);
			}
		}
	}

	protected void SetColor(Color color)
	{
		Sprite[] componentsInChildren = GetComponentsInChildren<Sprite>();
		if (componentsInChildren != null)
		{
			Sprite[] array = componentsInChildren;
			foreach (Sprite sprite in array)
			{
				sprite.Tint_ = color;
			}
		}
	}

	public virtual void SetActive()
	{
		_active = true;
		if (_collider == null)
		{
			AddCollider();
		}
	}

	public virtual void SetInactive()
	{
		_active = false;
	}

	public virtual void SetSelected()
	{
		_selected = true;
	}

	public virtual void SetUnselected()
	{
		_selected = false;
	}

	public void Init()
	{
		_spriteGui = base.transform.GetSpriteGui();
		_spriteGui.RegisterButton(this);
		AddCollider();
		Inited = true;
	}

	public void Init(int hpadding, int vpadding)
	{
		_horizontalPadding = hpadding;
		_verticalPadding = vpadding;
		Init();
	}

	public void Remove()
	{
		UnregisterMe();
	}

	private void AddCollider()
	{
		if (_collider != null)
		{
			return;
		}
		Sprite component = base.transform.GetComponent<Sprite>();
		if (component != null)
		{
			int width = component.Width;
			int height = component.Height;
			BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
			boxCollider.center = component.GetCenter();
			_horizontalPadding /= base.transform.localScale.x;
			_verticalPadding /= base.transform.localScale.y;
			boxCollider.size = new Vector3((float)width + 2f * _horizontalPadding, (float)height + 2f * _verticalPadding, 0f);
			_collider = boxCollider;
			return;
		}
		Mesh mesh = base.transform.GetMesh();
		if (mesh == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.Log("ERROR: ==== CANNOT CREATE COLLIDER FOR BUTTON: {0} ====".Fmt(base.name));
			}
			return;
		}
		Bounds bounds = mesh.bounds;
		if (!(bounds.size == Vector3.zero))
		{
			BoxCollider boxCollider2 = base.gameObject.AddComponent<BoxCollider>();
			boxCollider2.center = bounds.center;
			_horizontalPadding /= base.transform.localScale.x;
			_verticalPadding /= base.transform.localScale.y;
			boxCollider2.size = new Vector3(bounds.size.x + 2f * _horizontalPadding, bounds.size.y + 2f * _verticalPadding, bounds.size.z);
			_collider = boxCollider2;
		}
	}
}
