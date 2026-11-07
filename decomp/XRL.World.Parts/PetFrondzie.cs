using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PetFrondzie : IPart
{
	public int cooldown = 20;

	public static string[] tauntStyles = new string[12]
	{
		"terrible", "ghastly", "corny", "shameful", "rude", "painful", "monstrous", "horrid", "puerile", "childish",
		"boring", "ridiculous"
	};

	public static string[] tauntTypes = new string[6] { "pun", "joke", "double entendre", "jape", "goof", "tall tale" };

	public static string[] tauntTargets = new string[13]
	{
		"the stilt bazaar", "the Canticles Chromaic", "Omonporch", "the Argent Fathers", "Bey Lah", "Kyakukya", "the Spindle", "the Rainbow Wood", "mutations", "combat",
		"pets", "the Dark Calculus", "tacos suprema"
	};

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public void taunt(GameObject target)
	{
		string randomElement = tauntStyles.GetRandomElement();
		string randomElement2 = tauntTypes.GetRandomElement();
		int num = Stat.Roll(1, 100);
		string text;
		if (num <= 50)
		{
			text = EncountersAPI.GetAnObject().an();
		}
		else
		{
			text = ((num > 90) ? tauntTargets.GetRandomElement() : Factions.GetRandomFaction().DisplayName);
		}
		IComponent<GameObject>.XDidY(ParentObject, "yell", Grammar.A(randomElement) + " " + randomElement2 + " about " + text, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true);
		if (!target.MakeSave("Willpower", 12, ParentObject, "Ego", "Verbal Taunt"))
		{
			target.Brain.Goals.Clear();
			target.Target = ParentObject;
			IComponent<GameObject>.XDidY(target, "are", "enraged by the mockery", "!", null, null, null, target, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (cooldown > 0)
			{
				cooldown--;
			}
			if (cooldown <= 0)
			{
				GameObject partyLeader = ParentObject.PartyLeader;
				if (partyLeader != null && partyLeader.IsInCombat() && ParentObject.CurrentCell != null)
				{
					List<GameObject> objects = ParentObject.CurrentZone.GetObjects((GameObject o) => o.Target == ParentObject.PartyLeader);
					if (objects.Count > 0)
					{
						cooldown = 20;
						taunt(objects.GetRandomElement());
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
