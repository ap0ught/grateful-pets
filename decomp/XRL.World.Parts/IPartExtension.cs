using System;

namespace XRL.World.Parts;

[ComponentExtension]
public abstract class IPartExtension<T> : IPart where T : IPart
{
	[NonSerialized]
	public T ParentPart;

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == PooledEvent<PartRemovedEvent>.ID;
	}

	public override bool HandleEvent(PartRemovedEvent E)
	{
		if (E.Part == ParentPart)
		{
			E.Actor.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override void FinalizeRead(SerializationReader Reader)
	{
		if (!ParentObject.TryGetPart<T>(out ParentPart))
		{
			MetricsManager.LogAssemblyWarning(GetType(), "Lost parent part reference, removing " + GetType().GetName() + " from " + ParentObject.DebugName);
			ParentObject.RemovePart(this);
		}
	}
}
