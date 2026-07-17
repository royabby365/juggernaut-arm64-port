using System;
using System.Collections.Generic;

namespace Yarx.Collections;

[Serializable]
public class PriorityQueue<T> where T : IComparable<T>
{
	private const int DefaultInitialCapacity = 16;

	private readonly List<T> _queue;

	public int Count => _queue.Count;

	public PriorityQueue()
		: this(16)
	{
	}

	public PriorityQueue(int initialSize)
	{
		if (initialSize < 0)
		{
			throw new ArgumentException("initialSize < 0");
		}
		_queue = new List<T>(initialSize);
	}

	public T Peek()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("empty heap");
		}
		return _queue[0];
	}

	public PriorityQueue<T> Enqueue(T element)
	{
		_queue.Add(element);
		int num = Count - 1;
		if (num != 0)
		{
			Swim(num);
		}
		return this;
	}

	public T Dequeue()
	{
		T result = Peek();
		Swap(0, Count - 1);
		_queue.RemoveAt(Count - 1);
		Sink(0);
		return result;
	}

	private bool Less(int left, int right)
	{
		return _queue[left].CompareTo(_queue[right]) < 0;
	}

	private void Swap(int left, int right)
	{
		T value = _queue[left];
		_queue[left] = _queue[right];
		_queue[right] = value;
	}

	private void Swim(int k)
	{
		while (Less(k, k / 2))
		{
			Swap(k, k / 2);
			k /= 2;
		}
	}

	private void Sink(int k)
	{
		while (2 * k + 1 < Count)
		{
			int num = 2 * k + 1;
			int num2 = 2 * k + 2;
			int num3 = ((num2 >= Count || !Less(num2, num)) ? num : num2);
			if (Less(k, num3))
			{
				break;
			}
			Swap(k, num3);
			k = num3;
		}
	}
}
