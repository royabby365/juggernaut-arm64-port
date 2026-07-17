using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal class BattleStats : MonoBehaviour
{
	private Dictionary<int, float> _baseTime = new Dictionary<int, float>();

	private Dictionary<int, List<float>> _time = new Dictionary<int, List<float>>();

	public string _basePath;

	private readonly string FileName = "fstats.txt";

	private WWW _www;

	public void AddBattleResult(int mob, float time)
	{
		List<float> value = null;
		if (!_time.TryGetValue(mob, out value))
		{
			value = new List<float>();
			_time.Add(mob, value);
		}
		value.Add(time);
	}

	private void UpdateBaseTime()
	{
		foreach (KeyValuePair<int, List<float>> item in _time)
		{
			float num = Middle(item.Value);
			if (_baseTime.ContainsKey(item.Key))
			{
				Dictionary<int, float> baseTime;
				Dictionary<int, float> dictionary = (baseTime = _baseTime);
				int key2;
				int key = (key2 = item.Key);
				float num2 = baseTime[key2];
				dictionary[key] = num2 + num;
			}
			else
			{
				_baseTime.Add(item.Key, num);
			}
		}
		_time.Clear();
	}

	private string CreateSaveDirectory()
	{
		string text = Path.Combine(_basePath, Globals.AppDirName);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public void Save()
	{
		UpdateBaseTime();
		string path = CreateSaveDirectory();
		List<string> list = new List<string>();
		foreach (KeyValuePair<int, float> item in _baseTime)
		{
			list.Add("=");
			list.Add("1");
			list.Add(item.Key.ToString());
			list.Add(item.Value.ToString());
		}
		string path2 = Path.Combine(path, FileName);
		Utils.WriteAllLines(path2, list.ToArray());
	}

	public float Middle(List<float> list)
	{
		if (list.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (float item in list)
		{
			float num2 = item;
			num += num2;
		}
		return num / (float)list.Count;
	}

	public void Load()
	{
		if (_www == null)
		{
			string path = CreateSaveDirectory();
			string url = "file:///" + Path.Combine(path, FileName);
			_www = new WWW(url);
		}
	}

	private void Update()
	{
		if (_www != null && _www.isDone)
		{
			if (_www.error == null)
			{
				LoadData(_www.text);
			}
			_www = null;
		}
	}

	private void LoadData(string text)
	{
		string[] array = text.Split(new string[3]
		{
			"\r",
			"\n",
			Environment.NewLine
		}, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 0)
		{
			int num = 0;
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			do
			{
				Invs.Inv(array[num++] == "=");
				Invs.Inv(array[num++] == "1");
				int key = int.Parse(array[num++]);
				float value = float.Parse(array[num++]);
				dictionary.Add(key, value);
			}
			while (num < array.Length);
			_baseTime = dictionary;
			_time.Clear();
		}
	}
}
