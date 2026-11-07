using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedTeleportation : ModImprovedMutationBase<Teleportation>
{
	public ModImprovedTeleportation()
	{
	}

	public ModImprovedTeleportation(int Tier)
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
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}
}
