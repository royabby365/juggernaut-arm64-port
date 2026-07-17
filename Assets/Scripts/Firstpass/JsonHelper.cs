using System.Linq;
using System.Text;

internal class JsonHelper
{
	public static string FormatJson(string str)
	{
		int num = 0;
		bool flag = false;
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < str.Length; i++)
		{
			char c = str[i];
			switch (c)
			{
			case '[':
			case '{':
			{
				sb.Append(c);
				if (flag)
				{
					break;
				}
				int j;
				for (j = i + 1; j < str.Length && str[j] == ' '; j++)
				{
				}
				bool flag3 = j > i && j < str.Length && (str[j] == '}' || str[j] == ']');
				num++;
				if (!flag3)
				{
					sb.AppendLine();
					Enumerable.Range(0, num).ForEach(delegate
					{
						sb.Append("\t");
					});
				}
				break;
			}
			case ']':
			case '}':
				if (!flag)
				{
					sb.AppendLine();
					num--;
					Enumerable.Range(0, num).ForEach(delegate
					{
						sb.Append("\t");
					});
				}
				sb.Append(c);
				break;
			case '"':
			{
				sb.Append(c);
				bool flag2 = false;
				int num2 = i;
				while (num2 > 0 && str[--num2] == '\\')
				{
					flag2 = !flag2;
				}
				if (!flag2)
				{
					flag = !flag;
				}
				break;
			}
			case ',':
				sb.Append(c);
				if (!flag)
				{
					sb.AppendLine();
					Enumerable.Range(0, num).ForEach(delegate
					{
						sb.Append("\t");
					});
				}
				break;
			case ':':
				sb.Append(c);
				if (!flag)
				{
					sb.Append(" ");
				}
				break;
			case ' ':
				if (flag)
				{
					sb.Append(c);
				}
				break;
			default:
				sb.Append(c);
				break;
			}
		}
		return sb.ToString();
	}
}
