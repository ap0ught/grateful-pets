using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class UnstableGenome : BaseMutation
{
	public override bool CanLevel()
	{
		return false;
	}

	public override bool ShouldShowLevel()
	{
		return true;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<BeforeLevelGainedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeLevelGainedEvent E)
	{
		if (E.Actor == ParentObject && E.Actor.IsPlayer())
		{
			if (E.Pass == 1)
			{
				if (Stat.Random(1, 100) <= 33)
				{
					E.Handlers.Enqueue(this);
				}
			}
			else
			{
				StatusScreen.BuyRandomMutation(ParentObject);
				if (Level == 1)
				{
					ParentObject.GetPart<Mutations>().RemoveMutation(this);
				}
				else
				{
					Level--;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You gain one extra mutation each time you buy this, but the mutations don't manifest right away.\nWhenever you gain a level, there's a 33% chance that your genome destabilizes and you get to choose from 3 random mutations.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}
}
