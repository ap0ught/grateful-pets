using System;

namespace XRL.World.Effects;

[ComponentExtension]
public abstract class IEffectExtension<T> : Effect where T : Effect
{
	public Guid ParentEffectID;

	[NonSerialized]
	public T ParentEffect;

	public override string ApplySound => null;

	public override string RemoveSound => null;

	protected IEffectExtension()
	{
		Duration = 9999;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == EffectRemovedEvent.ID;
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (E.Effect == ParentEffect)
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override void FinalizeRead(SerializationReader Reader)
	{
		if (!Object.TryGetEffect<T>(ParentEffectID, out ParentEffect))
		{
			MetricsManager.LogAssemblyWarning(GetType(), "Lost parent effect reference, removing " + GetType().GetName() + " from " + Object.DebugName);
			Object.RemoveEffect(this);
		}
	}
}
