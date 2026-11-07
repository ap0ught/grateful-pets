using System;
using UnityEngine;
using XRL.World;

namespace XRL.Serialization;

internal sealed class ColorSerializationSurrogate : IFastSerializationTypeSurrogate
{
	public object Deserialize(SerializationReader reader, Type type)
	{
		return new Color
		{
			r = reader.ReadSingle(),
			g = reader.ReadSingle(),
			b = reader.ReadSingle(),
			a = reader.ReadSingle()
		};
	}

	public void Serialize(SerializationWriter writer, object value)
	{
		Color color = (Color)value;
		writer.Write(color.r);
		writer.Write(color.g);
		writer.Write(color.b);
		writer.Write(color.a);
	}

	public bool SupportsType(Type type)
	{
		return type == typeof(Color);
	}
}
