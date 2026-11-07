using System;

namespace XRL.World;

[Serializable]
public abstract class IScribedEffect : Effect
{
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteNamedFields(this, GetType());
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Reader.ReadNamedFields(this, GetType());
	}
}
