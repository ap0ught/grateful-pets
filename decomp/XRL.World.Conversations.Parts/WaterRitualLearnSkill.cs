using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.UI;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World.Conversations.Parts;

public class WaterRitualLearnSkill : IWaterRitualPart
{
	public string DisplayName;

	public string Text;

	public string Context = "WaterRitual,LearnSkill";

	public SkillEntry ParentSkill;

	public IBaseSkillEntry Entry;

	public int Points;

	public bool Disambiguate;

	public bool TagName = true;

	public string InitiatoryKey => BaseInitiatorySkill.GetInitiatoryKey(The.Speaker, Entry);

	public IBaseSkillEntry InitiatorySkill => BaseInitiatorySkill.GetSkillFor(The.Player, ParentSkill);

	public override bool Available => The.Player.Stat("SP") >= Points;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override void Awake()
	{
		if (The.Speaker.HasProperty("WaterRitualNoSellSkill"))
		{
			return;
		}
		string text = The.Speaker.GetStringProperty("WaterRitual_Skill") ?? The.Speaker.GetxTag("WaterRitual", "SellSkill") ?? (WaterRitual.Alternative ? WaterRitual.RecordFaction.WaterRitualAltSkill : WaterRitual.RecordFaction.WaterRitualSkill);
		if (text.IsNullOrEmpty())
		{
			return;
		}
		if (SkillFactory.Factory.TryGetFirstEntry(text, out Entry))
		{
			if (Entry is SkillEntry skillEntry)
			{
				ParentSkill = skillEntry;
				if (Entry.Cost == 0)
				{
					Entry = skillEntry.PowerList.FirstOrDefault((PowerEntry x) => x.Cost > 0) ?? Entry;
				}
			}
			else if (Entry is PowerEntry powerEntry)
			{
				ParentSkill = powerEntry.ParentSkill;
				if (ParentSkill != null && (Entry.Cost == 0 || ParentSkill.Initiatory))
				{
					Entry = ParentSkill;
				}
			}
		}
		if (Entry == null)
		{
			return;
		}
		TagName = true;
		DisplayName = Entry.Name;
		Reputation = Entry.Cost;
		Points = 0;
		if (!Entry.Generic.IsValidForElement(ParentElement, this, Context))
		{
			return;
		}
		if (ParentSkill != null && ParentSkill.Initiatory)
		{
			if (The.Player.GetIntProperty(InitiatoryKey) > 0 || InitiatorySkill == null)
			{
				return;
			}
		}
		else if (The.Player.HasSkill(Entry.Class))
		{
			return;
		}
		Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Skill", Reputation);
		Disambiguate = ShouldDisambiguatePower();
		Visible = true;
	}

	public void Unlock()
	{
		if (!UseReputation())
		{
			return;
		}
		string text = Entry.Class;
		if (ParentSkill != null && ParentSkill.Initiatory)
		{
			text = InitiatorySkill?.Class;
			if (text.IsNullOrEmpty())
			{
				BaseInitiatorySkill.ShowCompletedPopup(The.Player, The.Speaker, ParentSkill, Context);
				return;
			}
			The.Player.SetIntProperty(InitiatoryKey, The.Player.Stat("Level"));
		}
		The.Player.AddSkill(text, The.Speaker, Context);
	}

	public bool ShouldDisambiguatePower()
	{
		if (Entry is SkillEntry)
		{
			return false;
		}
		if (ParentSkill == null)
		{
			return false;
		}
		return Enumerable.Count(SkillFactory.Factory.PowersByClass, (KeyValuePair<string, PowerEntry> Pair) => Pair.Value.Name == Entry.Name && Pair.Value.Class != Entry.Class) >= 1;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (The.Player.Stat("SP") < Points)
		{
			Popup.ShowFail("You don't have enough skill points.");
			return false;
		}
		Unlock();
		if (Points > 0)
		{
			The.Player.GetStat("SP").Penalty += Points;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (!Text.IsNullOrEmpty())
		{
			E.Text.Clear().Append(Text);
		}
		E.Text.Replace("=skill.name=", DisplayName);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("{{").Append(Lowlight).Append("|[");
		if (TagName)
		{
			stringBuilder.Append("learn {{W|").Append(DisplayName);
			if (Disambiguate)
			{
				stringBuilder.Compound('(').Append(ParentSkill.Name).Append(')');
			}
			stringBuilder.Append("}}: ");
		}
		stringBuilder.Append("{{").Append(Numeric).Append("|")
			.Append(GetReputationCost())
			.Append("}} reputation");
		if (Points > 0)
		{
			stringBuilder.Append(", {{C|-").Append(Points).Append("}} SP");
		}
		E.Tag = stringBuilder.Append("]}}").ToString();
		return false;
	}
}
