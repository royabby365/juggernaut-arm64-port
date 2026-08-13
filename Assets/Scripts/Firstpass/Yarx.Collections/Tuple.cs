using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Yarx.Collections
{

public static class Tuple
{
	internal static int CombineHashCodes(int h1, int h2)
	{
		return ((h1 << 5) + h1) ^ h2;
	}

	internal static int CombineHashCodes(int h1, int h2, int h3)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2), h3);
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3), h4);
	}

	public static Tuple<T1> Create<T1>(T1 item1)
	{
		return new Tuple<T1>(item1);
	}

	public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
	{
		return new Tuple<T1, T2>(item1, item2);
	}

	public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
	{
		return new Tuple<T1, T2, T3>(item1, item2, item3);
	}

	public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
	{
		return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
	}
}
[Serializable]
public class Tuple<T1> : IComparable, ITuple, IStructuralComparable, IStructuralEquatable
{
	private readonly T1 _mItem1;

	int ITuple.Size => 1;

	public T1 Item1 => _mItem1;

	public Tuple(T1 item1)
	{
		_mItem1 = item1;
	}

	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)Comparer<object>.Default);
	}

	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1> tuple))
		{
			throw new ArgumentException("ArgumentException_TupleIncorrectType", "other");
		}
		return comparer.Compare(_mItem1, tuple._mItem1);
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		return other is Tuple<T1> tuple && comparer.Equals(_mItem1, tuple._mItem1);
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return comparer.GetHashCode(_mItem1);
	}

	int ITuple.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	string ITuple.ToString(StringBuilder sb)
	{
		sb.Append(_mItem1);
		sb.Append(")");
		return sb.ToString();
	}

	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITuple)this).ToString(stringBuilder);
	}
}
[Serializable]
public class Tuple<T1, T2> : IComparable, ITuple, IStructuralComparable, IStructuralEquatable
{
	private readonly T1 _mItem1;

	private readonly T2 _mItem2;

	int ITuple.Size => 2;

	public T1 Item1 => _mItem1;

	public T2 Item2 => _mItem2;

	public Tuple(T1 item1, T2 item2)
	{
		_mItem1 = item1;
		_mItem2 = item2;
	}

	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)Comparer<object>.Default);
	}

	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2> tuple))
		{
			throw new ArgumentException("ArgumentException_TupleIncorrectType", "other");
		}
		int num = comparer.Compare(_mItem1, tuple._mItem1);
		return (num == 0) ? comparer.Compare(_mItem2, tuple._mItem2) : num;
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2> tuple))
		{
			return false;
		}
		return comparer.Equals(_mItem1, tuple._mItem1) && comparer.Equals(_mItem2, tuple._mItem2);
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(_mItem1), comparer.GetHashCode(_mItem2));
	}

	int ITuple.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	string ITuple.ToString(StringBuilder sb)
	{
		sb.Append(_mItem1);
		sb.Append(", ");
		sb.Append(_mItem2);
		sb.Append(")");
		return sb.ToString();
	}

	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITuple)this).ToString(stringBuilder);
	}
}
[Serializable]
public class Tuple<T1, T2, T3> : IComparable, ITuple, IStructuralComparable, IStructuralEquatable
{
	private readonly T1 _mItem1;

	private readonly T2 _mItem2;

	private readonly T3 _mItem3;

	int ITuple.Size => 3;

	public T1 Item1 => _mItem1;

	public T2 Item2 => _mItem2;

	public T3 Item3 => _mItem3;

	public Tuple(T1 item1, T2 item2, T3 item3)
	{
		_mItem1 = item1;
		_mItem2 = item2;
		_mItem3 = item3;
	}

	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)Comparer<object>.Default);
	}

	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3> tuple))
		{
			throw new ArgumentException("ArgumentException_TupleIncorrectType", "other");
		}
		int num = comparer.Compare(_mItem1, tuple._mItem1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(_mItem2, tuple._mItem2);
		return (num == 0) ? comparer.Compare(_mItem3, tuple._mItem3) : num;
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3> tuple))
		{
			return false;
		}
		return comparer.Equals(_mItem1, tuple._mItem1) && comparer.Equals(_mItem2, tuple._mItem2) && comparer.Equals(_mItem3, tuple._mItem3);
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(_mItem1), comparer.GetHashCode(_mItem2), comparer.GetHashCode(_mItem3));
	}

	int ITuple.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	string ITuple.ToString(StringBuilder sb)
	{
		sb.Append(_mItem1);
		sb.Append(", ");
		sb.Append(_mItem2);
		sb.Append(", ");
		sb.Append(_mItem3);
		sb.Append(")");
		return sb.ToString();
	}

	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITuple)this).ToString(stringBuilder);
	}
}
[Serializable]
public class Tuple<T1, T2, T3, T4> : IComparable, ITuple, IStructuralComparable, IStructuralEquatable
{
	private readonly T1 _mItem1;

	private readonly T2 _mItem2;

	private readonly T3 _mItem3;

	private readonly T4 _mItem4;

	int ITuple.Size => 4;

	public T1 Item1 => _mItem1;

	public T2 Item2 => _mItem2;

	public T3 Item3 => _mItem3;

	public T4 Item4 => _mItem4;

	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
	{
		_mItem1 = item1;
		_mItem2 = item2;
		_mItem3 = item3;
		_mItem4 = item4;
	}

	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)Comparer<object>.Default);
	}

	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3, T4> tuple))
		{
			throw new ArgumentException("ArgumentException_TupleIncorrectType", "other");
		}
		int num = comparer.Compare(_mItem1, tuple._mItem1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(_mItem2, tuple._mItem2);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(_mItem3, tuple._mItem3);
		if (num != 0)
		{
			return num;
		}
		return (num == 0) ? comparer.Compare(_mItem4, tuple._mItem4) : num;
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3, T4> tuple))
		{
			return false;
		}
		return comparer.Equals(_mItem1, tuple._mItem1) && comparer.Equals(_mItem2, tuple._mItem2) && comparer.Equals(_mItem3, tuple._mItem3) && comparer.Equals(_mItem4, tuple._mItem4);
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(_mItem1), comparer.GetHashCode(_mItem2), comparer.GetHashCode(_mItem3), comparer.GetHashCode(_mItem3));
	}

	int ITuple.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	string ITuple.ToString(StringBuilder sb)
	{
		sb.Append(_mItem1);
		sb.Append(", ");
		sb.Append(_mItem2);
		sb.Append(", ");
		sb.Append(_mItem3);
		sb.Append(", ");
		sb.Append(_mItem4);
		sb.Append(")");
		return sb.ToString();
	}

	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)EqualityComparer<object>.Default);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITuple)this).ToString(stringBuilder);
	}
}
}
