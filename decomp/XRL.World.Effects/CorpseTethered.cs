using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class CorpseTethered : Effect
{
	public CorpseTethered()
	{
		DisplayName = "{{Y|tomb-tethered}}";
		Duration = 1;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public void CheckCell()
	{
		if (Object == null)
		{
			Duration = 0;
		}
		else if (!Object.HasMarkOfDeath())
		{
			Duration = 0;
		}
		else if (Object.GetCurrentCell() == null)
		{
			Duration = 0;
		}
		else if (!Object.GetCurrentCell().HasObject("AnchorRoomTile"))
		{
			Duration = 0;
		}
	}

	public override string GetDescription()
	{
		CheckCell();
		if (Duration <= 0)
		{
			return null;
		}
		return "{{Y|tomb-tethered}}";
	}

	public override bool SuppressInLookDisplay()
	{
		CheckCell();
		return Duration <= 0;
	}

	public override string GetDetails()
	{
		return "Currently occupying a tile safe from the tolling teleportation of the Bell of Rest.";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				E.Tile = null;
				E.RenderString = "\u0019";
				E.ColorString = "&Y^k";
				return false;
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckCell();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Reference)
		{
			CheckCell();
			if (Duration > 0)
			{
				E.AddTag("{{y|[{{Y|tomb-tethered}}]}}");
			}
		}
		return base.HandleEvent(E);
	}
}
