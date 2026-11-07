using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XRL.Serialization;

internal sealed class SimpleDeserializationBinder : SerializationBinder
{
	private Regex _assemRegex = new Regex("(?<assembly>^.*?),.*");

	private Regex _typeRegex = new Regex("(?<type>.*?),(?<assembly>.*?),.*(?<end>]])");

	public override Type BindToType(string assemblyName, string typeName)
	{
		Debug.Log("bind@" + assemblyName + "," + typeName);
		Match match = _assemRegex.Match(assemblyName);
		if (match.Success)
		{
			assemblyName = match.Groups["assembly"].Value;
		}
		match = _typeRegex.Match(typeName);
		if (match.Success)
		{
			typeName = string.Format("{0},{1}{2}", match.Groups["type"].Value, match.Groups["assembly"].Value, match.Groups["end"].Value);
		}
		Type type = null;
		type = Type.GetType($"{typeName}, {assemblyName}");
		if (type == null)
		{
			type = ModManager.ResolveType(typeName);
		}
		return type;
	}
}
