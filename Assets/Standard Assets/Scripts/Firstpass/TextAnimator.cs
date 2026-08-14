using UnityEngine;

public class TextAnimator : MonoBehaviour
{
	public float LifeTime;

	public float Speed;

	private float _currentTime;

	private SpriteText _spriteText;

	private Sprite _sprite;

	private void Start()
	{
		_currentTime = LifeTime;
		_spriteText = base.gameObject.GetComponentInChildren<SpriteText>();
		_sprite = base.gameObject.GetComponentInChildren<Sprite>();
	}

	private void Update()
	{
		float num = LifeTime / 2f;
		if (_currentTime > 0f)
		{
			if (_currentTime < num)
			{
				if (_spriteText != null)
				{
					_spriteText.TextAlpha_ = Mathf.Lerp(1f, 0f, 1f - _currentTime / num);
				}
				if (_sprite != null)
				{
					_sprite.Tint_ = new Color(_sprite.Tint.r, _sprite.Tint.g, _sprite.Tint.b, Mathf.Lerp(1f, 0f, 1f - _currentTime / num));
				}
			}
			_currentTime -= Time.deltaTime;
			base.gameObject.transform.position += new Vector3(0f, Speed * Time.deltaTime, 0f);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
