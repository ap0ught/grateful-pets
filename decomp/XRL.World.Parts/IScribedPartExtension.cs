namespace XRL.World.Parts;

public abstract class IScribedPartExtension<T> : IPartExtension<T> where T : IPart
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
