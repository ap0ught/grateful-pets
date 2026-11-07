using System;
using System.Linq;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class IsAFollowerGettingTeleport : Effect
{
	public Guid effectGuid = Guid.Empty;

	public IsAFollowerGettingTeleport()
	{
		Duration = 3;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (effectGuid == Guid.Empty)
		{
			ItemEffectBonusMutation<Teleportation> itemEffectBonusMutation = new ItemEffectBonusMutation<Teleportation>();
			itemEffectBonusMutation.DisplayName = "{{C|spurred to teleport}}";
			if (Object.ApplyEffect(itemEffectBonusMutation))
			{
				effectGuid = itemEffectBonusMutation.ID;
			}
		}
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		if (effectGuid != Guid.Empty)
		{
			Effect effect = Object.Effects.Where((Effect e) => e.ID == effectGuid).FirstOrDefault();
			if (effect != null)
			{
				effect.Duration = 0;
				effect.Remove(Object);
			}
		}
		base.Remove(Object);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Duration--;
		return base.HandleEvent(E);
	}
}
