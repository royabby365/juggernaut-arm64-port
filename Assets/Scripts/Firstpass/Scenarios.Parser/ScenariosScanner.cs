using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Scenarios.Parser;

public class ScenariosScanner : IScanner
{
	private int _line;

	private State _state;

	public IEnumerator<Token> Scan(TextReader reader)
	{
		_state = State.Outer;
		while (reader.Peek() != -1)
		{
			if ((ushort)reader.Peek() == 10)
			{
				_line++;
			}
			if (_state == State.Outer)
			{
				switch ((char)(ushort)reader.Peek())
				{
				case 'v':
					_state = State.VarBegin;
					break;
				case '$':
					_state = State.Scenario;
					break;
				default:
					reader.Read();
					break;
				}
				continue;
			}
			if (char.IsWhiteSpace((char)reader.Peek()))
			{
				reader.Read();
				continue;
			}
			if (char.IsDigit((char)reader.Peek()))
			{
				yield return ScanDigits(reader);
				continue;
			}
			if (char.IsLetter((char)reader.Peek()))
			{
				string id = ScanId(reader);
				if (_state == State.VarBegin)
				{
					if (id == "var")
					{
						_state = State.Var;
						yield return Token.FromKind(Kind.Var, Pos());
					}
					else
					{
						_state = State.Outer;
					}
				}
				else if ((ushort)reader.Peek() == 35)
				{
					reader.Read();
					yield return Token.HashIdClose(id, Pos());
				}
				else
				{
					yield return Token.Id(id, Pos());
				}
				continue;
			}
			char ch = (char)reader.Read();
			switch (ch)
			{
			case '=':
				yield return Token.FromKind(Kind.Equals, Pos());
				break;
			case '$':
				yield return Token.FromKind(Kind.Dollar, Pos());
				break;
			case '{':
				yield return Token.FromKind(Kind.BraceOpen, Pos());
				break;
			case '}':
				if (_state == State.Scenario)
				{
					_state = State.Outer;
				}
				yield return Token.FromKind(Kind.BraceClose, Pos());
				break;
			case '(':
				yield return Token.FromKind(Kind.ParenOpen, Pos());
				break;
			case ')':
				yield return Token.FromKind(Kind.ParenClose, Pos());
				break;
			case ',':
				yield return Token.FromKind(Kind.Comma, Pos());
				break;
			case ';':
				if (_state == State.Var)
				{
					_state = State.Outer;
				}
				yield return Token.FromKind(Kind.Semi, Pos());
				break;
			case '#':
				if (char.IsLetter((char)reader.Peek()))
				{
					string id2 = ScanId(reader);
					yield return Token.HashIdOpen(id2, Pos());
				}
				else
				{
					ErrorReporter.InScanner("illegal char: '{0}' at {1}", ch, Pos());
				}
				break;
			case '-':
				yield return Token.FromKind(Kind.Minus, Pos());
				break;
			case '+':
				yield return Token.FromKind(Kind.Plus, Pos());
				break;
			case '/':
				EatLineComment(reader);
				break;
			default:
				ErrorReporter.InScanner("illegal char: '{0}' at {1}", ch, Pos());
				break;
			}
		}
		yield return Token.FromKind(Kind.Eof, Pos());
	}

	private Lineinfo Pos()
	{
		return new Lineinfo(_line);
	}

	private static bool IsAlphanum(char ch)
	{
		return char.IsLetterOrDigit(ch) || ch == '_';
	}

	private string ScanId(TextReader reader)
	{
		StringBuilder stringBuilder = new StringBuilder(40);
		stringBuilder.Append((char)reader.Read());
		while (IsAlphanum((char)reader.Peek()))
		{
			stringBuilder.Append((char)reader.Read());
		}
		return stringBuilder.ToString();
	}

	private Token ScanDigits(TextReader reader)
	{
		int num = reader.Read() - 48;
		while (char.IsDigit((char)reader.Peek()))
		{
			num = 10 * num + reader.Read() - 48;
		}
		if (reader.Peek() == 46)
		{
			reader.Read();
			return Token.Float(ScanFrac(num, 0.1f, reader), Pos());
		}
		return Token.Int(num, Pos());
	}

	private static float ScanFrac(int n, float wt, TextReader reader)
	{
		float num = n;
		while (char.IsDigit((char)reader.Peek()))
		{
			num += wt * (float)(reader.Read() - 48);
			wt /= 10f;
		}
		return num;
	}

	private void EatLineComment(TextReader reader)
	{
		char c = (char)reader.Read();
		if (c == '/')
		{
			while (reader.Peek() != -1)
			{
				c = (char)reader.Read();
				if (c == '\n')
				{
					_line++;
					break;
				}
			}
		}
		else
		{
			ErrorReporter.InScanner("illegal char: '{0}'", c);
		}
	}
}
