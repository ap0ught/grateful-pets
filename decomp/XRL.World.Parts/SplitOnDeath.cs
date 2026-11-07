using System;

namespace XRL.World.Parts;

[Serializable]
public class SplitOnDeath : IPart
{
	public int Chance = 100;

	public string Blueprint;

	public string Amount;

	public string Message;

	public override bool SameAs(IPart Part)
	{
		SplitOnDeath splitOnDeath = Part as SplitOnDeath;
		if (splitOnDeath.Chance != Chance)
		{
			return false;
		}
		if (splitOnDeath.Blueprint != Blueprint)
		{
			return false;
		}
		if (splitOnDeath.Amount != Amount)
		{
			return false;
		}
		if (splitOnDeath.Message != Message)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (!Amount.IsNullOrEmpty() && !Blueprint.IsNullOrEmpty() && !ParentObject.IsNowhere() && Chance.in100())
		{
			Split();
		}
		return base.HandleEvent(E);
	}

	public void Split()
	{
		Cell cell = ParentObject.CurrentCell;
		int num = Amount.RollCached();
		if (num <= 0)
		{
			return;
		}
		if (!Message.IsNullOrEmpty())
		{
			EmitMessage(GameText.VariableReplace(Message, ParentObject), IComponent<GameObject>.ConsequentialColorChar(ParentObject));
		}
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = GameObject.Create(Blueprint);
			gameObject.TakeBaseAllegiance(ParentObject);
			gameObject.CopyTarget(ParentObject);
			gameObject.CopyLeader(ParentObject);
			if (i == 0)
			{
				cell.AddObject(gameObject);
			}
			else
			{
				cell.getClosestEmptyCell().AddObject(gameObject);
			}
		}
	}
}
