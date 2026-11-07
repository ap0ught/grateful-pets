using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class OmniphaseWhileFrozen : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override void Remove()
	{
		Omniphase linkedEffect = GetLinkedEffect();
		if (linkedEffect != null)
		{
			ParentObject.RemoveEffect(linkedEffect);
		}
		base.Remove();
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (!HasLinkedEffect())
		{
			ParentObject.ForceApplyEffect(new Omniphase(9999, Name));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!HasLinkedEffect())
		{
			ParentObject.ForceApplyEffect(new Omniphase(9999, Name));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (!ParentObject.IsFrozen())
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public Omniphase GetLinkedEffect()
	{
		int i = 0;
		for (int count = ParentObject.Effects.Count; i < count; i++)
		{
			if (ParentObject.Effects[i] is Omniphase omniphase && omniphase.SourceKey == Name)
			{
				return omniphase;
			}
		}
		return null;
	}

	public bool HasLinkedEffect()
	{
		int i = 0;
		for (int count = ParentObject.Effects.Count; i < count; i++)
		{
			if (ParentObject.Effects[i] is Omniphase omniphase && omniphase.SourceKey == Name)
			{
				return true;
			}
		}
		return false;
	}
}
