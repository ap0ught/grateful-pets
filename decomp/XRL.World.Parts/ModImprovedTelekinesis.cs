using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedTelekinesis : ModImprovedMutationBase<Telekinesis>
{
	public ModImprovedTelekinesis()
	{
	}

	public ModImprovedTelekinesis(int Tier)
		: base(Tier)
	{
		base.Tier = Tier;
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
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}
}
