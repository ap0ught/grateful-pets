using System;

namespace XRL.World.Units;

[Serializable]
public abstract class GameObjectUnit
{
	public virtual void Initialize(GameObject Object)
	{
	}

	public virtual void Apply(GameObject Object)
	{
	}

	public virtual void Remove(GameObject Object)
	{
	}

	public virtual void Reset()
	{
	}

	/// <summary>Whether the unit description should be written as rules text in the object description.</summary>
	public virtual bool CanInscribe()
	{
		return true;
	}

	/// <summary>Get description of unit effects.</summary>
	/// <param name="Inscription">Returned text will be used as inscription.</param>
	public virtual string GetDescription(bool Inscription = false)
	{
		return "";
	}

	public override string ToString()
	{
		return base.ToString();
	}
}
