using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhotosyntheticSkin_RegenerationUnit : ProceduralCookingEffectUnit
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
		return (20 + Tier * 10).Signed() + "% to natural healing rate";
	}

	public override string GetTemplatedDescription()
	{
		return "+20% + (Photosynthetic Skin level * 10)% to natural healing rate";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "Regenerating2");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "Regenerating2");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "Regenerating2")
		{
			float num = 1f + (float)(20 + Tier * 10) * 0.01f;
			int value = (int)Math.Ceiling((float)E.GetIntParameter("Amount") * num);
			E.SetParameter("Amount", value);
		}
	}
}
