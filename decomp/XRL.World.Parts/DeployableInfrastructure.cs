using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class DeployableInfrastructure : IPart
{
	public string DeployNoun = "tech";

	public string DeployVerb = "deploy";

	public string DeploySound;

	public string ModPart;

	public string ObjectBlueprint;

	public string SkillRequired;

	public string NoModIfPart;

	public string SuppressIfPart;

	public string MessageByCountSuffix;

	public int Cells = 1;

	public int MaxCells;

	public int EnergyCost = 1000;

	public bool AllowExistenceSupport;

	public bool MakeUnderstood = true;

	public bool RepairDuplicates = true;

	public bool SuppressDuplicates = true;

	public bool UsesStack;

	public bool RequireVisibility = true;

	public override bool SameAs(IPart p)
	{
		DeployableInfrastructure deployableInfrastructure = p as DeployableInfrastructure;
		if (deployableInfrastructure.DeployNoun != DeployNoun)
		{
			return false;
		}
		if (deployableInfrastructure.DeployVerb != DeployVerb)
		{
			return false;
		}
		if (deployableInfrastructure.ModPart != ModPart)
		{
			return false;
		}
		if (deployableInfrastructure.ObjectBlueprint != ObjectBlueprint)
		{
			return false;
		}
		if (deployableInfrastructure.SkillRequired != SkillRequired)
		{
			return false;
		}
		if (deployableInfrastructure.NoModIfPart != NoModIfPart)
		{
			return false;
		}
		if (deployableInfrastructure.SuppressIfPart != SuppressIfPart)
		{
			return false;
		}
		if (deployableInfrastructure.Cells != Cells)
		{
			return false;
		}
		if (deployableInfrastructure.MaxCells != MaxCells)
		{
			return false;
		}
		if (deployableInfrastructure.EnergyCost != EnergyCost)
		{
			return false;
		}
		if (deployableInfrastructure.AllowExistenceSupport != AllowExistenceSupport)
		{
			return false;
		}
		if (deployableInfrastructure.MakeUnderstood != MakeUnderstood)
		{
			return false;
		}
		if (deployableInfrastructure.RepairDuplicates != RepairDuplicates)
		{
			return false;
		}
		if (deployableInfrastructure.SuppressDuplicates != SuppressDuplicates)
		{
			return false;
		}
		if (deployableInfrastructure.UsesStack != UsesStack)
		{
			return false;
		}
		if (deployableInfrastructure.RequireVisibility != RequireVisibility)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (SkillRequired.IsNullOrEmpty() || The.Player.HasSkill(SkillRequired))
		{
			E.AddAction("Deploy", DeployNoun.IsNullOrEmpty() ? "deploy" : ("deploy " + DeployNoun), "DeployInfrastructure", null, 'y');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DeployInfrastructure" && AttemptDeploy(E.Actor) && EnergyCost > 0)
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public bool AttemptDeploy(GameObject Actor)
	{
		if (Actor == null)
		{
			return false;
		}
		if (!SkillRequired.IsNullOrEmpty() && !Actor.HasSkill(SkillRequired))
		{
			return false;
		}
		if (Actor.OnWorldMap())
		{
			return Actor.Fail("You cannot do that on the world map.");
		}
		if (ParentObject.IsRusted())
		{
			return Actor.Fail(ParentObject.Itis + " rusted...");
		}
		if (ParentObject.IsBroken())
		{
			return Actor.Fail(ParentObject.Itis + " broken...");
		}
		int num = (UsesStack ? ParentObject.Count : Cells);
		if (MaxCells > 0 && num > MaxCells)
		{
			num = MaxCells;
		}
		if (num <= 0)
		{
			return false;
		}
		List<Cell> list;
		if (num != 1 || UsesStack)
		{
			list = ((!DeployNoun.IsNullOrEmpty()) ? PickFieldAdjacent(num, Actor, ColorUtility.CapitalizeExceptFormatting(DeployNoun), ReturnNullForAbort: false, RequireVisibility) : PickFieldAdjacent(num, Actor, null, ReturnNullForAbort: false, RequireVisibility));
		}
		else
		{
			list = new List<Cell>();
			Cell cell;
			if (DeployNoun.IsNullOrEmpty())
			{
				cell = PickDirection(ForAttack: false, "Deploy", Actor);
			}
			else
			{
				cell = PickDirection(ForAttack: false, "Deploy " + ColorUtility.CapitalizeExceptFormatting(DeployNoun), Actor);
			}
			if (cell == null)
			{
				return false;
			}
			list.Add(cell);
		}
		if (list == null || list.Count == 0)
		{
			return false;
		}
		int num2 = 0;
		foreach (Cell item in list)
		{
			if (DeployOne(Actor, item))
			{
				num2++;
			}
		}
		if (num2 == 0)
		{
			Popup.ShowFail("There is no useful way to " + DeployVerb + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " there.");
			return false;
		}
		Actor.PlayWorldOrUISound(DeploySound);
		if (!MessageByCountSuffix.IsNullOrEmpty())
		{
			Popup.Show("You " + DeployVerb + " " + num2 + MessageByCountSuffix);
		}
		else
		{
			Popup.Show("You " + DeployVerb + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
		if (EnergyCost > 0)
		{
			Actor.UseEnergy(EnergyCost, "Tinkering");
		}
		if (UsesStack)
		{
			for (int i = 0; i < num2; i++)
			{
				ParentObject.Destroy();
			}
		}
		else
		{
			ParentObject.Destroy();
		}
		The.ZoneManager.PaintWalls();
		return true;
	}

	public bool DeployOne(GameObject Actor, Cell Cell, bool Message = false, bool Sound = false)
	{
		bool flag = false;
		if (!SuppressIfPart.IsNullOrEmpty() && Cell.HasObjectWithPart(SuppressIfPart))
		{
			return flag;
		}
		if (RepairDuplicates || SuppressDuplicates)
		{
			if (!ObjectBlueprint.IsNullOrEmpty())
			{
				GameObject firstObject = Cell.GetFirstObject(ObjectBlueprint);
				if (firstObject != null)
				{
					if (RepairDuplicates)
					{
						if (firstObject.IsBroken())
						{
							firstObject.RemoveEffect<Broken>();
							flag = true;
						}
						if (firstObject.isDamaged())
						{
							firstObject.RestorePristineHealth();
							flag = true;
						}
					}
					if (SuppressDuplicates)
					{
						goto IL_01a2;
					}
				}
			}
			if (!ModPart.IsNullOrEmpty())
			{
				GameObject firstObjectWithPart = Cell.GetFirstObjectWithPart(ModPart);
				if (firstObjectWithPart != null)
				{
					if (RepairDuplicates)
					{
						if (firstObjectWithPart.IsBroken())
						{
							firstObjectWithPart.RemoveEffect<Broken>();
							flag = true;
						}
						if (firstObjectWithPart.isDamaged())
						{
							firstObjectWithPart.RestorePristineHealth();
							flag = true;
						}
					}
					if (SuppressDuplicates)
					{
						goto IL_01a2;
					}
				}
			}
		}
		GameObject gameObject = null;
		if (!ModPart.IsNullOrEmpty())
		{
			gameObject = Cell.GetFirstObjectWithPropertyOrTag("Wall", ValidInstall) ?? Cell.GetFirstObjectWithPart("Physics", ValidInstall);
		}
		if (gameObject != null)
		{
			ItemModding.ApplyModification(gameObject, ModPart);
			ZoneManager.PaintWalls(Cell.ParentZone, Cell.X - 1, Cell.Y - 1, Cell.X + 1, Cell.Y + 1);
			flag = true;
		}
		else if (!ObjectBlueprint.IsNullOrEmpty())
		{
			GameObject gameObject2 = Cell.AddObject(GameObject.CreateUnmodified(ObjectBlueprint));
			if (gameObject2 != null)
			{
				if (MakeUnderstood)
				{
					gameObject2.MakeUnderstood();
				}
				flag = true;
			}
		}
		goto IL_01a2;
		IL_01a2:
		if (Sound)
		{
			Cell.PlayWorldSound(DeploySound);
		}
		if (Message & flag)
		{
			Actor.EmitMessage(Actor.t() + " " + Actor.GetVerb(DeployVerb, PrependSpace: false) + " " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".", null, null, Actor.IsPlayer());
		}
		return flag;
	}

	private bool ValidInstall(GameObject obj)
	{
		if (obj.IsTakeable())
		{
			return false;
		}
		if (!obj.ConsiderSolid() && !obj.IsDoor())
		{
			return false;
		}
		if (!obj.IsReal)
		{
			return false;
		}
		if (!AllowExistenceSupport && obj.HasPart<ExistenceSupport>())
		{
			return false;
		}
		if (obj.IsCombatObject())
		{
			return false;
		}
		if (!ModPart.IsNullOrEmpty() && obj.HasPart(ModPart))
		{
			return false;
		}
		if (!NoModIfPart.IsNullOrEmpty() && obj.HasPart(NoModIfPart))
		{
			return false;
		}
		if (!SuppressIfPart.IsNullOrEmpty() && obj.HasPart(SuppressIfPart))
		{
			return false;
		}
		return true;
	}
}
