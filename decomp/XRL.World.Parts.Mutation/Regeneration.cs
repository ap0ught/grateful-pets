using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Regeneration : BaseMutation
{
	public int nRegrowCount;

	public int nNextLimb = 5000;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Regenerating");
		Registrar.Register("BeginTakeAction");
		Registrar.Register("BeforeDecapitate");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "Your wounds heal very quickly.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text += "Your full natural healing rate applies in combat.\n";
		text = text + "+{{rules|" + (int)(100f * GetRegenerationBonus(Level)) + "%}} faster natural healing rate\n";
		text = text + "{{rules|" + GetRegenerationChance(Level) + "%}} chance to regrow a missing limb each round\n";
		if (GetRegenerationChance(Level) >= 100)
		{
			text += "{{rules|You cannot be decapitated.}}\n";
		}
		if (Level < 5)
		{
			return text + "{{rules|" + GetDebuffChance(Level) + "}}% chance to remove a {{rules|minor physical debuff}} at random each round";
		}
		return text + "{{rules|" + GetDebuffChance(Level) + "}}% chance to remove a {{rules|physical debuff}} at random each round";
	}

	public int GetDebuffChance(int Level)
	{
		return 1 + Level / 3;
	}

	public int GetRegenerationChance(int Level)
	{
		return Level * 10;
	}

	public float GetRegenerationBonus(int Level)
	{
		return 0.1f + 0.1f * (float)Level;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDecapitate")
		{
			if (GetRegenerationChance(Level) >= 100)
			{
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("{{G|You were decapitated, but a new head regrew immediately!}}");
				}
				return false;
			}
		}
		else if (E.ID == "Regenerating")
		{
			int intParameter = E.GetIntParameter("Amount");
			intParameter += (int)Math.Ceiling((float)intParameter * GetRegenerationBonus(Level));
			E.SetParameter("Amount", intParameter);
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (GetDebuffChance(Level).in100())
			{
				ParentObject.FireEvent(Event.New("Regenera", "Level", Level, "Involuntary", 1));
			}
			if (GetRegenerationChance(Level).in100())
			{
				RegenerateLimbEvent.Send(ParentObject, null, null, Whole: false, All: false, IncludeMinor: true, Voluntary: false);
			}
		}
		return base.FireEvent(E);
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
