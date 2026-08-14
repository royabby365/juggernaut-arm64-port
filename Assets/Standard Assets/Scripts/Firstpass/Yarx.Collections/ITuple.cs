using System.Collections;
using System.Text;

namespace Yarx.Collections
{

public interface ITuple
{
	int Size { get; }

	int GetHashCode(IEqualityComparer comparer);

	string ToString(StringBuilder sb);
}
}
