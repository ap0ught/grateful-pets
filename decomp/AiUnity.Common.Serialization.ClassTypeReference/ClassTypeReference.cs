using System;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using UnityEngine;

namespace AiUnity.Common.Serialization.ClassTypeReference;

[Serializable]
public sealed class ClassTypeReference : ISerializationCallbackReceiver, IValidateTypeReference
{
	[SerializeField]
	[HideInInspector]
	private string _classRef;

	private Type _type;

	public Type Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (value != null && !value.IsClass)
			{
				throw new ArgumentException($"'{value.FullName}' is not a class type.", "value");
			}
			_type = value;
			_classRef = GetClassRef(value);
		}
	}

	private static IInternalLogger Logger => Singleton<CommonInternalLogger>.Instance;

	public ClassTypeReference()
	{
	}

	public ClassTypeReference(string assemblyQualifiedClassName)
	{
		Type = ((!string.IsNullOrEmpty(assemblyQualifiedClassName)) ? Type.GetType(assemblyQualifiedClassName) : null);
	}

	public ClassTypeReference(Type type)
	{
		Type = type;
	}

	public static string GetClassRef(Type type)
	{
		if (!(type != null))
		{
			return string.Empty;
		}
		return type.FullName + ", " + type.Assembly.GetName().Name;
	}

	public bool IsValidType()
	{
		return Type != null;
	}

	public override string ToString()
	{
		if (!(Type != null))
		{
			return "(None)";
		}
		return Type.FullName;
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (!string.IsNullOrEmpty(_classRef))
		{
			_type = Type.GetType(_classRef);
			if (_type == null)
			{
				Debug.LogWarning($"'{_classRef}' was referenced but class type was not found.");
			}
		}
		else
		{
			_type = null;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	public static implicit operator ClassTypeReference(Type type)
	{
		return new ClassTypeReference(type);
	}

	public static implicit operator string(ClassTypeReference typeReference)
	{
		return typeReference._classRef;
	}

	public static implicit operator Type(ClassTypeReference typeReference)
	{
		return typeReference.Type;
	}
}
