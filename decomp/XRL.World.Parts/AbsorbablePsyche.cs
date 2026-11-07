using System;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Encounters;

namespace XRL.World.Parts;

[Serializable]
public class AbsorbablePsyche : IPart
{
	public static readonly int ABSORB_CHANCE = 10;

	public int EgoBonus = 1;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != KilledEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(KilledEvent E)
	{
		if (E.Dying.GetPsychicGlimmer() >= DimensionManager.GLIMMER_FLOOR)
		{
			if (ParentObject.GetPrimaryFaction() == "Seekers")
			{
				E.Reason = "You were resorbed into the Mass Mind.";
				E.ThirdPersonReason = E.Dying.Does("were", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " @@resorbed into the Mass Mind.";
			}
			else if (ABSORB_CHANCE.in100())
			{
				E.Reason = "Your psyche exploded, and its psionic bits were encoded on the holographic boundary surrounding the psyche of " + ParentObject.GetReferenceDisplayName() + ".";
				E.ThirdPersonReason = E.Dying.Its + " psyche exploded, and its psionic bits were encoded on the holographic boundary surrounding the psyche of " + Grammar.MakePossessive(ParentObject.BaseDisplayName) + ".";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (EgoBonus > 0)
		{
			GameObject killer = E.Killer;
			if (killer != null && killer.IsPlayer())
			{
				int egoBonus = EgoBonus;
				EgoBonus = 0;
				if (ABSORB_CHANCE.in100())
				{
					if (Popup.ShowYesNo("At the moment of victory, your swelling ego curves the psychic aether and causes the psyche of " + ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true) + " to collide with your own. As the weaker of the two, its binding energy is exceeded and it explodes. Would you like to encode its psionic bits on the holographic boundary of your own psyche?\n\n(+1 Ego permanently)") == DialogResult.Yes)
					{
						IComponent<GameObject>.ThePlayer.GetStat("Ego").BaseValue += egoBonus;
						Popup.Show("You encode the psyche of " + ParentObject.BaseDisplayName + " and gain +{{C|1}} {{Y|Ego}}!");
						JournalAPI.AddAccomplishment("You slew " + ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true) + " and encoded their psyche's psionic bits on the holographic boundary of your own psyche.", "After a climactic battle of wills, =name= slew " + ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true) + " and absorbed " + ParentObject.its + " psyche, thickening toward Godhood.", $"In the month of {Calendar.GetMonth()} of {Calendar.GetYear()}, =name= was challenged by <spice.commonPhrases.pretender.!random.article> to a duel over the rights of {Factions.GetMostLikedFormattedName()}. =name= won and had the pretender's psyche kibbled and absorbed into {The.Player.GetPronounProvider().PossessiveAdjective} own.", null, "general", MuralCategory.Slays, MuralWeight.High, null, -1L);
						Achievement.ABSORB_PSYCHE.Unlock();
					}
					else
					{
						Popup.Show("You pause as the psyche of " + ParentObject.BaseDisplayName + " radiates into nothingness.");
						JournalAPI.AddAccomplishment("You slew " + ParentObject.GetReferenceDisplayName() + " and watched their psyche radiate into nothingness.", "After a climactic battle of wills, =name= slew " + ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true) + " and watched " + ParentObject.its + " psyche radiate into nothingness.", $"In the month of {Calendar.GetMonth()} of {Calendar.GetYear()}, =name= was challenged by <spice.commonPhrases.pretender.!random.article> to a duel over the rights of {Factions.GetMostLikedFormattedName()}. =name= won and had the pretender's psyche kibbled and radiated into nothingness.", null, "general", MuralCategory.Slays, MuralWeight.Medium, null, -1L);
					}
				}
				else
				{
					JournalAPI.AddAccomplishment("You slew " + ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true) + ".", "After a climactic battle of wills, =name= slew " + ParentObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: true) + ".", $"In the month of {Calendar.GetMonth()} of {Calendar.GetYear()}, =name= was challenged by <spice.commonPhrases.pretender.!random.article> to a duel over the rights of {Factions.GetMostLikedFormattedName()}. =name= won and slew the pretender, charitably not radiating <spice.pronouns.possessive.!random> psyche into nothingness.", null, "general", MuralCategory.Slays, MuralWeight.Medium, null, -1L);
				}
			}
		}
		return base.HandleEvent(E);
	}
}
