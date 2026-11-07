using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhotosyntheticSkin_UnitQuickness : ProceduralCookingEffectUnit
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
		return (13 + Tier * 2).Signed() + " Quickness";
	}

	public override string GetTemplatedDescription()
	{
		return "+13 + (Photosynthetic Skin level * 2) Quickness";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.Statistics["Speed"].Bonus += 13 + Tier * 2;
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.Statistics["Speed"].Bonus -= 13 + Tier * 2;
	}
}
