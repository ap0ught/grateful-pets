using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Dystechnia : BaseMutation
{
	public Dystechnia()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != GetTinkeringBonusEvent.ID)
		{
			return ID == RepairCriticalFailureEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Inspect")
		{
			E.Bonus -= 20;
		}
		else if (E.Type == "Examine" || E.Type == "Repair" || E.Type == "ReverseEngineerTurns" || E.Type == "ItemModding" || E.Type == "Disarming")
		{
			E.Bonus--;
		}
		else if (E.Type == "Hacking")
		{
			E.Bonus -= 2;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (E.Pass == ExamineCriticalFailureEvent.PASSES)
		{
			CauseExplosion(E.Item, E.Actor, E);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RepairCriticalFailureEvent E)
	{
		CauseExplosion(E.Item, E.Actor, E);
		return false;
	}

	public static void CauseExplosion(GameObject Object, GameObject Actor = null, IEvent Event = null)
	{
		int complexity = Object.GetComplexity();
		Object.PotentiallyAngerOwner(Actor, "DontWarnOnExamine");
		IComponent<GameObject>.XDidY(Object, "explode", null, "!", null, null, null, Actor, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true, Actor.IsPlayer());
		Object.Explode((int)((float)complexity * 3000f * Stat.Random(0.8f, 1.2f)), Actor, null, 1f, complexity >= 8);
		Event?.RequestInterfaceExit();
	}

	public override string GetDescription()
	{
		string text = "";
		text += "You are befuddled by technological complexity.\n\n";
		text += "You're much worse at examining artifacts.\n";
		text += "You can't have artifacts identified for you because you don't understand their explanations.\n";
		text += "When you fail severely during artifact examination, the artifact explodes.\n";
		if (Options.AnySifrah)
		{
			text += "You lose a turn in most tinkering Sifrah games, and two turns in hacking Sifrah games.";
		}
		return text;
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
