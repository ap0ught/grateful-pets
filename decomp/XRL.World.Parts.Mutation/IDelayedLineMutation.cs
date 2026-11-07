using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public abstract class IDelayedLineMutation : BaseMutation
{
	[NonSerialized]
	public List<Cell> _TargetLine;

	public virtual int Delay { get; set; }

	public virtual Cell Target { get; set; }

	public abstract GameObject Projectile { get; }

	public abstract string Command { get; }

	public virtual string Label => GetDisplayName();

	public virtual int PaintDuration => 500;

	public virtual bool CanRefract => false;

	public bool IsGazeBased => (Projectile?.GetPart<TreatAsSolid>())?.GazeBased ?? false;

	public List<Cell> TargetLine
	{
		get
		{
			Cell cell = ParentObject.CurrentCell;
			bool isGazeBased = IsGazeBased;
			if (_TargetLine == null)
			{
				if (cell == null || Target == null)
				{
					return null;
				}
				if (isGazeBased && cell.IsBlackedOut())
				{
					return null;
				}
				List<Point> list = Zone.Line(cell.X, cell.Y, Target.X, Target.Y);
				if (list.Count <= 1)
				{
					List<Cell> list2 = new List<Cell> { cell };
					List<Cell> result = list2;
					_TargetLine = list2;
					return result;
				}
				list.RemoveAt(0);
				_TargetLine = new List<Cell>(GetRange(Level) + 1) { cell };
				int num = 0;
				int num2 = cell.X;
				int num3 = cell.Y;
				int range = GetRange(Level);
				while (_TargetLine.Count <= range)
				{
					int num4 = num % list.Count;
					num2 += list[num4].X - ((num4 > 0) ? list[num4 - 1].X : cell.X);
					num3 += list[num4].Y - ((num4 > 0) ? list[num4 - 1].Y : cell.Y);
					Cell cell2 = cell.ParentZone.GetCell(num2, num3);
					if (cell2 == null || !ParentObject.HasLOSTo(cell2) || (isGazeBased && cell2.IsBlackedOut()))
					{
						break;
					}
					if (cell2 != _TargetLine.Last())
					{
						_TargetLine.Add(cell2);
					}
					num++;
				}
			}
			else if (_TargetLine.Count == 0 || _TargetLine[0] != cell)
			{
				Delay = 0;
				Target = null;
				_TargetLine = null;
			}
			else if (isGazeBased)
			{
				int i = 0;
				for (int count = _TargetLine.Count; i < count; i++)
				{
					if (_TargetLine[i].IsBlackedOut())
					{
						do
						{
							_TargetLine.RemoveAt(i);
						}
						while (_TargetLine.Count > i && _TargetLine[i] != null);
						break;
					}
				}
			}
			return _TargetLine;
		}
	}

	public abstract int GetRange(int Level);

	public abstract int GetDelay(int Level);

	public abstract int GetCooldown(int Level);

	public abstract void FireLine(List<Cell> Path);

	public void Refract(List<Cell> Path)
	{
		Event obj = null;
		for (int i = 1; i < Path.Count; i++)
		{
			Cell cell = Path[i];
			if (!cell.HasObjectWithRegisteredEvent("RefractLight"))
			{
				continue;
			}
			if (obj == null)
			{
				Cell cell2 = Path[0];
				Cell cell3 = Path[Path.Count - 1];
				obj = Event.New("RefractLight");
				obj.SetParameter("Projectile", Projectile);
				obj.SetParameter("Attacker", ParentObject);
				obj.SetParameter("Angle", (float)Math.Atan2(cell3.X - cell2.X, cell3.Y - cell2.Y).toDegrees());
				obj.SetParameter("Sound", "sfx_light_refract");
			}
			obj.SetParameter("Cell", cell);
			obj.SetParameter("Direction", Stat.Random(0, 359));
			obj.SetParameter("Verb", null);
			obj.SetParameter("By", (object)null);
			if (cell.FireEvent(obj))
			{
				continue;
			}
			GameObject Object = obj.GetGameObjectParameter("By");
			if (!GameObject.Validate(ref Object))
			{
				continue;
			}
			Path.RemoveRange(i, Path.Count - i);
			PlayWorldSound(obj.GetStringParameter("Sound"), 0.5f, 0f, Combat: true);
			IComponent<GameObject>.XDidYToZ(Object, obj.GetStringParameter("Verb") ?? "refract", Projectile, "!", null, null, null, Object);
			int num = obj.GetIntParameter("Direction").normalizeDegrees();
			float num2 = cell.X;
			float num3 = cell.Y;
			float num4 = Mathf.Sin((float)num * (MathF.PI / 180f));
			float num5 = Mathf.Cos((float)num * (MathF.PI / 180f));
			Cell cell4 = cell;
			do
			{
				num2 += num4;
				num3 += num5;
				Cell cell5 = cell.ParentZone.GetCell((int)num2, (int)num3);
				if (cell5 == null)
				{
					break;
				}
				if (cell5 != cell4)
				{
					Path.Add(cell4 = cell5);
					if (cell5.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true) != null || cell5.HasSolidObjectForMissile(ParentObject, null, Projectile))
					{
						break;
					}
				}
			}
			while (num2 > 0f && num2 < 79f && num3 > 0f && num3 < 24f && Path.Count < 400);
		}
	}

	public void Perform()
	{
		List<Cell> targetLine = TargetLine;
		Delay = 0;
		Target = null;
		_TargetLine = null;
		if (!targetLine.IsNullOrEmpty())
		{
			if (CanRefract)
			{
				Refract(targetLine);
			}
			FireLine(targetLine);
			UseEnergy(1000);
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
		}
	}

	public virtual void AlertTargetLine()
	{
		if (Target == null)
		{
			return;
		}
		List<Cell> targetLine = TargetLine;
		if (targetLine.IsNullOrEmpty())
		{
			return;
		}
		int i = 0;
		for (int count = targetLine.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = targetLine[i].Objects.Count; j < count2; j++)
			{
				GameObject gameObject = targetLine[i].Objects[j];
				if (gameObject.IsPlayer())
				{
					AutoAct.Interrupt("you are in the path of " + ParentObject.poss(Projectile.DisplayNameOnlyStripped), targetLine[i], null, IsThreat: true);
				}
				else if (gameObject.IsPotentiallyMobile())
				{
					gameObject.Brain.PushGoal(new FleeLocation(targetLine[i], 2));
				}
			}
		}
	}

	public override void OnPaint(ScreenBuffer SB)
	{
		if (Target == null)
		{
			return;
		}
		List<Cell> targetLine = TargetLine;
		if (targetLine.IsNullOrEmpty())
		{
			return;
		}
		int num = (int)(IComponent<GameObject>.frameTimerMS % PaintDuration / (PaintDuration / targetLine.Count));
		for (int i = 0; i < targetLine.Count; i++)
		{
			Cell cell = targetLine[i];
			if (cell.IsVisible() && cell != ParentObject.CurrentCell && cell.ParentZone == ParentObject.CurrentZone)
			{
				ConsoleChar consoleChar = SB[cell];
				if (i == num)
				{
					Color tileBackground = (consoleChar.Background = The.Color.DarkRed);
					consoleChar.TileBackground = tileBackground;
					tileBackground = (consoleChar.Detail = The.Color.Red);
					consoleChar.TileForeground = tileBackground;
				}
				else
				{
					Color tileBackground = (consoleChar.Background = The.Color.Red);
					consoleChar.TileBackground = tileBackground;
					tileBackground = (consoleChar.Detail = The.Color.DarkRed);
					consoleChar.TileForeground = tileBackground;
				}
				consoleChar.SetForeground('r');
			}
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		Writer.WriteOptimized(Delay);
		Writer.Write(Target);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		Delay = Reader.ReadOptimizedInt32();
		Target = Reader.ReadCell();
	}

	public override bool FinalRender(RenderEvent E)
	{
		E.WantsToPaint = Target != null;
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Target != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = ((E.BackgroundString == "&r") ? "&Y" : "&r");
				E.BackgroundString = ((E.BackgroundString == "^R") ? "^Y" : "^R");
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (Target == null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Distance <= GetRange(Level) && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, IsGazeBased))
		{
			E.Add(Command);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Delay > 0)
		{
			if (--Delay <= 0)
			{
				Perform();
			}
			else
			{
				if (ParentObject.IsPlayer())
				{
					The.Core.RenderDelay(500);
				}
				UseEnergy(1000);
				AlertTargetLine();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == Command && PickTarget())
		{
			UseEnergy(1000);
			AlertTargetLine();
			PlayWorldSound("sfx_ability_longBeam_attack_chargeUp", 1f, 0f, Combat: true);
			DidXToY("focus", Projectile, null, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, ParentObject, null, DescribeSubjectDirection: true, DescribeSubjectDirectionLate: false, Target == The.Player.CurrentCell);
		}
		return base.HandleEvent(E);
	}

	public virtual bool IsValidTarget(GameObject Object)
	{
		if (Object != null && Object != ParentObject)
		{
			return Object.IsCombatObject();
		}
		return false;
	}

	public virtual bool PickTarget()
	{
		if (Target != null)
		{
			return false;
		}
		int level = Level;
		List<Cell> list = PickLine(GetRange(level), AllowVis.OnlyVisible, IsValidTarget, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, BlackoutStops: false, ParentObject, Projectile, Label);
		if (list.IsNullOrEmpty())
		{
			return false;
		}
		Cell target = list.Last();
		Target = target;
		Delay = GetDelay(level);
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility(Label, Command, "Physical Mutations", null, "è", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public static List<Cell> GetLine(Cell Origin, Cell Target, int Range = -1, GameObject Looker = null, bool IsGazeBased = false)
	{
		List<Cell> list = new List<Cell>();
		FillLine(list, Origin, Target, Range, Looker, IsGazeBased);
		return list;
	}

	public static void FillLine(List<Cell> Line, Cell Origin, Cell Target, int Range = -1, GameObject Looker = null, bool IsGazeBased = false)
	{
		List<Point> list = Zone.Line(Origin.X, Origin.Y, Target.X, Target.Y, ReadOnly: true);
		int num = list.Count - 1;
		if (num <= 0)
		{
			Line.Add(Origin);
			return;
		}
		list.RemoveAt(0);
		if (Range == -1)
		{
			Range = num;
		}
		Cell cell = Origin;
		Line.EnsureCapacity(Math.Clamp(Range, 1, 16));
		Line.Add(Origin);
		int num2 = 0;
		int num3 = Origin.X;
		int num4 = Origin.Y;
		while (Line.Count <= Range)
		{
			int num5 = num2 % num;
			num3 += list[num5].X - ((num5 > 0) ? list[num5 - 1].X : Origin.X);
			num4 += list[num5].Y - ((num5 > 0) ? list[num5 - 1].Y : Origin.Y);
			Cell cell2 = Origin.ParentZone.GetCell(num3, num4);
			if (cell2 != null && (Looker == null || Looker.HasLOSTo(cell2)) && (!IsGazeBased || !cell2.IsBlackedOut()))
			{
				if (cell2 != cell)
				{
					Line.Add(cell2);
					cell = cell2;
				}
				num2++;
				continue;
			}
			break;
		}
	}
}
