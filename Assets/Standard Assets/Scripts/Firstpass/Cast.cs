using System;
using UnityEngine;

internal abstract class Cast
{
	public Action<Cast> Casted;

	public string Name { get; protected set; }

	public abstract void Start(Vector2 point);

	public abstract void Move(Vector2 pos);

	public abstract void End(Vector2 point);

	public abstract void Reset();
}
