using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Capabilities;

public static class Firefighting
{
	public static bool CanAttemptFirefighting(GameObject Actor, GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = Actor;
		}
		BodyPart bodyPart = Actor?.Body?.GetFirstPart("Hands");
		if (Actor != Subject)
		{
			if (bodyPart == null)
			{
				return false;
			}
			if (Subject.CurrentCell != null && Actor.CurrentCell != null && Subject.IsFlying && !Actor.IsFlying)
			{
				return false;
			}
		}
		else if (bodyPart == null)
		{
			if (Actor.IsFlying)
			{
				return false;
			}
			if (!Actor.PhaseMatches(1))
			{
				return false;
			}
		}
		if (Actor.isDamaged(50, inclusive: true) && Actor.Stat("Intelligence") >= 7 && Actor.Stat("Willpower") >= 7)
		{
			return true;
		}
		return false;
	}

	public static bool NeedsToLandToAttemptFirefighting(GameObject Actor, GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = Actor;
		}
		if (Actor != Subject)
		{
			return false;
		}
		if (Actor?.Body?.GetFirstPart("Hands") == null && Actor.IsFlying && Actor.PhaseMatches(1))
		{
			return true;
		}
		return false;
	}

	public static bool AttemptFirefighting(GameObject Actor, GameObject Subject, int EnergyCost = 1000, bool Automatic = false, bool Dialog = false)
	{
		bool flag = AttemptFirefightingCore(Actor, Subject, EnergyCost, Automatic, Dialog);
		if (flag && EnergyCost > 0)
		{
			Actor.UseEnergy(1000, Automatic ? "Pass Firefighting" : "Firefighting");
		}
		return flag;
	}

	private static bool AttemptFirefightingCore(GameObject Actor, GameObject Subject, int EnergyCost, bool Automatic, bool Dialog)
	{
		BodyPart bodyPart = Actor?.Body?.GetFirstPart("Hands");
		if (Subject.CurrentCell != null && Actor.CurrentCell != null && Subject.IsFlying && !Actor.IsFlying)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You cannot reach " + Subject.the + Subject.ShortDisplayName + "!");
			}
			return false;
		}
		if (Actor == Subject && Actor.HasEffect<Prone>() && !Actor.IsFlying && Actor.PhaseMatches(1))
		{
			Messaging.XDidY(Actor, "roll", "on the ground", "!", null, null, Actor, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Dialog);
			int num = GetFirefightingPerformanceEvent.GetFor(Actor, null, Patting: false, Rolling: true);
			if (num != 0)
			{
				Actor.TemperatureChange(num, null, Radiant: false, MinAmbient: true, MaxAmbient: false, IgnoreResistance: false, 5);
			}
			return true;
		}
		if (bodyPart != null)
		{
			if (Actor != Subject)
			{
				if (Subject.IsHostileTowards(Actor) && Subject.IsMobile())
				{
					int combatDV = Stats.GetCombatDV(Subject);
					if (Stat.Random(1, 20) + Actor.StatMod("Agility") < combatDV)
					{
						Messaging.XDidYToZ(Actor, "try", "to beat at the flames on", Subject, ", but " + Subject.it + Subject.GetVerb("dodge", PrependSpace: true, PronounAntecedent: true), "!", null, null, null, Actor, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Dialog);
						return true;
					}
				}
				if (Subject.CurrentCell != null && !Actor.PhaseMatches(Subject))
				{
					Messaging.XDidYToZ(Actor, "try", "to beat at the flames on", Subject, ", but " + Actor.its + " " + bodyPart.Name + " pass through " + Subject.them, "!", null, null, null, Actor, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Dialog);
					return true;
				}
			}
			if (Actor == Subject)
			{
				Messaging.XDidY(Actor, "beat", "at the flames with " + Actor.its + " " + bodyPart.Name, "!", null, null, Actor, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Dialog);
			}
			else
			{
				Messaging.XDidYToZ(Actor, "beat", "at the flames on", Subject, "with " + Actor.its + " " + bodyPart.Name, "!", null, null, Subject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Dialog);
			}
			if (!Actor.FireEvent(Event.New("PerformedFirefighting", "Subject", Subject)))
			{
				return true;
			}
			if (!Subject.FireEvent(Event.New("ReceivedFirefighting", "Subject", Subject)))
			{
				return true;
			}
			int num2 = GetFirefightingPerformanceEvent.GetFor(Actor, Subject, Patting: true);
			if (num2 != 0)
			{
				Subject.TemperatureChange(num2, null, Radiant: false, MinAmbient: true, MaxAmbient: false, IgnoreResistance: false, 5);
			}
			return true;
		}
		if (Actor != Subject)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You have no hands to beat at the flames with!");
			}
			return false;
		}
		if (Actor.IsFlying)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You have no hands to beat at the flames with, and cannot roll on the ground because you are flying!");
			}
			return false;
		}
		if (!Actor.PhaseMatches(1))
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You have no hands to beat at the flames with, and cannot roll on the ground because you are phased out!");
			}
			return false;
		}
		bool flag = false;
		if (Actor.IsPlayer())
		{
			if (Popup.ShowYesNo("You have no hands to beat at the flames with. Do you want to roll on the ground to try to put them out?") == DialogResult.Yes)
			{
				flag = true;
			}
		}
		else if (Actor.isDamaged(50, inclusive: true) && Actor.Stat("Intelligence") >= 7 && Actor.Stat("Willpower") >= 7)
		{
			flag = true;
		}
		if (flag)
		{
			Subject.ApplyEffect(new Prone(Voluntary: true));
			Messaging.XDidY(Actor, "roll", "on the ground", "!", null, null, Actor, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Dialog);
			Subject.TemperatureChange(-200, null, Radiant: false, MinAmbient: true, MaxAmbient: false, IgnoreResistance: false, 5);
			return true;
		}
		return false;
	}
}
