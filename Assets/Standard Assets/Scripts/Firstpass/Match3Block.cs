using UnityEngine;

public class Match3Block : MonoBehaviour
{
	public enum TypeE
	{
		Red,
		Blue,
		Violet,
		Loot
	}

	private Sprite _mainSprite;

	private bool _isSelected;

	private ServerData.Bonus.DropElement _loot;

	private float _timer;

	private float _animationTime;

	private int _direction;

	public SpriteText Count;

	public Sprite Selection;

	public Sprite Icon;

	public int X;

	public int Y;

	public Vector3 StartPos;

	public TypeE Type { get; private set; }

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			Selection.gameObject.SetActiveRecursivelyMk1(_isSelected);
			if (_isSelected)
			{
				_timer = 0f;
			}
			else
			{
				base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
		}
	}

	public ServerData.Bonus.DropElement Loot => _loot;

	private void OnEnable()
	{
		_mainSprite = GetComponent<Sprite>();
	}

	private void Start()
	{
		Selection.gameObject.SetActiveRecursivelyMk1(setActive: false);
	}

	private void Update()
	{
		if (_isSelected)
		{
			if (_timer > 0f)
			{
				float num = 3.5f;
				float z = 0f - num + num * 2f * (float)_direction;
				float z2 = num - num * 2f * (float)_direction;
				base.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(0f, 0f, z2), Quaternion.Euler(0f, 0f, z), 1f - _timer / _animationTime);
				_timer -= Time.deltaTime;
			}
			else
			{
				_animationTime = Random.Range(0.2f, 0.4f);
				_timer = _animationTime;
				_direction = 1 - _direction;
			}
		}
	}

	public void Init(TypeE type, ServerData.Bonus.DropElement loot, Vector2 size)
	{
		Vector3 localScale = new Vector3(size.x / (float)_mainSprite.Width, size.y / (float)_mainSprite.Height, 1f);
		base.transform.localScale = localScale;
		Type = type;
		_loot = loot;
		Count.gameObject.SetActiveRecursivelyMk1(setActive: false);
		Icon.gameObject.SetActiveRecursivelyMk1(setActive: false);
		switch (Type)
		{
		case TypeE.Red:
			_mainSprite.SpriteName_ = "tile_red";
			break;
		case TypeE.Blue:
			_mainSprite.SpriteName_ = "tile_blue";
			break;
		case TypeE.Violet:
			_mainSprite.SpriteName_ = "tile_violet";
			break;
		case TypeE.Loot:
			_mainSprite.SpriteName_ = "tile_dark";
			Count.gameObject.SetActiveRecursivelyMk1(setActive: true);
			Count.Text_ = _loot.Count.ToString();
			Icon.gameObject.SetActiveRecursivelyMk1(setActive: true);
			Icon.SpriteName_ = Match3LootHud.GetLootImage(loot);
			break;
		}
	}
}
