namespace XRL.World.Effects;

public abstract class IScribedEffectExtension<T> : IEffectExtension<T> where T : Effect
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
