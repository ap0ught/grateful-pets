using System;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class SummoningCurio : IPart
{
	public string Display;

	public string Creature;

	public string Template;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "activate", "ActivateSummoningCurio", null, 'a');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateSummoningCurio")
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (E.Actor.OnWorldMap())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You cannot do that on the world map.");
				}
				return false;
			}
			Cell cell = null;
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				cell = E.Actor.Physics.PickDestinationCell(8, AllowVis.OnlyVisible, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Fabricate " + Display);
			}
			if (cell == null)
			{
				return true;
			}
			PlayWorldSound("sfx_interact_curio_creaturesummon_activate");
			GameObject gameObject = GameObject.Create(Creature);
			HeroMaker.MakeHero(gameObject);
			gameObject.RemovePart<GivesRep>();
			gameObject.FireEvent("CurioInit");
			gameObject.SetAlliedLeader<AllyCurio>(E.Actor);
			gameObject.MakeActive();
			cell.AddObject(gameObject);
			cell.PlayWorldSound("sfx_creature_appear_summon");
			gameObject.PlayWorldSoundTag("AmbientIdleSound");
			ParentObject.Destroy();
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				Popup.Show("You activate the curio and toss it on the ground. It erupts into a throng of tiny polygons, which amalgamate into a fully formed " + gameObject.DisplayName + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Display == null && Creature != null)
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateSampleObject(Creature);
			Display = ColorUtility.StripFormatting(gameObject.Render.DisplayName);
			gameObject.Obliterate();
		}
		if (Display != null)
		{
			E.Postfix.AppendRules("Fabricates one " + Display + ", cognitively altered to like you.");
		}
		return base.HandleEvent(E);
	}
}
