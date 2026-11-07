using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
/// This part is not used in the base game.
/// </remarks>
[Serializable]
public class CyberneticsStasisArena : IPoweredPart
{
	public int BaseCooldown = 60;

	public int BaseExclusionSize = 5;

	public int BaseRealityStabilizationPenetration = 30;

	public string BaseDuration = "10";

	public string CommandID;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsStasisArena()
	{
		ChargeUse = 0;
		WorksOnImplantee = true;
		NameForStatus = "StasisArena";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (CommandID == null)
		{
			CommandID = Guid.NewGuid().ToString();
		}
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Stasis Arena", CommandID, "Cybernetics", null, "é");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == CommandID && !ActivateStasisArena(E.Actor, E.Target, E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		int computePower = GetAvailableComputePowerEvent.GetFor(this);
		int exclusionSize = GetExclusionSize(computePower);
		int cooldown = GetCooldown(computePower);
		int minDuration = GetMinDuration(computePower);
		int maxDuration = GetMaxDuration(computePower);
		E.Description = "Activated. Cooldown " + cooldown + ".\nPick an exclusion zone of up to " + exclusionSize.Things("square") + "; the rest of the zone, other than the square you are in, is enveloped in stasis fields that last " + ((minDuration == maxDuration) ? minDuration.Things("turn") : (minDuration + "-" + maxDuration + " turns")) + ".\nCompute power on the local lattice increases this implant's effectiveness.";
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool ActivateStasisArena(GameObject Actor, GameObject Target = null, IEvent FromEvent = null)
	{
		if (!GameObject.Validate(ref Actor))
		{
			return false;
		}
		if (!IsObjectActivePartSubject(Actor))
		{
			return false;
		}
		if (Actor.OnWorldMap())
		{
			return Actor.Fail("You cannot do that on the world map.");
		}
		Zone currentZone = Actor.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (!Actor.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			return false;
		}
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return Actor.Fail(ParentObject.Does("are") + " " + GetStatusPhrase() + ".");
		}
		int computePower = GetAvailableComputePowerEvent.GetFor(this);
		int realityStabilizationPenetration = GetRealityStabilizationPenetration(computePower);
		Event obj = Event.New("InitiateRealityDistortionLocal");
		obj.SetParameter("Object", Actor);
		obj.SetParameter("Device", ParentObject);
		obj.SetParameter("RealityStabilizationPenetration", realityStabilizationPenetration);
		if (!Actor.FireEvent(obj, FromEvent))
		{
			return false;
		}
		Cell cell = Actor.CurrentCell;
		int exclusionSize = GetExclusionSize(computePower);
		List<Cell> list = null;
		if (Actor.IsPlayer())
		{
			list = PickFieldAdjacent(exclusionSize, Actor, "Exclusion");
		}
		else if ((Target ?? (Target = Actor.Target)) != null)
		{
			list = new List<Cell>();
			Cell cell2 = Target.CurrentCell;
			int num = 0;
			Cell cell3 = cell;
			while (list.Count < exclusionSize && cell3 != null && cell3 != cell2 && ++num < 100)
			{
				string generalDirectionFromCell = cell3.GetGeneralDirectionFromCell(cell2);
				if (generalDirectionFromCell.IsNullOrEmpty())
				{
					continue;
				}
				Cell cell4 = cell3.GetCellFromDirection(generalDirectionFromCell);
				if (cell4 != null)
				{
					if (list.Contains(cell4))
					{
						cell4 = null;
					}
					else
					{
						list.Add(cell4);
					}
				}
				cell3 = cell4;
			}
		}
		else
		{
			List<GameObject> list2 = currentZone.FastCombatSquareVisibility(cell.X, cell.Y, 25, Actor, Actor.IsHostileTowards);
			if (list2.Count > 0)
			{
				int num2 = 0;
				List<GameObject> list3 = Event.NewGameObjectList();
				foreach (GameObject item in list2)
				{
					if (item.InAdjacentCellTo(Actor))
					{
						list3.Add(item);
					}
				}
				if (list3.Count > 0)
				{
					list = new List<Cell>(list2.Count);
					GameObject randomElement = list3.GetRandomElement();
					list2.Remove(randomElement);
					list.Add(randomElement.CurrentCell);
					List<Cell> list4 = new List<Cell>();
					while (list2.Count > 1 && list.Count < exclusionSize && ++num2 < 100)
					{
						Cell cell5 = list[list.Count - 1];
						Cell cell6 = null;
						list4.Clear();
						foreach (Cell localCardinalAdjacentCell in cell5.GetLocalCardinalAdjacentCells())
						{
							if (!list.Contains(localCardinalAdjacentCell) && !localCardinalAdjacentCell.Objects.Contains(Actor))
							{
								list4.Add(localCardinalAdjacentCell);
							}
						}
						list4.ShuffleInPlace();
						foreach (Cell item2 in list4)
						{
							foreach (GameObject item3 in list2)
							{
								if (item2.Objects.Contains(item3))
								{
									cell6 = item2;
									break;
								}
							}
							if (cell6 != null)
							{
								break;
							}
						}
						if (cell6 == null)
						{
							int num3 = 0;
							foreach (Cell item4 in list4)
							{
								int num4 = 0;
								foreach (GameObject item5 in list2)
								{
									num4 += 100 - item5.DistanceTo(item4);
								}
								if (num4 > num3)
								{
									cell6 = item4;
									num3 = num4;
								}
							}
						}
						if (cell6 == null)
						{
							break;
						}
						list.Add(cell6);
						foreach (GameObject @object in cell6.Objects)
						{
							list2.Remove(@object);
						}
					}
				}
			}
		}
		if (list == null)
		{
			return false;
		}
		if (!list.Contains(cell))
		{
			list.Add(cell);
		}
		Actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown(computePower));
		Actor.UseEnergy(1000, "Cybernetics StasisArena");
		DeployToCells(currentZone, list, Actor, computePower, realityStabilizationPenetration);
		FromEvent?.RequestInterfaceExit();
		return true;
	}

	private GameObject DeployToCells(Zone Zone, List<Cell> ExclusionCells, GameObject Actor, int ComputePower, int RealityStabilizationPenetration)
	{
		GameObject result = null;
		Phase.carryOverPrep(Actor, out var FX, out var FX2);
		_ = Look._TextConsole;
		The.Core.RenderBase();
		for (int i = 0; i < Zone.Height; i++)
		{
			for (int j = 0; j < Zone.Width; j++)
			{
				Cell cell = Zone.GetCell(j, i);
				if (!ExclusionCells.Contains(cell) && IComponent<GameObject>.CheckRealityDistortionAccessibility(null, cell, Actor, ParentObject, null, RealityStabilizationPenetration))
				{
					GameObject gameObject = GameObject.Create("Stasisfield");
					gameObject.GetPart<Forcefield>().Creator = Actor;
					gameObject.AddPart(new Temporary(GetDuration(ComputePower) + 1));
					Phase.carryOver(Actor, gameObject, FX, FX2);
					cell.AddObject(gameObject);
					result = gameObject;
					if (cell.IsVisible())
					{
						The.Core.RenderBase();
					}
				}
			}
		}
		return result;
	}

	public int GetExclusionSize(int ComputePower)
	{
		return BaseExclusionSize + ComputePower / 5;
	}

	public int GetExclusionSize()
	{
		return GetExclusionSize(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetCooldown(int ComputePower)
	{
		int num = BaseCooldown;
		if (ComputePower != 0)
		{
			num = Math.Max(num * (100 - ComputePower / 2) / 100, num / 2);
		}
		return num;
	}

	public int GetCooldown()
	{
		return GetCooldown(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetDuration(int ComputePower)
	{
		int num = BaseDuration.RollCached() + ComputePower / 3;
		return num + Stat.Random(-num / 5, num / 5);
	}

	public int GetDuration()
	{
		return GetDuration(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetMinDuration(int ComputePower)
	{
		int num = BaseDuration.RollMinCached() + ComputePower / 3;
		return num - num / 5;
	}

	public int GetMinDuration()
	{
		return GetMinDuration(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetMaxDuration(int ComputePower)
	{
		int num = BaseDuration.RollMaxCached() + ComputePower / 3;
		return num + num / 5;
	}

	public int GetMaxDuration()
	{
		return GetMaxDuration(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetRealityStabilizationPenetration(int ComputePower)
	{
		return BaseRealityStabilizationPenetration + ComputePower / 3;
	}

	public int GetRealityStabilizationPenetration()
	{
		return GetRealityStabilizationPenetration(GetAvailableComputePowerEvent.GetFor(this));
	}
}
