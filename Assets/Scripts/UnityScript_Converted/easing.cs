using System;
using UnityEngine;

[Serializable]
public class easing : MonoBehaviour
{
	[NonSerialized]
	public static EaseE Ease = EaseE.Expo1;

	public static bool IsNull()
	{
		return Ease == EaseE.Null;
	}

	public static void NextEase()
	{
		switch (Ease)
		{
		case EaseE.Null:
			Ease = EaseE.Circ;
			break;
		case EaseE.Circ:
			Ease = EaseE.Expo1;
			break;
		case EaseE.Expo1:
			Ease = EaseE.Expo2;
			break;
		case EaseE.Expo2:
			Ease = EaseE.Expo5;
			break;
		case EaseE.Expo5:
			Ease = EaseE.Quad;
			break;
		case EaseE.Quad:
			Ease = EaseE.Cubic;
			break;
		case EaseE.Cubic:
			Ease = EaseE.Quart;
			break;
		case EaseE.Quart:
			Ease = EaseE.Quint;
			break;
		case EaseE.Quint:
			Ease = EaseE.Sine;
			break;
		}
		Ease = EaseE.Null;
	}

	public static float CalcEase(float start, float distance, float elapsedTime, float duration)
	{
		return (Ease == EaseE.Circ) ? EaseInOutCirc(start, distance, elapsedTime, duration) : ((Ease == EaseE.Circ) ? EaseInOutCirc(start, distance, elapsedTime, duration) : ((Ease == EaseE.Expo1) ? EaseInOutExpo1(start, distance, elapsedTime, duration) : ((Ease == EaseE.Expo2) ? EaseInOutExpo2(start, distance, elapsedTime, duration) : ((Ease == EaseE.Expo5) ? EaseInOutExpo5(start, distance, elapsedTime, duration) : ((Ease == EaseE.Quad) ? EaseInOutQuad(start, distance, elapsedTime, duration) : ((Ease == EaseE.Cubic) ? EaseInOutCubic(start, distance, elapsedTime, duration) : ((Ease == EaseE.Quart) ? EaseInOutQuart(start, distance, elapsedTime, duration) : ((Ease != EaseE.Quint) ? EaseInOutSine(start, distance, elapsedTime, duration) : EaseInOutQuint(start, distance, elapsedTime, duration)))))))));
	}

	public static float EaseInOutQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		else
		{
			elapsedTime -= 2f;
			result = distance / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 2f) + start;
		}
		return result;
	}

	public static float EaseInOutSine(float start, float distance, float elapsedTime, float duration)
	{
		if (!(elapsedTime <= duration))
		{
			elapsedTime = duration;
		}
		return (0f - distance) / 2f * (Mathf.Cos((float)Math.PI * elapsedTime / duration) - 1f) + start;
	}

	public static float EaseInOutLinear(float start, float distance, float elapsedTime, float duration)
	{
		return start + elapsedTime * distance / duration;
	}

	public static float EaseInOutElastic(float b, float d, float t, float c)
	{
		int num = 1;
		float num2 = 1f;
		float result;
		if (t == 0f)
		{
			result = b;
		}
		else
		{
			t /= d / 2f;
			if (t == 2f)
			{
				result = b + c;
			}
			else
			{
				if (num == 0)
				{
					num = (int)(d * 0.45f);
				}
				int num3 = 0;
				if (num2 == 0f || !(num2 >= Math.Abs(c)))
				{
					num2 = c;
					num3 = num / 4;
				}
				else
				{
					num3 = (int)((float)num / ((float)Math.PI * 2f) * Mathf.Asin(c / num2));
				}
				if (!(t >= 1f))
				{
					t -= 1f;
					result = -0.5f * (num2 * Mathf.Pow(2f, 10f * t) * Mathf.Sin((t * d - (float)num3) * ((float)Math.PI * 2f) / (float)num)) + b;
				}
				else
				{
					t -= 1f;
					result = num2 * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * d - (float)num3) * ((float)Math.PI * 2f) / (float)num) * 0.5f + c + b;
				}
			}
		}
		return result;
	}

	public static float EaseInOutExpo1(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = distance / 2f * Mathf.Pow(1.3f, 10f * (elapsedTime - 1f)) + start;
		}
		else
		{
			elapsedTime -= 1f;
			result = distance / 2f * (0f - Mathf.Pow(1.3f, -10f * elapsedTime) + 2f) + start;
		}
		return result;
	}

	public static float EaseInOutExpo2(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = distance / 2f * Mathf.Pow(2f, 10f * (elapsedTime - 1f)) + start;
		}
		else
		{
			elapsedTime -= 1f;
			result = distance / 2f * (0f - Mathf.Pow(2f, -10f * elapsedTime) + 2f) + start;
		}
		return result;
	}

	public static float EaseInOutExpo5(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = distance / 2f * Mathf.Pow(5f, 10f * (elapsedTime - 1f)) + start;
		}
		else
		{
			elapsedTime -= 1f;
			result = distance / 2f * (0f - Mathf.Pow(5f, -10f * elapsedTime) + 2f) + start;
		}
		return result;
	}

	public static float EaseInOutQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		else
		{
			elapsedTime -= 2f;
			result = (0f - distance) / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2f) + start;
		}
		return result;
	}

	public static float EaseInOutQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = distance / 2f * elapsedTime * elapsedTime + start;
		}
		else
		{
			elapsedTime -= 1f;
			result = (0f - distance) / 2f * (elapsedTime * (elapsedTime - 2f) - 1f) + start;
		}
		return result;
	}

	public static float EaseInOutCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = distance / 2f * elapsedTime * elapsedTime * elapsedTime + start;
		}
		else
		{
			elapsedTime -= 2f;
			result = distance / 2f * (elapsedTime * elapsedTime * elapsedTime + 2f) + start;
		}
		return result;
	}

	public static float EaseInOutCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime <= duration) ? (elapsedTime / (duration / 2f)) : 2f);
		float result;
		if (!(elapsedTime >= 1f))
		{
			result = (0f - distance) / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) - 1f) + start;
		}
		else
		{
			elapsedTime -= 2f;
			result = distance / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) + 1f) + start;
		}
		return result;
	}

	public virtual void Main()
	{
	}
}
