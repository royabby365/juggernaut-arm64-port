using System;
using System.Collections.Generic;

internal class TimeEventsManager : SingletonT<TimeEventsManager>
{
	private class Data : IDisposable
	{
		internal float Time;

		internal Action Action;

		internal Data Next;

		internal bool Dead;

		internal string Name;

		void IDisposable.Dispose()
		{
			Dead = true;
		}
	}

	private Data _first;

	private bool _inUpdate;

	private List<Data> _toAdd = new List<Data>();

	internal void Update(float dt)
	{
		Data data = _first;
		if (data == null)
		{
			return;
		}
		_inUpdate = true;
		while (data != null)
		{
			data.Time -= dt;
			if (data.Time <= 0f && !data.Dead)
			{
				data.Action();
			}
			data = data.Next;
		}
		data = _first;
		while (data != null && data.Time <= 0f)
		{
			data = data.Next;
		}
		_first = data;
		if (_toAdd.Count > 0)
		{
			foreach (Data item in _toAdd)
			{
				StartOneShotTimeEvent(item);
			}
			_toAdd.Clear();
		}
		_inUpdate = false;
	}

	private IDisposable StartOneShotTimeEvent(Data data)
	{
		if (_first == null)
		{
			_first = data;
		}
		else
		{
			Data data2 = _first;
			Data data3 = null;
			while (data2 != null)
			{
				if (data2.Time > data.Time)
				{
					data.Next = data2;
					if (data3 == null)
					{
						_first = data;
					}
					else
					{
						data3.Next = data;
					}
					break;
				}
				if (data2.Next == null)
				{
					data2.Next = data;
					break;
				}
				data3 = data2;
				data2 = data2.Next;
			}
		}
		return data;
	}

	internal IDisposable StartOneShotTimeEvent(float time, Action action)
	{
		if (time > 0f)
		{
			return StartOneShotTimeEvent(null, time, action);
		}
		action();
		return null;
	}

	internal IDisposable StartOneShotTimeEvent(string name, float time, Action action)
	{
		Data data = new Data();
		data.Name = name;
		data.Time = time;
		data.Action = action;
		Data data2 = data;
		if (_inUpdate)
		{
			_toAdd.Add(data2);
			return data2;
		}
		return StartOneShotTimeEvent(data2);
	}

	internal void StopAllWithName(string name)
	{
		Data data = _first;
		if (data == null)
		{
			return;
		}
		while (data != null)
		{
			if (data.Name == name)
			{
				data.Dead = true;
			}
			data = data.Next;
		}
	}

	internal void Clear()
	{
		_first = null;
		_inUpdate = false;
		_toAdd.Clear();
	}
}
