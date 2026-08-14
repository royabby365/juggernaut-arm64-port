using System.Collections.Generic;
using System.IO;

namespace Scenarios.Parser
{

public interface IScanner
{
	IEnumerator<Token> Scan(TextReader reader);
}
}
