using System;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class GraftekGraft : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 40;

	public int GraftType;

	public override bool SameAs(IPart p)
	{
		if ((p as GraftekGraft).GraftType != GraftType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (GraftType == 0)
		{
			GraftType = Stat.Random(1, 3);
		}
		StatShifter.SetStatShift("Hitpoints", Stat.Random(6, 12), baseValue: true);
		if (GraftType == 1)
		{
			StatShifter.SetStatShift("Strength", Stat.Random(4, 6), baseValue: true);
			StatShifter.SetStatShift("Toughness", Stat.Random(4, 6), baseValue: true);
		}
		else if (GraftType == 2)
		{
			StatShifter.SetStatShift("Agility", Stat.Random(4, 6), baseValue: true);
			StatShifter.SetStatShift("DV", Stat.Random(4, 8), baseValue: true);
		}
		else if (GraftType == 3)
		{
			StatShifter.SetStatShift("AV", Stat.Random(2, 4), baseValue: true);
			StatShifter.SetStatShift("HeatResistance", Stat.Random(50, 75), baseValue: true);
			StatShifter.SetStatShift("ColdResistance", Stat.Random(50, 75), baseValue: true);
		}
		ParentObject.RequirePart<Metal>();
	}

	public override void Remove()
	{
		StatShifter.RemoveStatShifts();
		base.Remove();
	}

	public override bool Render(RenderEvent E)
	{
		string text = null;
		switch (GraftType)
		{
		case 1:
			text = "&c";
			break;
		case 2:
			text = "&c";
			break;
		case 3:
			text = "&C";
			break;
		}
		if (!text.IsNullOrEmpty())
		{
			E.ApplyColors(text, ICON_COLOR_PRIORITY);
		}
		return base.Render(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != GetUnknownShortDescriptionEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject && Cloning.IsCloning(E.Context))
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Object == ParentObject && !E.Reference)
		{
			switch (GraftType)
			{
			case 1:
				E.AddAdjective("grafted");
				E.AddColor('c', 20);
				break;
			case 2:
				E.AddAdjective("cyberized");
				E.AddColor('c', 20);
				break;
			case 3:
				E.AddAdjective("chromed");
				E.AddColor('C', 20);
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AddShortDescription(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		AddShortDescription(E);
		return base.HandleEvent(E);
	}

	public void AddShortDescription(IShortDescriptionEvent E)
	{
		switch (GraftType)
		{
		case 1:
			E.Postfix.Append("\nTwining metallic musculature has been grotesquely merged with =pronouns.possessive= body.");
			break;
		case 2:
			E.Postfix.Append("\nShining metallic veins pulse beneath =pronouns.possessive= skin.");
			break;
		case 3:
			E.Postfix.Append("\n=pronouns.Possessive= skin is tesselated in tiny chrome scales.");
			break;
		}
	}
}
