using System;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhotosyntheticSkin_SatedUnit : ProceduralCookingEffectUnit
{
	public int Tier = 1;

	public override void Init(GameObject target)
	{
		PhotosyntheticSkin part = target.GetPart<PhotosyntheticSkin>();
		if (part != null)
		{
			Tier = part.Level;
		}
	}

	public override string GetDescription()
	{
		return "";
	}

	public override string GetTemplatedDescription()
	{
		return "";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "BeginTakeAction");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.GetPart<Stomach>()?.GetHungry();
		Object.UnregisterEffectEvent(parent, "BeginTakeAction");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			parent.Object.GetPart<Stomach>()?.ResetCookingCounter();
			parent.Duration--;
		}
	}
}
