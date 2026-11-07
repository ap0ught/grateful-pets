using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FrostWebs : BaseMutation
{
	public FrostWebs()
	{
		Type = "Physical";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 12 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandFrostWebs");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("ice", 1);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyStuck");
		Registrar.Register("CommandFrostWebs");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return string.Concat(string.Concat("You fill a nearby area with frosty webs.\n\n" + "Range: 12\n", "Area: 3x3\n"), "Cooldown: 30 rounds\n");
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public int GetCooldown()
	{
		return 30;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", 12);
		stats.Set("Area", "3x3");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown());
	}

	public void FrostWeb(List<Cell> Cells)
	{
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		foreach (Cell Cell in Cells)
		{
			if (80.in100())
			{
				GameObject gameObject = GameObject.Create("FrostWeb");
				Cell.AddObject(gameObject);
				gameObject.DotPuff("&C");
			}
		}
		IComponent<GameObject>.XDidY(ParentObject, "shoot", "a swatch of frost webs", "!", null, null, ParentObject);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck")
		{
			return false;
		}
		if (E.ID == "CommandFrostWebs")
		{
			List<Cell> list = PickBurst(1, 12, Locked: false, AllowVis.OnlyVisible, "Knit Frosty Webs");
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(ParentObject) > 13)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("That is out of range! (12 squares)");
					}
					return false;
				}
			}
			if (list == null)
			{
				return false;
			}
			PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_spinnerets_webDrop", 0.5f, 0f, Combat: true);
			UseEnergy(1000, "Physical Mutation Frost Webs");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown());
			FrostWeb(list);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Knit Frosty Webs", "CommandFrostWebs", "Mental Mutations", null, "#");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
