using System;
using System.Collections.Generic;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class sfx_weapon_trail : MonoBehaviour
{
	public float height;

	public float time;

	public bool alwaysUp;

	public float minDistance;

	public Color startColor;

	public Color endColor;

	public object character_scenarios;

	private List<TronTrailSection> sections;

	public sfx_weapon_trail()
	{
		height = 0.5f;
		time = 0.1f;
		minDistance = 0.1f;
		startColor = new Color(1f, 1f, 1f, 0.9f);
		endColor = new Color(0.5f, 0.5f, 0.5f, 0f);
	}

	public virtual void Start()
	{
		sections = new List<TronTrailSection>();
	}

	public virtual void LateUpdate()
	{
		Vector3 position = transform.position;
		float num = Time.time;
		while (sections.Count > 0 && num > sections[sections.Count - 1].time + time)
		{
			sections.RemoveAt(sections.Count - 1);
		}
		if (sections.Count < 2)
		{
			minDistance = 0.8f;
		}
		else
		{
			minDistance = 0.1f;
		}
		if (sections.Count == 0 || !((sections[0].point - position).sqrMagnitude <= minDistance * minDistance))
		{
			TronTrailSection tronTrailSection = new TronTrailSection();
			tronTrailSection.point = position;
			if (alwaysUp)
			{
				tronTrailSection.upDir = Vector3.up;
			}
			else
			{
				tronTrailSection.upDir = transform.TransformDirection(Vector3.up);
			}
			tronTrailSection.time = num;
			sections.Insert(0, tronTrailSection);
		}
		Mesh mesh = ((MeshFilter)GetComponent<MeshFilter>()).mesh;
		if ((bool)mesh)
		{
			mesh.Clear();
		}
		if (sections.Count < 4)
		{
			return;
		}
		Vector3[] array = new Vector3[sections.Count * 2];
		Color[] array2 = new Color[sections.Count * 2];
		Vector2[] array3 = new Vector2[sections.Count * 2];
		TronTrailSection tronTrailSection2 = sections[0];
		TronTrailSection tronTrailSection3 = sections[0];
		Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
		for (int i = 0; i < sections.Count; i++)
		{
			tronTrailSection2 = tronTrailSection3;
			tronTrailSection3 = sections[i];
			float num2 = 0f;
			if (i != 0)
			{
				num2 = Mathf.Clamp01((Time.time - tronTrailSection3.time) / time);
			}
			Vector3 upDir = tronTrailSection3.upDir;
			ref Vector3 reference = ref array[i * 2 + 0];
			reference = worldToLocalMatrix.MultiplyPoint(tronTrailSection3.point);
			ref Vector3 reference2 = ref array[i * 2 + 1];
			reference2 = worldToLocalMatrix.MultiplyPoint(tronTrailSection3.point + upDir * height);
			ref Vector2 reference3 = ref array3[i * 2 + 0];
			reference3 = new Vector2(num2, 0f);
			ref Vector2 reference4 = ref array3[i * 2 + 1];
			reference4 = new Vector2(num2, 1f);
			Color color = Color.Lerp(startColor, endColor, num2);
			array2[i * 2 + 0] = color;
			array2[i * 2 + 1] = color;
		}
		int[] array4 = new int[(sections.Count - 1) * 2 * 3];
		for (int i = 0; i < UnityScript.Lang.Extensions.get_length((System.Array)array4) / 6; i++)
		{
			array4[i * 6 + 0] = i * 2;
			array4[i * 6 + 1] = i * 2 + 1;
			array4[i * 6 + 2] = i * 2 + 2;
			array4[i * 6 + 3] = i * 2 + 2;
			array4[i * 6 + 4] = i * 2 + 1;
			array4[i * 6 + 5] = i * 2 + 3;
		}
		mesh.vertices = array;
		mesh.colors = array2;
		mesh.uv = array3;
		mesh.triangles = array4;
	}

	public virtual void Main()
	{
	}
}
