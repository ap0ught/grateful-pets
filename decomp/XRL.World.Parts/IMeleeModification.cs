using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IMeleeModification : IModification
{
	public IMeleeModification()
	{
	}

	public IMeleeModification(int Tier)
		: base(Tier)
	{
	}

	public override bool ModificationApplicable(GameObject obj)
	{
		if (obj.HasPart<MissileWeapon>() || obj.GetBlueprint().InheritsFrom("BaseMissileWeapon"))
		{
			return false;
		}
		return true;
	}
}
