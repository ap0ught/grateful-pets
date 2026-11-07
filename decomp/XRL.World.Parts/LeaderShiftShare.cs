using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LeaderShiftShare : IPart
{
	public string RequiresAncestor;

	public string RequiresPart;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EquipperEquippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquipperEquippedEvent E)
	{
		GameObject partyLeader = ParentObject.PartyLeader;
		if (partyLeader == null)
		{
			return base.HandleEvent(E);
		}
		if ((RequiresAncestor.IsNullOrEmpty() || E.Item.GetBlueprint().DescendsFrom(RequiresAncestor)) && (RequiresPart.IsNullOrEmpty() || E.Item.HasPart(RequiresPart)))
		{
			Effect effect = partyLeader.GetEffect((Effect fx) => fx is ShiftShareEffect shiftShareEffect && shiftShareEffect.Source == E.Item);
			if (effect != null)
			{
				partyLeader.RemoveEffect(effect);
			}
			partyLeader.ApplyEffect(new ShiftShareEffect
			{
				Duration = 9999,
				Source = E.Item
			});
		}
		return base.HandleEvent(E);
	}
}
