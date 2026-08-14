using System;
using System.Reflection;
using System.Runtime.Serialization;

public sealed class AnyExecutingAssemblyBinder : SerializationBinder
{
	public override Type BindToType(string assemblyName, string typeName)
	{
		if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		string fullName = Assembly.GetExecutingAssembly().FullName;
		assemblyName = fullName;
		return Type.GetType($"{typeName}, {assemblyName}");
	}
}
