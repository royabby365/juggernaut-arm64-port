using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiniJSON
{

public class Json
{
	private class Parser
	{
		private enum TOKEN
		{
			NONE,
			CURLY_OPEN,
			CURLY_CLOSE,
			SQUARED_OPEN,
			SQUARED_CLOSE,
			COLON,
			COMMA,
			STRING,
			NUMBER,
			TRUE,
			FALSE,
			NULL
		}

		private StringReader json;

		public Parser(string jsonData)
		{
			json = new StringReader(jsonData);
		}

		public object Parse()
		{
			return ParseValue();
		}

		private Dictionary<string, object> ParseObject()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			json.Read();
			while (true)
			{
				switch (NextToken())
				{
				case TOKEN.COMMA:
					continue;
				case TOKEN.NONE:
					return null;
				case TOKEN.CURLY_CLOSE:
					return dictionary;
				}
				string text = ParseString();
				if (text == null)
				{
					return null;
				}
				TOKEN tOKEN = NextToken();
				if (tOKEN != TOKEN.COLON)
				{
					return null;
				}
				json.Read();
				dictionary[text] = ParseValue();
			}
		}

		private List<object> ParseArray()
		{
			List<object> list = new List<object>();
			json.Read();
			while (true)
			{
				switch (NextToken())
				{
				case TOKEN.COMMA:
					break;
				case TOKEN.NONE:
					return null;
				default:
					goto IL_0039;
				case TOKEN.SQUARED_CLOSE:
					return list;
				}
				continue;
				IL_0039:
				object item = ParseValue();
				list.Add(item);
			}
		}

		private object ParseValue()
		{
			return NextToken() switch
			{
				TOKEN.STRING => ParseString(), 
				TOKEN.NUMBER => ParseNumber(), 
				TOKEN.CURLY_OPEN => ParseObject(), 
				TOKEN.SQUARED_OPEN => ParseArray(), 
				TOKEN.TRUE => true, 
				TOKEN.FALSE => false, 
				TOKEN.NULL => null, 
				_ => null, 
			};
		}

		private string ParseString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			json.Read();
			bool flag = false;
			while (json.Peek() != -1)
			{
				char c = ReadChar();
				switch (c)
				{
				case '"':
					flag = true;
					break;
				case '\\':
					if (json.Peek() == -1)
					{
						break;
					}
					switch (ReadChar())
					{
					case '"':
						stringBuilder.Append('"');
						break;
					case '\\':
						stringBuilder.Append('\\');
						break;
					case '/':
						stringBuilder.Append('/');
						break;
					case 'b':
						stringBuilder.Append('\b');
						break;
					case 'f':
						stringBuilder.Append('\f');
						break;
					case 'n':
						stringBuilder.Append('\n');
						break;
					case 'r':
						stringBuilder.Append('\r');
						break;
					case 't':
						stringBuilder.Append('\t');
						break;
					case 'u':
					{
						StringBuilder stringBuilder2 = new StringBuilder();
						for (int i = 0; i < 4; i++)
						{
							stringBuilder2.Append(ReadChar());
						}
						stringBuilder.Append((char)Convert.ToInt32(stringBuilder2.ToString(), 16));
						break;
					}
					}
					continue;
				default:
					stringBuilder.Append(c);
					continue;
				}
				break;
			}
			if (!flag)
			{
				return null;
			}
			return stringBuilder.ToString();
		}

		private object ParseNumber()
		{
			string text = NextWord();
			if (text.IndexOf('.') == -1)
			{
				return long.Parse(text);
			}
			return double.Parse(text);
		}

		private void EatWhitespace()
		{
			while (" \t\n\r".IndexOf(PeekChar()) != -1)
			{
				json.Read();
				if (json.Peek() == -1)
				{
					break;
				}
			}
		}

		private char PeekChar()
		{
			try
			{
				return Convert.ToChar(json.Peek());
			}
			catch (OverflowException)
			{
				return '\0';
			}
		}

		private char ReadChar()
		{
			try
			{
				return Convert.ToChar(json.Read());
			}
			catch (OverflowException)
			{
				return '\0';
			}
		}

		private string NextWord()
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (" \t\n\r{}[],:\"".IndexOf(PeekChar()) == -1)
			{
				stringBuilder.Append(ReadChar());
				if (json.Peek() == -1)
				{
					break;
				}
			}
			return stringBuilder.ToString();
		}

		private TOKEN NextToken()
		{
			EatWhitespace();
			if (json.Peek() == -1)
			{
				return TOKEN.NONE;
			}
			switch (PeekChar())
			{
			case '{':
				return TOKEN.CURLY_OPEN;
			case '}':
				json.Read();
				return TOKEN.CURLY_CLOSE;
			case '[':
				return TOKEN.SQUARED_OPEN;
			case ']':
				json.Read();
				return TOKEN.SQUARED_CLOSE;
			case ',':
				json.Read();
				return TOKEN.COMMA;
			case '"':
				return TOKEN.STRING;
			case ':':
				return TOKEN.COLON;
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return TOKEN.NUMBER;
			default:
				return NextWord() switch
				{
					"false" => TOKEN.FALSE, 
					"true" => TOKEN.TRUE, 
					"null" => TOKEN.NULL, 
					_ => TOKEN.NONE, 
				};
			}
		}
	}

	private class Serializer
	{
		private StringBuilder builder;

		private object obj;

		public Serializer(object obj)
		{
			this.obj = obj;
			builder = new StringBuilder();
		}

		public string Serialize()
		{
			SerializeValue(obj);
			return builder.ToString();
		}

		private void SerializeValue(object value)
		{
			if (value == null)
			{
				builder.Append("null");
			}
			else if (value is IDictionary)
			{
				SerializeObject((IDictionary)value);
			}
			else if (value is IList)
			{
				SerializeArray((IList)value);
			}
			else if (value is string)
			{
				SerializeString((string)value);
			}
			else if (value is char)
			{
				SerializeString(((char)value).ToString());
			}
			else if (value is bool)
			{
				builder.Append((!(bool)value) ? "false" : "true");
			}
			else
			{
				SerializeOther(value);
			}
		}

		private void SerializeObject(IDictionary obj)
		{
			bool flag = true;
			builder.Append('{');
			foreach (object key in obj.Keys)
			{
				if (!flag)
				{
					builder.Append(',');
				}
				SerializeString(key.ToString());
				builder.Append(':');
				SerializeValue(obj[key]);
				flag = false;
			}
			builder.Append('}');
		}

		private void SerializeArray(IList anArray)
		{
			builder.Append('[');
			bool flag = true;
			foreach (object item in anArray)
			{
				if (!flag)
				{
					builder.Append(',');
				}
				SerializeValue(item);
				flag = false;
			}
			builder.Append(']');
		}

		private void SerializeString(string str)
		{
			builder.Append('"');
			char[] array = str.ToCharArray();
			char[] array2 = array;
			foreach (char c in array2)
			{
				switch (c)
				{
				case '"':
					builder.Append("\\\"");
					continue;
				case '\\':
					builder.Append("\\\\");
					continue;
				case '\b':
					builder.Append("\\b");
					continue;
				case '\f':
					builder.Append("\\f");
					continue;
				case '\n':
					builder.Append("\\n");
					continue;
				case '\r':
					builder.Append("\\r");
					continue;
				case '\t':
					builder.Append("\\t");
					continue;
				}
				int num = Convert.ToInt32(c);
				if (num >= 32 && num <= 126)
				{
					builder.Append(c);
				}
				else
				{
					builder.Append("\\u" + Convert.ToString(num, 16).PadLeft(4, '0'));
				}
			}
			builder.Append('"');
		}

		private void SerializeOther(object value)
		{
			if (value is float || value is int || value is uint || value is long || value is double || value is sbyte || value is byte || value is short || value is ushort || value is ulong || value is decimal)
			{
				builder.Append(value.ToString());
			}
			else
			{
				SerializeString(value.ToString());
			}
		}
	}

	public static object Deserialize(string json)
	{
		if (json == null)
		{
			return null;
		}
		Parser parser = new Parser(json);
		return parser.Parse();
	}

	public static string Serialize(object obj)
	{
		Serializer serializer = new Serializer(obj);
		return serializer.Serialize();
	}
}
}
