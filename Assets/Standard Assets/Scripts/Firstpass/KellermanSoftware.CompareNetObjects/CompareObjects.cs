using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KellermanSoftware.CompareNetObjects
{

public class CompareObjects
{
	private List<string> _differences = new List<string>();

	private List<object> _parents = new List<object>();

	private List<string> _elementsToIgnore = new List<string>();

	private bool _comparePrivateProperties;

	private bool _comparePrivateFields;

	private bool _compareChildren = true;

	private bool _compareReadOnly = true;

	private bool _compareFields = true;

	private bool _compareProperties = true;

	private int _maxDifferences = 1;

	public List<string> ElementsToIgnore
	{
		get
		{
			return _elementsToIgnore;
		}
		set
		{
			_elementsToIgnore = value;
		}
	}

	public bool ComparePrivateProperties
	{
		get
		{
			return _comparePrivateProperties;
		}
		set
		{
			_comparePrivateProperties = value;
		}
	}

	public bool ComparePrivateFields
	{
		get
		{
			return _comparePrivateFields;
		}
		set
		{
			_comparePrivateFields = value;
		}
	}

	public bool CompareChildren
	{
		get
		{
			return _compareChildren;
		}
		set
		{
			_compareChildren = value;
		}
	}

	public bool CompareReadOnly
	{
		get
		{
			return _compareReadOnly;
		}
		set
		{
			_compareReadOnly = value;
		}
	}

	public bool CompareFields
	{
		get
		{
			return _compareFields;
		}
		set
		{
			_compareFields = value;
		}
	}

	public bool CompareProperties
	{
		get
		{
			return _compareProperties;
		}
		set
		{
			_compareProperties = value;
		}
	}

	public int MaxDifferences
	{
		get
		{
			return _maxDifferences;
		}
		set
		{
			_maxDifferences = value;
		}
	}

	public List<string> Differences
	{
		get
		{
			return _differences;
		}
		set
		{
			_differences = value;
		}
	}

	public string DifferencesString
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(4096);
			stringBuilder.Append("\r\nBegin Differences:\r\n");
			foreach (string difference in Differences)
			{
				stringBuilder.AppendFormat("{0}\r\n", difference);
			}
			stringBuilder.AppendFormat("End Differences (Maximum of {0} differences shown).", MaxDifferences);
			return stringBuilder.ToString();
		}
	}

	public bool Compare(object object1, object object2)
	{
		string empty = string.Empty;
		Differences.Clear();
		Compare(object1, object2, empty);
		return Differences.Count == 0;
	}

	private void Compare(object object1, object object2, string breadCrumb)
	{
		if (object1 == null && object2 == null)
		{
			return;
		}
		if (object1 == null)
		{
			Differences.Add(string.Format("object1{0} == null && object2{0} != null ((null),{1})", breadCrumb, cStr(object2)));
			return;
		}
		if (object2 == null)
		{
			Differences.Add(string.Format("object1{0} != null && object2{0} == null ({1},(null))", breadCrumb, cStr(object1)));
			return;
		}
		Type type = object1.GetType();
		Type type2 = object2.GetType();
		if (type != type2)
		{
			Differences.Add(string.Format("Different Types:  object1{0}.GetType() != object2{0}.GetType() {1} {2}", breadCrumb, type.Name, type2.Name));
			return;
		}
		if (IsDataset(type))
		{
			CompareDataset(object1, object2, breadCrumb);
			return;
		}
		if (IsDataTable(type))
		{
			CompareDataTable(object1, object2, breadCrumb);
			return;
		}
		if (IsDataRow(type))
		{
			CompareDataRow(object1, object2, breadCrumb);
			return;
		}
		if (IsIList(type))
		{
			CompareIList(object1, object2, breadCrumb);
			return;
		}
		if (IsIDictionary(type))
		{
			CompareIDictionary(object1, object2, breadCrumb);
			return;
		}
		if (IsEnum(type))
		{
			CompareEnum(object1, object2, breadCrumb);
			return;
		}
		if (IsSimpleType(type))
		{
			CompareSimpleType(object1, object2, breadCrumb);
			return;
		}
		if (IsClass(type))
		{
			CompareClass(object1, object2, breadCrumb);
			return;
		}
		if (IsTimespan(type))
		{
			CompareTimespan(object1, object2, breadCrumb);
			return;
		}
		if (IsStruct(type))
		{
			CompareStruct(object1, object2, breadCrumb);
			return;
		}
		throw new NotImplementedException("Cannot compare object of type " + type.Name);
	}

	private void CompareDataRow(object object1, object object2, string breadCrumb)
	{
	}

	private void CompareDataTable(object object1, object object2, string breadCrumb)
	{
	}

	private void CompareDataset(object object1, object object2, string breadCrumb)
	{
	}

	private bool IsTimespan(Type t)
	{
		return t == typeof(TimeSpan);
	}

	private bool IsEnum(Type t)
	{
		return t.IsEnum;
	}

	private bool IsStruct(Type t)
	{
		return t.IsValueType && !IsSimpleType(t);
	}

	private bool IsSimpleType(Type t)
	{
		if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
		{
			t = Nullable.GetUnderlyingType(t);
		}
		return t.IsPrimitive || t == typeof(DateTime) || t == typeof(decimal) || t == typeof(string) || t == typeof(Guid);
	}

	private bool ValidStructSubType(Type t)
	{
		return IsSimpleType(t) || IsEnum(t) || IsArray(t) || IsClass(t) || IsIDictionary(t) || IsTimespan(t) || IsIList(t);
	}

	private bool IsArray(Type t)
	{
		return t.IsArray;
	}

	private bool IsClass(Type t)
	{
		return t.IsClass;
	}

	private bool IsIDictionary(Type t)
	{
		return t.GetInterface("System.Collections.IDictionary", ignoreCase: true) != null;
	}

	private bool IsDataset(Type t)
	{
		return false;
	}

	private bool IsDataRow(Type t)
	{
		return false;
	}

	private bool IsDataTable(Type t)
	{
		return false;
	}

	private bool IsIList(Type t)
	{
		return t.GetInterface("System.Collections.IList", ignoreCase: true) != null;
	}

	private bool IsChildType(Type t)
	{
		return !IsSimpleType(t) && (IsClass(t) || IsArray(t) || IsIDictionary(t) || IsIList(t) || IsStruct(t));
	}

	private void CompareTimespan(object object1, object object2, string breadCrumb)
	{
		if (((TimeSpan)object1).Ticks != ((TimeSpan)object2).Ticks)
		{
			Differences.Add(string.Format("object1{0}.Ticks != object2{0}.Ticks", breadCrumb));
		}
	}

	private void CompareEnum(object object1, object object2, string breadCrumb)
	{
		if (object1.ToString() != object2.ToString())
		{
			string arg = AddBreadCrumb(breadCrumb, object1.GetType().Name, string.Empty, -1);
			Differences.Add(string.Format("object1{0} != object2{0} ({1},{2})", arg, object1, object2));
		}
	}

	private void CompareSimpleType(object object1, object object2, string breadCrumb)
	{
		if (object2 == null)
		{
			throw new ArgumentNullException("object2");
		}
		if (!(object1 is IComparable comparable))
		{
			throw new ArgumentNullException("object1");
		}
		if (comparable.CompareTo(object2) != 0)
		{
			Differences.Add(string.Format("object1{0} != object2{0} ({1},{2})", breadCrumb, object1, object2));
		}
	}

	private void CompareStruct(object object1, object object2, string breadCrumb)
	{
		Type type = object1.GetType();
		FieldInfo[] fields = type.GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (ValidStructSubType(fieldInfo.FieldType))
			{
				string breadCrumb2 = AddBreadCrumb(breadCrumb, fieldInfo.Name, string.Empty, -1);
				Compare(fieldInfo.GetValue(object1), fieldInfo.GetValue(object2), breadCrumb2);
				if (Differences.Count >= MaxDifferences)
				{
					break;
				}
			}
		}
	}

	private void CompareClass(object object1, object object2, string breadCrumb)
	{
		try
		{
			_parents.Add(object1);
			_parents.Add(object2);
			Type type = object1.GetType();
			if (!ElementsToIgnore.Contains(type.Name))
			{
				if (CompareProperties)
				{
					PerformCompareProperties(type, object1, object2, breadCrumb);
				}
				if (CompareFields)
				{
					PerformCompareFields(type, object1, object2, breadCrumb);
				}
			}
		}
		finally
		{
			_parents.Remove(object1);
			_parents.Remove(object2);
		}
	}

	private void PerformCompareFields(Type t1, object object1, object object2, string breadCrumb)
	{
		FieldInfo[] array = ((!ComparePrivateFields) ? t1.GetFields() : t1.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		FieldInfo[] array2 = array;
		foreach (FieldInfo fieldInfo in array2)
		{
			if ((!CompareChildren && IsChildType(fieldInfo.FieldType)) || ElementsToIgnore.Contains(fieldInfo.Name))
			{
				continue;
			}
			object value = fieldInfo.GetValue(object1);
			object value2 = fieldInfo.GetValue(object2);
			bool flag = value != null && (value == object1 || _parents.Contains(value));
			bool flag2 = value2 != null && (value2 == object2 || _parents.Contains(value2));
			if (!IsClass(fieldInfo.FieldType) || (!flag && !flag2))
			{
				string breadCrumb2 = AddBreadCrumb(breadCrumb, fieldInfo.Name, string.Empty, -1);
				Compare(value, value2, breadCrumb2);
				if (Differences.Count >= MaxDifferences)
				{
					break;
				}
			}
		}
	}

	private void PerformCompareProperties(Type t1, object object1, object object2, string breadCrumb)
	{
		PropertyInfo[] array = ((!ComparePrivateProperties) ? t1.GetProperties() : t1.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		PropertyInfo[] array2 = array;
		foreach (PropertyInfo propertyInfo in array2)
		{
			if (!propertyInfo.CanRead || (!CompareChildren && IsChildType(propertyInfo.PropertyType)) || ElementsToIgnore.Contains(propertyInfo.Name) || (!CompareReadOnly && !propertyInfo.CanWrite))
			{
				continue;
			}
			if (!IsValidIndexer(propertyInfo, object1, object2, breadCrumb))
			{
				object value = propertyInfo.GetValue(object1, null);
				object value2 = propertyInfo.GetValue(object2, null);
				bool flag = value != null && (value == object1 || _parents.Contains(value));
				bool flag2 = value2 != null && (value2 == object2 || _parents.Contains(value2));
				if (!IsClass(propertyInfo.PropertyType) || !flag || !flag2)
				{
					string breadCrumb2 = AddBreadCrumb(breadCrumb, propertyInfo.Name, string.Empty, -1);
					Compare(value, value2, breadCrumb2);
					if (Differences.Count >= MaxDifferences)
					{
						break;
					}
				}
			}
			else
			{
				CompareIndexer(propertyInfo, object1, object2, breadCrumb);
			}
		}
	}

	private bool IsValidIndexer(PropertyInfo info, object object1, object object2, string breadCrumb)
	{
		ParameterInfo[] indexParameters = info.GetIndexParameters();
		if (indexParameters.Length == 0)
		{
			return false;
		}
		if (indexParameters.Length > 1)
		{
			throw new Exception("Cannot compare objects with more than one indexer for object " + breadCrumb);
		}
		if (indexParameters[0].ParameterType != typeof(int))
		{
			throw new Exception("Cannot compare objects with a non integer indexer for object " + breadCrumb);
		}
		if (info.ReflectedType.GetProperty("Count") == null)
		{
			throw new Exception("Indexer must have a corresponding Count property for object " + breadCrumb);
		}
		if (info.ReflectedType.GetProperty("Count").PropertyType != typeof(int))
		{
			throw new Exception("Indexer must have a corresponding Count property that is an integer for object " + breadCrumb);
		}
		return true;
	}

	private void CompareIndexer(PropertyInfo info, object object1, object object2, string breadCrumb)
	{
		int num = (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object1, new object[0]);
		int num2 = (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object2, new object[0]);
		if (num != num2)
		{
			string arg = AddBreadCrumb(breadCrumb, info.Name, string.Empty, -1);
			Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", arg, num, num2));
			if (Differences.Count >= MaxDifferences)
			{
				return;
			}
		}
		for (int i = 0; i < num; i++)
		{
			string arg = AddBreadCrumb(breadCrumb, info.Name, string.Empty, i);
			object value = info.GetValue(object1, new object[1] { i });
			object value2 = info.GetValue(object2, new object[1] { i });
			Compare(value, value2, arg);
			if (Differences.Count >= MaxDifferences)
			{
				break;
			}
		}
	}

	private void CompareIDictionary(object object1, object object2, string breadCrumb)
	{
		IDictionary dictionary = object1 as IDictionary;
		IDictionary dictionary2 = object2 as IDictionary;
		if (dictionary == null)
		{
			throw new ArgumentNullException("object1");
		}
		if (dictionary2 == null)
		{
			throw new ArgumentNullException("object2");
		}
		try
		{
			_parents.Add(object1);
			_parents.Add(object2);
			if (dictionary.Count != dictionary2.Count)
			{
				Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", breadCrumb, dictionary.Count, dictionary2.Count));
				if (Differences.Count >= MaxDifferences)
				{
					return;
				}
			}
			IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
			IDictionaryEnumerator enumerator2 = dictionary2.GetEnumerator();
			while (enumerator.MoveNext() && enumerator2.MoveNext())
			{
				string breadCrumb2 = AddBreadCrumb(breadCrumb, "Key", string.Empty, -1);
				Compare(enumerator.Key, enumerator2.Key, breadCrumb2);
				if (Differences.Count >= MaxDifferences)
				{
					break;
				}
				breadCrumb2 = AddBreadCrumb(breadCrumb, "Value", string.Empty, -1);
				Compare(enumerator.Value, enumerator2.Value, breadCrumb2);
				if (Differences.Count >= MaxDifferences)
				{
					break;
				}
			}
		}
		finally
		{
			_parents.Remove(object1);
			_parents.Remove(object2);
		}
	}

	private string cStr(object obj)
	{
		try
		{
			if (obj == null)
			{
				return "(null)";
			}
			if (obj == DBNull.Value)
			{
				return "System.DBNull.Value";
			}
			return obj.ToString();
		}
		catch
		{
			return string.Empty;
		}
	}

	private void CompareIList(object object1, object object2, string breadCrumb)
	{
		IList list = object1 as IList;
		IList list2 = object2 as IList;
		if (list == null)
		{
			throw new ArgumentNullException("object1");
		}
		if (list2 == null)
		{
			throw new ArgumentNullException("object2");
		}
		try
		{
			_parents.Add(object1);
			_parents.Add(object2);
			if (list.Count != list2.Count)
			{
				Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", breadCrumb, list.Count, list2.Count));
				if (Differences.Count >= MaxDifferences)
				{
					return;
				}
			}
			IEnumerator enumerator = list.GetEnumerator();
			IEnumerator enumerator2 = list2.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext() && enumerator2.MoveNext())
			{
				string breadCrumb2 = AddBreadCrumb(breadCrumb, string.Empty, string.Empty, num);
				Compare(enumerator.Current, enumerator2.Current, breadCrumb2);
				if (Differences.Count >= MaxDifferences)
				{
					break;
				}
				num++;
			}
		}
		finally
		{
			_parents.Remove(object1);
			_parents.Remove(object2);
		}
	}

	private string AddBreadCrumb(string existing, string name, string extra, string index)
	{
		bool flag = !string.IsNullOrEmpty(index);
		bool flag2 = name.Length > 0;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(existing);
		if (flag2)
		{
			stringBuilder.AppendFormat(".");
			stringBuilder.Append(name);
		}
		stringBuilder.Append(extra);
		if (flag)
		{
			int result = -1;
			if (int.TryParse(index, out result))
			{
				stringBuilder.AppendFormat("[{0}]", index);
			}
			else
			{
				stringBuilder.AppendFormat("[\"{0}\"]", index);
			}
		}
		return stringBuilder.ToString();
	}

	private string AddBreadCrumb(string existing, string name, string extra, int index)
	{
		return AddBreadCrumb(existing, name, extra, (index < 0) ? null : index.ToString());
	}
}
}
