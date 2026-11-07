using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedraWhiteOpal : CyberneticsCathedra
{
	public int BillowsTimer = -1;

	public override void OnImplanted(GameObject Object)
	{
		base.OnImplanted(Object);
		ActivatedAbilityID = Object.AddActivatedAbility("Glitter Bomb", "CommandActivateCathedra", "Cybernetics", null, "á", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, "CommandCathedraWhiteOpal");
	}

	public override void CollectStats(Templates.StatCollector stats)
	{
		base.CollectStats(stats);
		stats.Set("Duration", GetDuration());
	}

	public int GetDuration(GameObject Actor = null)
	{
		int level = GetLevel(Actor);
		return 1 + level / 2;
	}

	public override void Activate(GameObject Actor)
	{
		int level = GetLevel(Actor);
		BillowsTimer = GetDuration();
		IComponent<GameObject>.XDidY(Actor, "start", "streaming ribbons of {{glittering|glitter}}", "!");
		GlitterBomb(Actor, level, 800);
		base.Activate(Actor);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (BillowsTimer < 0)
		{
			return true;
		}
		GameObject user = User;
		if (user == null)
		{
			BillowsTimer = 0;
			return true;
		}
		GlitterBomb(user, GetLevel(user), 800);
		BillowsTimer--;
		return base.HandleEvent(E);
	}

	public static void GlitterBomb(GameObject Actor, int Level, int Density)
	{
		if (Actor.OnWorldMap())
		{
			return;
		}
		List<Cell> cells = new List<Cell>(8);
		Actor.CurrentCell.ForeachAdjacentCell((Cell C) =>
		{
			if (!C.IsOccluding())
			{
				cells.Add(C);
			}
		});
		if (cells.Count == 0)
		{
			cells.Add(Actor.CurrentCell);
		}
		Phase.carryOverPrep(Actor, out var FX, out var FX2);
		Event obj = Event.New("CreatorModifyGas", "Gas", (object)null);
		Actor?.PlayWorldSound("Sounds/Grenade/sfx_grenade_glitter_explode");
		foreach (Cell item in cells)
		{
			GameObject gameObject = GameObject.Create("GlitterGas");
			Gas part = gameObject.GetPart<Gas>();
			part.Creator = Actor;
			part.Density = Density / cells.Count;
			part.Level = Level;
			Phase.carryOver(Actor, gameObject, FX, FX2);
			obj.SetParameter("Gas", part);
			Actor.FireEvent(obj);
			item.AddObject(gameObject);
		}
	}
}
