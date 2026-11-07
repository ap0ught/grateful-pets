using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Astral : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != EnteredCellEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		EnsurePhased();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		EnsurePhased();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("stars", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override string GetDescription()
	{
		return "You live in an alternate plane of reality.";
	}

	private void EnsurePhased()
	{
		if (!ParentObject.HasEffect(typeof(Phased)) && CheckMyRealityDistortionUsability())
		{
			ParentObject.ApplyEffect(new Phased(9999));
		}
	}
}
