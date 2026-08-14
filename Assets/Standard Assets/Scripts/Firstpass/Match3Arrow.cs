using UnityEngine;

public class Match3Arrow : MonoBehaviour
{
	public enum DirectionE
	{
		N,
		S,
		E,
		W
	}

	private Sprite _sprite;

	private DirectionE _direction;

	public DirectionE Direction
	{
		get
		{
			return _direction;
		}
		set
		{
			_direction = value;
			switch (_direction)
			{
			case DirectionE.N:
				_sprite.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case DirectionE.S:
				_sprite.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			case DirectionE.E:
				_sprite.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				break;
			case DirectionE.W:
				_sprite.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			}
		}
	}

	private void OnEnable()
	{
		_sprite = GetComponentInChildren<Sprite>();
	}
}
