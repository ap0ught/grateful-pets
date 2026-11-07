using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedraRuby : CyberneticsCathedra
{
	public static readonly int BASE_PYRO_FIELD_DURATION = 3;

	public int Duration = -1;

	[NonSerialized]
	public Dictionary<Point2D, GameObject> Field = new Dictionary<Point2D, GameObject>();

	[NonSerialized]
	public List<Point2D> RemovePoints = new List<Point2D>();

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void OnImplanted(GameObject Object)
	{
		base.OnImplanted(Object);
		Object.RegisterPartEvent(this, "BeginMove");
		Object.RegisterPartEvent(this, "MoveFailed");
		ActivatedAbilityID = Object.AddActivatedAbility("Pyrokinesis Field", "CommandActivateCathedra", "Cybernetics", null, "*", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, "CommandCathedraRuby");
	}

	public override void OnUnimplanted(GameObject Object)
	{
		base.OnUnimplanted(Object);
		Object.UnregisterPartEvent(this, "BeginMove");
		Object.UnregisterPartEvent(this, "MoveFailed");
	}

	public override void CollectStats(Templates.StatCollector stats)
	{
		base.CollectStats(stats);
		Pyrokinesis.CollectProxyStats(stats, GetLevel());
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			SuspendPyroField();
		}
		else if (E.ID == "MoveFailed")
		{
			DesuspendPyroField();
		}
		return base.FireEvent(E);
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		Writer.Write(Field.Count);
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			Writer.Write(item.Key.x);
			Writer.Write(item.Key.y);
			Writer.WriteGameObject(item.Value);
		}
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		Field.Clear();
		int i = 0;
		for (int num = Reader.ReadInt32(); i < num; i++)
		{
			Point2D key = new Point2D(Reader.ReadInt32(), Reader.ReadInt32());
			Field[key] = Reader.ReadGameObject();
		}
	}

	public void Validate()
	{
		if (Field.Count == 0)
		{
			return;
		}
		RemovePoints.Clear();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			if (item.Value == null || item.Value.IsInvalid())
			{
				RemovePoints.Add(item.Key);
			}
		}
		foreach (Point2D removePoint in RemovePoints)
		{
			Field.Remove(removePoint);
		}
	}

	public override void Activate(GameObject Actor)
	{
		Cell cell = Actor.CurrentCell;
		if (cell?.ParentZone == null || cell.ParentZone.IsWorldMap())
		{
			return;
		}
		List<Cell> cells = cell.YieldAdjacentCells(2, LocalOnly: false, BuiltOnly: false).ToList();
		Duration = BASE_PYRO_FIELD_DURATION;
		foreach (GameObject item in Pyrokinesis.Pyro(Actor, GetLevel(Actor), cells, 9999, 0, Dependent: true))
		{
			Point2D key = item.CurrentCell.PathDifferenceTo(cell);
			Field[key] = item;
		}
		base.Activate(Actor);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<BeforeTemperatureChangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Field.Count > 0 && --Duration < 0)
		{
			DestroyPyroField();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeTemperatureChangeEvent E)
	{
		if (Duration >= 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		DesuspendPyroField();
		return base.HandleEvent(E);
	}

	public void DestroyPyroField()
	{
		Validate();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			item.Value.Obliterate();
		}
		Field.Clear();
	}

	public void SuspendPyroField()
	{
		Validate();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			item.Value.RemoveFromContext();
		}
	}

	public void DesuspendPyroField(GameObject Actor = null, bool Validated = false)
	{
		if (!Validated)
		{
			Validate();
		}
		Cell cell = (Actor ?? User)?.CurrentCell;
		Zone zone = cell?.ParentZone;
		if (zone == null || zone.IsWorldMap())
		{
			DestroyPyroField();
			return;
		}
		RemovePoints.Clear();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			Point2D key = item.Key;
			GameObject value = item.Value;
			if (value.CurrentCell != null)
			{
				continue;
			}
			Cell cellGlobal = zone.GetCellGlobal(cell.X + key.x, cell.Y + key.y);
			if (cellGlobal == null)
			{
				value.Obliterate();
				RemovePoints.Add(key);
				continue;
			}
			cellGlobal.AddObject(value);
			if (value.CurrentCell != cellGlobal)
			{
				value.Obliterate();
				RemovePoints.Add(key);
			}
		}
		foreach (Point2D removePoint in RemovePoints)
		{
			Field.Remove(removePoint);
		}
	}
}
