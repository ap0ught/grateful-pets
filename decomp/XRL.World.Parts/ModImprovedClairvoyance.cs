using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedClairvoyance : ModImprovedMutationBase<Clairvoyance>
{
	public ModImprovedClairvoyance()
	{
	}

	public ModImprovedClairvoyance(int Tier)
		: base(Tier)
	{
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("glass", 1);
		}
		return base.HandleEvent(E);
	}
}
