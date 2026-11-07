using System;
using XRL.World;

namespace XRL;

[Serializable]
public abstract class IScribedSystem : IGameSystem
{
	public sealed override bool WantFieldReflection => false;

	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteNamedFields(this, GetType());
	}

	public override void Read(SerializationReader Reader)
	{
		Reader.ReadNamedFields(this, GetType());
	}
}
