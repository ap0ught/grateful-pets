using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_LayMine : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public Guid TimedActivatedAbilityID = Guid.Empty;

	public static void Init()
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandLayMine");
		Registrar.Register("CommandLayMineTimed");
		base.Register(Object, Registrar);
	}

	private bool Explosives(GameObject obj)
	{
		if (obj == ParentObject)
		{
			return false;
		}
		if (obj.HasPart<Tinkering_Layable>())
		{
			return true;
		}
		if (obj.HasPart<Tinkering_Mine>())
		{
			return true;
		}
		return false;
	}

	public static GameObject CreateBomb(GameObject obj = null, GameObject Actor = null, int Countdown = -1)
	{
		if (obj == null)
		{
			obj = GameObject.Create(PopulationManager.RollOneFrom("DynamicObjectsTable:Grenades").Blueprint);
		}
		Tinkering_Mine part = obj.GetPart<Tinkering_Mine>();
		GameObject gameObject;
		if (part == null)
		{
			gameObject = GameObject.Create("MineShell");
			part = gameObject.GetPart<Tinkering_Mine>();
			obj = obj.RemoveOne();
			obj.RemoveFromContext();
			part.SetExplosive(obj);
		}
		else
		{
			gameObject = obj;
			obj = part.Explosive;
		}
		Tinkering_Layable part2 = obj.GetPart<Tinkering_Layable>();
		if (part2 == null)
		{
			throw new Exception("no Tinkering_Layable part in " + obj.DebugName);
		}
		Render render = gameObject.Render;
		Render render2 = obj.Render;
		TinkerItem part3 = gameObject.GetPart<TinkerItem>();
		TinkerItem part4 = obj.GetPart<TinkerItem>();
		string text = ((Countdown > 0) ? "bomb" : "mine");
		part.Message = part2.DetonationMessage;
		part.Timer = Countdown;
		part.Arm(Actor);
		if (render != null && render2 != null)
		{
			string displayName = render2.DisplayName;
			displayName = ((!displayName.Contains("grenade")) ? (displayName + " " + text) : displayName.Replace("grenade", text));
			render.DisplayName = displayName;
		}
		Examiner part5 = obj.GetPart<Examiner>();
		if (part5 != null)
		{
			gameObject.RequirePart<Examiner>().CopyFrom(part5);
		}
		if (part3 != null && part4 != null)
		{
			part3.SubstituteBlueprint = (part4.SubstituteBlueprint.IsNullOrEmpty() ? obj.Blueprint : part4.SubstituteBlueprint);
			part3.CanBuild = part4.CanBuild;
			part3.CanDisassemble = part4.CanDisassemble;
		}
		return gameObject;
	}

	public static GameObject CreateBomb(string Blueprint, GameObject Actor = null, int Countdown = -1)
	{
		return CreateBomb(GameObject.CreateUnmodified(Blueprint), Actor, Countdown);
	}

	public static GameObject CreateMine(GameObject obj = null, GameObject Actor = null)
	{
		return CreateBomb(obj, Actor);
	}

	public static GameObject CreateMine(string Blueprint, GameObject Actor = null)
	{
		return CreateMine(GameObject.CreateUnmodified(Blueprint), Actor);
	}

	private bool AttemptLayMine(bool Timed = false)
	{
		if (ParentObject.OnWorldMap())
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You cannot do that on the world map.");
			}
			return false;
		}
		if (!ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		Inventory inventory = ParentObject.Inventory;
		string title = (Timed ? "Select an explosive to set as a bomb:\n\n" : "Select an explosive to lay as a mine:\n\n");
		List<GameObject> objects = inventory.GetObjects(Explosives);
		if (objects.Count == 0)
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You have no explosives to deploy!");
			}
			return false;
		}
		GameObject gameObject = (ParentObject.IsPlayer() ? Popup.PickGameObject(title, objects, AllowEscape: true) : objects.GetRandomElement());
		if (gameObject == null)
		{
			return false;
		}
		Cell cell;
		if (ParentObject.IsPlayer())
		{
			string text = XRL.UI.PickDirection.ShowPicker("Lay Mine");
			if (text == null)
			{
				return false;
			}
			cell = ParentObject.CurrentCell.GetCellFromDirection(text);
		}
		else
		{
			cell = ParentObject.CurrentCell.GetEmptyAdjacentCells().GetRandomElement();
		}
		if (cell == null || !cell.IsEmpty())
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You can't deploy there!");
			}
			return false;
		}
		int countdown = -1;
		if (Timed)
		{
			if (ParentObject.IsPlayer())
			{
				int? num = Popup.AskNumber("For how many rounds would you like to set the timer? (max=10)", "Sounds/UI/ui_notification", "", 1, 0, 10);
				if (!num.HasValue || num == 0)
				{
					return false;
				}
				try
				{
					countdown = Convert.ToInt32(num);
					if (countdown > 10)
					{
						countdown = 10;
					}
					else if (countdown < 0)
					{
						return false;
					}
					countdown++;
				}
				catch
				{
					return false;
				}
			}
			else
			{
				countdown = Stat.Random(6, 11);
			}
		}
		GameObject gameObject2 = ((!Timed) ? CreateMine(gameObject, ParentObject) : CreateBomb(gameObject, ParentObject, countdown));
		if (Timed)
		{
			cell.PlayWorldSound("Sounds/Interact/sfx_interact_mine_setBomb");
		}
		else
		{
			cell.PlayWorldSound("Sounds/Interact/sfx_interact_mine_lay");
		}
		if (gameObject2.Understood())
		{
			DidXToY(Timed ? "set" : "lay", gameObject2, null, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
		}
		else
		{
			DidX("drop", "something");
		}
		cell.AddObject(gameObject2);
		ParentObject.UseEnergy(1000, "Skill Tinkering " + (Timed ? "Bomb" : "Mine") + " Deploy");
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandLayMine")
		{
			if (AttemptLayMine())
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.ID == "CommandLayMineTimed" && AttemptLayMine(Timed: true))
		{
			E.RequestInterfaceExit();
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Lay Mine", "CommandLayMine", "Tinkering", null, "é");
		TimedActivatedAbilityID = AddMyActivatedAbility("Set Bomb", "CommandLayMineTimed", "Tinkering", null, "ë");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		RemoveMyActivatedAbility(ref TimedActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
