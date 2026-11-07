using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Survival_Camp : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandSurvivalCamp";

	public Guid ActivatedAbilityID = Guid.Empty;

	public Guid StopActivatedAbilityID = Guid.Empty;

	public List<string> CampedZones = new List<string>();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			AttemptCamp(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool AttemptCamp(GameObject Actor)
	{
		if (Actor.AreHostilesNearby())
		{
			return Actor.Fail("You can't cook with hostiles nearby.");
		}
		if (Actor.OnWorldMap())
		{
			return Actor.Fail("You can't cook on the world map.");
		}
		if (!Actor.CanChangeMovementMode("Camping", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		PointOfInterest one = GetPointsOfInterestEvent.GetOne(Actor, "PlayerCampfire");
		if (one != null && one.GetDistanceTo(Actor) <= 24)
		{
			GameObject gameObject = one.Object;
			if (gameObject != null)
			{
				if (Actor.IsPlayer())
				{
					if (one.IsAt(Actor))
					{
						Actor.Fail("There " + gameObject.Is + " already " + gameObject.an() + " here.");
					}
					else if (Popup.ShowYesNoCancel("There " + gameObject.Is + " already " + gameObject.an() + " " + Actor.DescribeDirectionToward(gameObject) + ". Do you want to go to " + gameObject.them + "?") == DialogResult.Yes)
					{
						one.NavigateTo(Actor);
					}
				}
				return false;
			}
		}
		string text = PickDirectionS("Make Camp");
		if (text == null)
		{
			return false;
		}
		Cell cellFromDirection = Actor.CurrentCell.GetCellFromDirection(text);
		if (cellFromDirection == null || cellFromDirection.ParentZone != Actor.CurrentZone)
		{
			return Actor.Fail("You can only build a campfire in the same zone you are in.");
		}
		if (cellFromDirection.HasObjectWithTag("ExcavatoryTerrainFeature"))
		{
			return Actor.Fail("There is nothing there you can build a campfire on.");
		}
		if (!cellFromDirection.IsEmpty())
		{
			return Actor.Fail("Something is in the way!");
		}
		GameObject gameObject2 = Campfire.FindExtinguishingPool(cellFromDirection);
		if (gameObject2 != null)
		{
			return Actor.Fail("You cannot start a campfire in " + gameObject2.t() + ".");
		}
		cellFromDirection.PlayWorldSound("Sounds/Abilities/sfx_ability_makeCamp");
		IComponent<GameObject>.XDidY(Actor, "make", "camp");
		GameObject gameObject3 = ((!cellFromDirection.ParentZone.ZoneID.StartsWith("ThinWorld")) ? cellFromDirection.AddObject("Campfire") : cellFromDirection.AddObject("BlueCampfire"));
		if (Actor.IsPlayer())
		{
			gameObject3.SetIntProperty("PlayerCampfire", 1);
			gameObject3.SetStringProperty("PointOfInterestKey", "PlayerCampfire");
		}
		if (!CampedZones.Contains(Actor.CurrentZone.ZoneID))
		{
			CampedZones.Add(Actor.CurrentZone.ZoneID);
		}
		return true;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Make Camp", COMMAND_NAME, "Maneuvers", "Start a campfire for cooking meals and preserving foods. You can't make camp in combat.", "\u0006");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.AddSkill(GO);
	}
}
