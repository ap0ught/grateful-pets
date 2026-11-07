using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Clairvoyance : BaseMutation
{
	public bool RealityDistortionBased = true;

	public string Sound = "Sounds/Abilities/sfx_ability_mutation_mental_generic_activate";

	[NonSerialized]
	public List<VisCell> Cells = new List<VisCell>(64);

	public Clairvoyance()
	{
		Type = "Mental";
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(Cells.Count);
		foreach (VisCell cell in Cells)
		{
			Writer.Write(cell.C.ParentZone.ZoneID);
			Writer.Write(cell.C.X);
			Writer.Write(cell.C.Y);
			Writer.Write(cell.Turns);
		}
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		Cells.Clear();
		for (int i = 0; i < num; i++)
		{
			string zoneID = Reader.ReadString();
			int x = Reader.ReadInt32();
			int y = Reader.ReadInt32();
			int turns = Reader.ReadInt32();
			Cell cell = XRLCore.Core.Game.ZoneManager.GetZone(zoneID).GetCell(x, y);
			Cells.Add(new VisCell(cell, turns));
		}
		base.Read(Basis, Reader);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("stars", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		foreach (VisCell cell in Cells)
		{
			cell.C.ParentZone.AddLight(cell.C.X, cell.C.Y, 0, LightLevel.Omniscient);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		List<VisCell> list = null;
		foreach (VisCell cell in Cells)
		{
			cell.Turns--;
			if (cell.Turns <= 0)
			{
				if (list == null)
				{
					list = new List<VisCell>();
				}
				list.Add(cell);
			}
		}
		if (list != null)
		{
			foreach (VisCell item in list)
			{
				Cells.Remove(item);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandClairvoyance");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandClairvoyance")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return true;
			}
			int radius = 3 + Level;
			List<Cell> list;
			if (Level < 15)
			{
				list = PickCircle(radius, 80, Locked: false, AllowVis.Any, "Clairvoyance");
				if (list == null)
				{
					return true;
				}
				if (list.Count == 0)
				{
					return true;
				}
				if (RealityDistortionBased)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", list[0]);
					if (!ParentObject.FireEvent(e, E) || !list[0].FireEvent(e, E))
					{
						return true;
					}
				}
			}
			else
			{
				list = ParentObject.CurrentCell.ParentZone.GetCells();
				if (RealityDistortionBased && !ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this)))
				{
					return true;
				}
			}
			IComponent<GameObject>.PlayUISound(Sound);
			if (ParentObject.IsPlayer())
			{
				DidX("peer", "into another space", null, null, null, ParentObject);
			}
			UseEnergy(1000, "Mental Mutation Clairvoyance");
			CooldownMyActivatedAbility(ActivatedAbilityID, 100);
			Predicate<Cell> predicate = ((!RealityDistortionBased) ? ((Predicate<Cell>)((Cell C) => C != null && !C.ParentZone.IsWorldMap())) : ((Predicate<Cell>)((Cell C) => C != null && !C.OnWorldMap() && IComponent<GameObject>.CheckRealityDistortionAccessibility(null, C, ParentObject, null, this))));
			foreach (Cell item in list)
			{
				if (predicate(item))
				{
					Cells.Add(new VisCell(item, 19 + Level));
					item.SetExplored();
				}
			}
		}
		return base.FireEvent(E);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		if (Level < 15)
		{
			stats.Set("VisionRadius", 3 + Level, !flag);
		}
		else
		{
			stats.Set("VisionRadius", "whole map", !flag);
		}
		stats.Set("VisionDuration", $"{Level + 19}", !flag);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 100);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		bool result = base.ChangeLevel(NewLevel);
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return result;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Clairvoyance", "CommandClairvoyance", "Mental Mutations", null, "+", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
