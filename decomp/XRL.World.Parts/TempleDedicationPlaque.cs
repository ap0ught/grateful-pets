using System;
using System.Text;
using HistoryKit;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class TempleDedicationPlaque : IPlaquePart
{
	public string Prefix = "";

	public string Postfix = "";

	public override string BaseInscription
	{
		get
		{
			string name = "TempleDedicationPlaque";
			GameObject gameObject = GetAnyBasisZone().GetCell(0, 0).RequireObject("Widget");
			string stringProperty = gameObject.GetStringProperty(name);
			if (!stringProperty.IsNullOrEmpty())
			{
				return stringProperty;
			}
			stringProperty = GenerateInscription();
			gameObject.SetStringProperty(name, stringProperty);
			return stringProperty;
		}
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Look.ShowLooker(0, cell.X, cell.Y);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Prefix.Append(Prefix);
		E.Base.Clear();
		E.Base.Append(Inscription);
		E.Postfix.Append(Postfix);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public string GenerateInscription()
	{
		Random seededRandomGenerator = Stat.GetSeededRandomGenerator("TempleDedicationPlaque");
		StringBuilder stringBuilder = Event.NewStringBuilder("This temple was built in 638,01qy by the Exhaustiers' Guild, who detached from their egregore ");
		stringBuilder.Append(Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.egregorePrefix.!random> <spice.nouns.!random>", seededRandomGenerator)));
		stringBuilder.Append(" in the ");
		stringBuilder.Append(Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.nouns.!random> Era.", seededRandomGenerator)));
		return Event.FinalizeString(stringBuilder);
	}
}
