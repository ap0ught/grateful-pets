namespace XRL.World.AI;

/// <remarks>
/// As of yet only for hostility debugging purposes, may be leveraged somewhere player-facing in the future.
/// </remarks>
public abstract class IAllyReason : IComposite
{
	public enum ReplaceTarget
	{
		None,
		Source,
		Type
	}

	public long Time;

	public virtual bool WantFieldReflection => false;

	/// <summary>Replace other allegiances with this type of reason.</summary>
	public virtual ReplaceTarget Replace => ReplaceTarget.None;

	public virtual void Initialize(GameObject Actor, GameObject Source, AllegianceSet Set)
	{
	}

	public virtual string GetText(GameObject Actor)
	{
		return null;
	}

	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Time);
	}

	public virtual void Read(SerializationReader Reader)
	{
		Time = Reader.ReadOptimizedInt64();
	}
}
