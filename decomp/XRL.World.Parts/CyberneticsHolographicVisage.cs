using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsHolographicVisage : IPart
{
	public static readonly int AMOUNT = 200;

	public string CommandID;

	public string Faction;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == PooledEvent<AfterPlayerBodyChangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		SelectVisage(E.Implantee);
		ApplyVisage(E.Implantee);
		ActivatedAbilityID = E.Implantee.AddDynamicCommand(out CommandID, "AlterHolographicVisage", "Alter Visage", "Cybernetics", null, "\u0002");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		UnapplyVisage(E.Implantee);
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		if (!Faction.IsNullOrEmpty())
		{
			GameObject implantee = ParentObject.Implantee;
			if (E.NewBody == implantee)
			{
				The.Game?.PlayerReputation?.Modify(Faction, AMOUNT, null, null, null, Silent: true, Transient: true);
			}
			else if (E.OldBody == implantee)
			{
				The.Game?.PlayerReputation?.Modify(Faction, -AMOUNT, null, null, null, Silent: true, Transient: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!Faction.IsNullOrEmpty())
		{
			E.Postfix.AppendRules(AMOUNT.Signed() + " reputation with " + XRL.World.Faction.GetFormattedName(Faction));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == CommandID && E.Actor == ParentObject.Implantee)
		{
			UnapplyVisage(E.Actor);
			SelectVisage(E.Actor);
			ApplyVisage(E.Actor);
			E.Actor.UseEnergy(1000, "Cybernetics HolographicVisage");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void ApplyVisage(GameObject Subject)
	{
		if (!Faction.IsNullOrEmpty() && Subject != null && Subject.IsPlayer())
		{
			The.Game?.PlayerReputation?.Modify(Faction, AMOUNT, null, null, null, Silent: true, Transient: true);
		}
	}

	private void UnapplyVisage(GameObject Subject)
	{
		if (!Faction.IsNullOrEmpty() && Subject != null && Subject.IsPlayer())
		{
			The.Game?.PlayerReputation?.Modify(Faction, -AMOUNT, null, null, null, Silent: true, Transient: true);
		}
	}

	private void SelectVisage(GameObject Subject)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return;
		}
		List<Faction> list = new List<Faction>(Factions.GetFactionCount());
		foreach (Faction item in Factions.Loop())
		{
			if (item.Visible)
			{
				list.Add(item);
			}
		}
		list.Sort((Faction a, Faction b) => a.DisplayName.CompareTo(b.DisplayName));
		List<string> list2 = new List<string>(list.Count + 1);
		list2.Add("none");
		foreach (Faction item2 in list)
		{
			list2.Add(item2.DisplayName);
		}
		if (Subject.IsPlayer())
		{
			int num = Popup.PickOption("Choose a model faction for your holographic glamour.", null, "", "Sounds/UI/ui_notification", list2.ToArray());
			if (num == 0)
			{
				Faction = null;
			}
			else
			{
				if (num == -1)
				{
					num = Stat.Random(1, list.Count);
				}
				Faction = list[num - 1].Name;
			}
		}
		else
		{
			Faction = list.GetRandomElement()?.Name;
		}
		if (!Faction.IsNullOrEmpty())
		{
			IComponent<GameObject>.EmitMessage(Subject, "In a glissade of light, " + Subject.poss("visage") + " morphs into an image pleasing to " + XRL.World.Faction.GetFormattedName(Faction) + ".", ' ', FromDialog: true);
		}
	}
}
