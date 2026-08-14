using System.Collections;

namespace Yarx
{

public interface IStructuralComparable
{
	int CompareTo(object other, IComparer comparer);
}
}
