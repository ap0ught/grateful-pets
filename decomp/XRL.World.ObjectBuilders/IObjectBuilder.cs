namespace XRL.World.ObjectBuilders;

/// <summary>A game object builder singleton.</summary>
public abstract class IObjectBuilder
{
	public virtual void Initialize()
	{
	}

	public abstract void Apply(GameObject Object, string Context);
}
