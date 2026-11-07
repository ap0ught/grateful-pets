namespace XRL.World;

/// <summary>
/// Contracts a type as composited entirely from public fields supported by <see cref="T:XRL.Serialization.FastSerialization" />
/// and/or handling its own serialization.
/// </summary>
public interface IComposite
{
	/// <summary>
	/// If true, public instance field values will be serialized via reflection.
	/// </summary>
	bool WantFieldReflection => true;

	void Write(SerializationWriter Writer)
	{
	}

	void Read(SerializationReader Reader)
	{
	}
}
