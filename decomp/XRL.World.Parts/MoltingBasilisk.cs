using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class MoltingBasilisk : IPart
{
	public bool Created;

	public int Puffed;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != SingletonEvent<BeginTakeActionEvent>.ID || Created) && ID != EnteredCellEvent.ID)
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		SyncState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		SyncState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Created)
		{
			Created = true;
			Zone currentZone = ParentObject.CurrentZone;
			if (currentZone != null && !currentZone.Built)
			{
				int num = Stat.Random(7, 11);
				for (int i = 0; i < num; i++)
				{
					int num2 = 20;
					while (num2-- > 0)
					{
						int x = Stat.Random(0, currentZone.Width - 1);
						int y = Stat.Random(0, currentZone.Height - 1);
						Cell cell = currentZone.GetCell(x, y);
						if (cell.IsEmpty())
						{
							cell.AddObject("Molting Basilisk Husk");
							break;
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AICombatStart");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AICombatStart")
		{
			SyncState();
		}
		return base.FireEvent(E);
	}

	public void SyncState()
	{
		Description part = ParentObject.GetPart<Description>();
		if (ParentObject.Target == null)
		{
			part.Short = "The sloughed off skin is dull quartz and statuesque.";
			ParentObject.DisplayName = "molting basilisk husk";
			ParentObject.SetIntProperty("HideCon", 1);
		}
		else
		{
			part.Short = "A lizard of quartz scales reposes in the stillness of an artist's mould. When prey gets too comfortable with =pronouns.possessive= lifelessness and traipeses by, =pronouns.subjective= =verb:quicken:afterpronoun= and snaps like a thunder clap.";
			ParentObject.DisplayName = "molting basilisk";
			ParentObject.RemoveIntProperty("HideCon");
		}
	}
}
