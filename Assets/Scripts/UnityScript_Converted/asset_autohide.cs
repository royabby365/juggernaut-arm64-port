using System;
using UnityEngine;

[Serializable]
public class asset_autohide : MonoBehaviour
{
	public Vector3 visibility_bounds;

	public Vector3 visibility_bounds_offset;

	private bool visibility_bounds_visible;

	private ParticleSystem pe;

	private float visibility_bounds_disabled_time;

	private bool visibility_bounds_enabled;

	public asset_autohide()
	{
		visibility_bounds = new Vector3(0f, 0f, 0f);
		visibility_bounds_offset = new Vector3(0f, 0f, 0f);
		visibility_bounds_enabled = true;
	}

	public virtual void Update()
	{
		if (visibility_bounds_enabled && (bool)pe)
		{
			UpdateVisibility();
		}
	}

	public virtual void UpdateVisibility()
	{
		Bounds bounds = new Bounds(transform.position + visibility_bounds_offset, visibility_bounds);
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
		bool flag = GeometryUtility.TestPlanesAABB(planes, bounds);
		if (flag != visibility_bounds_visible)
		{
			visibility_bounds_visible = flag;
			{ var em = pe.emission; em.enabled = flag; }
			if (flag)
			{
				visibility_bounds_disabled_time = 0f;
			}
		}
		if (!flag)
		{
			visibility_bounds_disabled_time += Time.deltaTime;
		}
	}

	public virtual void Start()
	{
		visibility_bounds = new Vector3(3f, 10f, 3f);
		visibility_bounds_offset = new Vector3(0f, 4f, 0f);
		visibility_bounds_visible = false;
		visibility_bounds_disabled_time = 0f;
		if ((bool)GetComponent<Renderer>())
		{
			pe = (ParticleSystem)GetComponent<Renderer>().gameObject.GetComponent<ParticleSystem>();
			if ((bool)pe && visibility_bounds_enabled)
			{
				{ var em = pe.emission; em.enabled = false; }
			}
		}
	}

	public virtual void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position + visibility_bounds_offset, visibility_bounds);
	}

	public virtual void Main()
	{
	}
}
