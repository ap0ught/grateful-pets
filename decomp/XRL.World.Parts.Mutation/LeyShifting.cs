using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LeyShifting : BaseMutation
{
	public const string COMMAND_NAME = "CommandLeyShift";

	public const int ABL_CLD = 250;

	[NonSerialized]
	private bool Active;

	[NonSerialized]
	private List<GameObject> ShiftList = new List<GameObject>();

	[NonSerialized]
	private List<GameObject> ShiftList2 = new List<GameObject>();

	[NonSerialized]
	private Renderable ShiftPaint = new Renderable
	{
		RenderString = null
	};

	[NonSerialized]
	private Renderable ShiftPaint2 = new Renderable
	{
		RenderString = null
	};

	public LeyShifting()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public int GetCooldown(int Level)
	{
		return 250;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && (ID != BeforeRenderEvent.ID || !Active) && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == GetMovementCapabilitiesEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Ley Shift", "CommandLeyShift", 25000, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 2 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandLeyShift");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Active)
		{
			Cell cell = ParentObject.CurrentCell;
			cell.ParentZone.AddLight(cell.X, cell.Y, 1);
			cell.ParentZone.AddVisibility(cell.X, cell.Y, 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandLeyShift")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			if (!TryGetDestination(out var Direction, out var Amount))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that here.");
				}
				return false;
			}
			Event.PinCurrentPool();
			bool useOverlayCombatEffects = Options.UseOverlayCombatEffects;
			int renderLayer = ParentObject.Render.RenderLayer;
			ParentObject.Render.RenderLayer = 100;
			Active = true;
			DidX("shift", "spacetime in the local region", null, null, null, ParentObject);
			if (useOverlayCombatEffects)
			{
				CombatJuice.Hover(ParentObject.CurrentCell.Location, (float)Amount * 0.3f + 1f, 2f);
				The.Core.RenderDelay(2000, Interruptible: false);
			}
			GameManager.Instance.Fuzzing = true;
			for (int i = 1; i <= Amount; i++)
			{
				Shift(Direction);
				SoundManager.PlaySound((i == Amount) ? "sfx_statusEffect_spacetimeWeirdness" : "sfx_ability_leyShift_repetition");
				if (useOverlayCombatEffects)
				{
					CombatJuice.cameraShake((i == Amount) ? 2f : 0.3f);
				}
				The.Core.RenderDelay(300, Interruptible: false);
				Event.ResetToPin();
				MinEvent.ResetPools();
			}
			if (useOverlayCombatEffects)
			{
				The.Core.RenderDelay(3000);
				CombatJuice.finishAll();
			}
			ParentObject.Render.RenderLayer = renderLayer;
			Active = false;
			UseEnergy(1000, "Mental Mutation LeyShifting");
			CooldownMyActivatedAbility(ActivatedAbilityID, 250, ParentObject);
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool TryGetDestination(out string Direction, out int Amount)
	{
		Cell cell = ParentObject.CurrentCell;
		Zone parentZone = cell.ParentZone;
		bool flag = false;
		int num = 100;
		do
		{
			Amount = Stat.Random(8, 12) + 1;
			Direction = Location2D.Directions.GetRandomElement();
			Point2D point2D = Point2D.zero.FromDirection(Direction);
			Cell cell2 = parentZone.GetCell(WrapX(parentZone, cell.X - point2D.x * Amount), WrapY(parentZone, cell.Y - point2D.y * Amount));
			flag = cell2 != null && ((num > 50) ? cell2.IsEmptyAtRenderLayer(1) : cell2.IsEmpty());
		}
		while (!flag && num-- > 0);
		return flag;
	}

	public override string GetDescription()
	{
		return string.Concat("" + "You shift spacetime in the local region.\n\n", "Cooldown: ", 250.ToString());
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public void Shift(string Direction)
	{
		Zone currentZone = ParentObject.CurrentZone;
		Point2D point2D = Point2D.zero.FromDirection(Direction);
		int num = currentZone.Width * currentZone.Height;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		if (point2D.x == 0 && point2D.y != 0)
		{
			num3 = 1;
		}
		else
		{
			num5 = 1;
		}
		while (num > 0)
		{
			int num6 = num2;
			int num7 = num4;
			do
			{
				Cell c = currentZone.Map[num2][num4];
				TakeObjects(c);
				TakePaint(c);
				ApplyObjects(c);
				ApplyPaint(c);
				num2 = WrapX(currentZone, num2 + point2D.x);
				num4 = WrapY(currentZone, num4 + point2D.y);
				num--;
			}
			while (num2 != num6 || num4 != num7);
			Cell c2 = currentZone.Map[num6][num7];
			ApplyObjects(c2);
			ApplyPaint(c2);
			num2 += num3;
			num4 += num5;
		}
		ZoneManager.PaintWalls(currentZone);
		ZoneManager.PaintWater(currentZone);
	}

	public void TakeObjects(Cell C)
	{
		for (int num = C.Objects.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = C.Objects[num];
			if (gameObject != ParentObject && (gameObject.IsReal || (gameObject.Render != null && gameObject.Render.Visible)))
			{
				ShiftList.Add(C.Objects[num]);
				C.RemoveObject(C.Objects[num], Forced: false, System: false, IgnoreGravity: false, Silent: false, NoStack: false, Repaint: false);
			}
		}
	}

	public void ApplyObjects(Cell C)
	{
		for (int num = ShiftList2.Count - 1; num >= 0; num--)
		{
			C.AddObject(ShiftList2[num], Forced: false, System: false, IgnoreGravity: false, NoStack: false, Silent: false, Repaint: false);
		}
		List<GameObject> shiftList = ShiftList;
		ShiftList = ShiftList2;
		ShiftList2 = shiftList;
		ShiftList.Clear();
	}

	public void ApplyPaint(Cell C)
	{
		if (ShiftPaint.RenderString != null)
		{
			C.PaintTile = ShiftPaint.Tile;
			C.PaintRenderString = ShiftPaint.RenderString;
			C.PaintColorString = ShiftPaint.ColorString;
			C.PaintTileColor = ShiftPaint.TileColor;
			C.PaintDetailColor = ((ShiftPaint.DetailColor == '\0') ? null : ShiftPaint.DetailColor.ToString());
			ShiftPaint.RenderString = null;
		}
		Renderable shiftPaint = ShiftPaint;
		ShiftPaint = ShiftPaint2;
		ShiftPaint2 = shiftPaint;
	}

	public void TakePaint(Cell C)
	{
		ShiftPaint2.Tile = C.PaintTile;
		ShiftPaint2.RenderString = C.PaintRenderString;
		ShiftPaint2.ColorString = C.PaintColorString;
		ShiftPaint2.TileColor = C.PaintTileColor;
		ShiftPaint2.DetailColor = ((!string.IsNullOrEmpty(C.PaintDetailColor)) ? C.PaintDetailColor[0] : '\0');
	}

	public int WrapX(Zone Z, int X)
	{
		while (X >= Z.Width)
		{
			X -= Z.Width;
		}
		while (X < 0)
		{
			X += Z.Width;
		}
		return X;
	}

	public int WrapY(Zone Z, int Y)
	{
		while (Y >= Z.Height)
		{
			Y -= Z.Height;
		}
		while (Y < 0)
		{
			Y += Z.Height;
		}
		return Y;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Ley Shift", "CommandLeyShift", "Mental Mutations", null, "\u00a8");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
