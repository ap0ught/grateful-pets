using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using Qud.UI;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class IrisdualBeam : BaseMutation
{
	public const string ABL_COMMAND = "CommandIrisdualBeam";

	public const int SND_MAX_COST = 20;

	[NonSerialized]
	public static string[] DamageTypes = new string[7] { "Acid", "Electric", "Heat", "Cold", "Poison", "Umbral", "Cosmic" };

	public int Duration;

	public int Delay;

	[NonSerialized]
	private bool PausePaint;

	[NonSerialized]
	private bool AllowPaint;

	[NonSerialized]
	private List<GameObject> Objects = new List<GameObject>();

	[NonSerialized]
	private List<Location2D> Targets = new List<Location2D>();

	[NonSerialized]
	private List<List<Cell>> Lines = new List<List<Cell>>();

	[NonSerialized]
	private List<string> Threatened = new List<string>();

	[NonSerialized]
	private long RecalculateSegment;

	[NonSerialized]
	private static bool Preloaded;

	[NonSerialized]
	private static GameObject _Projectile;

	public GameObject Projectile
	{
		get
		{
			if (!GameObject.Validate(ref _Projectile))
			{
				_Projectile = GameObject.CreateUnmodified("ProjectileIrisdualBeam");
			}
			return _Projectile;
		}
	}

	public int GetDelay(int Level)
	{
		return 1;
	}

	public int GetBeams(int Level)
	{
		return Mathf.RoundToInt((float)Level * 0.6f);
	}

	public int GetDuration(int Level)
	{
		return GetDelay(Level) * 3;
	}

	public int GetCooldown(int Level)
	{
		return Mathf.RoundToInt(36f - (float)Level * 1.1f);
	}

	public int GetMinimumRange(int Level)
	{
		return 1;
	}

	public override string GetDescription()
	{
		return "You molt powerful beams across the spectrum of light and matter.";
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string text = ((GetBeams(Level) == 1) ? "beam" : "beams");
		stringBuilder.Append("Fires {{rules|").Append(GetBeams(Level)).Append("}} " + text + " at random enemies every round for {{rules|")
			.Append(GetDuration(Level))
			.Append("}} rounds\n");
		stringBuilder.Append("Cooldown: {{rules|").Append(GetCooldown(Level)).Append("}} rounds");
		return stringBuilder.ToString();
	}

	public override void CollectStats(Templates.StatCollector Collector, int Level)
	{
		Collector.Set("Beams", GetBeams(Level));
		Collector.Set("Duration", GetDuration(Level));
		Collector.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
		Writer.WriteOptimized(Targets.Count);
		foreach (Location2D target in Targets)
		{
			Writer.WriteOptimized(target.X);
			Writer.WriteOptimized(target.Y);
		}
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		int num = Reader.ReadOptimizedInt32();
		Targets.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			int x = Reader.ReadOptimizedInt32();
			int y = Reader.ReadOptimizedInt32();
			Targets.Add(Location2D.Get(x, y));
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != EnteredCellEvent.ID && ID != LeftCellEvent.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == SingletonEvent<AfterGameLoadedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (Duration <= 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandIrisdualBeam", 10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (!Preloaded)
		{
			PreloadClips();
		}
		if (Duration > 0)
		{
			LockRefractors();
			if (--Delay > 0)
			{
				if (ParentObject.IsPlayer())
				{
					The.Core.RenderDelay(500);
				}
				ParentObject.ForfeitTurn();
				AlertTargetLine();
			}
			else
			{
				Perform();
				if (Duration > 1)
				{
					PickTarget();
					AlertTargetLine();
				}
			}
			if (--Duration == 0)
			{
				ParentObject.RemoveEffect(typeof(IrisdualMolting));
				ParentObject.ApplyEffect(new IrisdualCallow(5));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Duration > 0 && !Targets.IsNullOrEmpty())
		{
			RecalculateLines(Prospective: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		if (Duration > 0 && !Targets.IsNullOrEmpty())
		{
			LockRefractors();
		}
		return base.HandleEvent(E);
	}

	public bool IsValidTarget(GameObject Object)
	{
		if (Object.InSameCellAs(ParentObject))
		{
			return false;
		}
		if (!ParentObject.IsHostileTowards(Object) && !Object.IsHostileTowards(ParentObject))
		{
			return Object.HasPart<RefractCosmic>();
		}
		return true;
	}

	public int CompareTarget(GameObject First, GameObject Second)
	{
		bool flag = Threatened.Contains(ParentObject.GetDirectionToward(First));
		bool value = Threatened.Contains(ParentObject.GetDirectionToward(Second));
		int num = flag.CompareTo(value);
		if (num != 0)
		{
			return num;
		}
		GameObject target = ParentObject.Target;
		if (target != null)
		{
			flag = First == target;
			num = (Second == target).CompareTo(flag);
			if (num != 0)
			{
				return num;
			}
		}
		flag = First.HasPart<RefractCosmic>();
		value = Second.HasPart<RefractCosmic>();
		return flag.CompareTo(value);
	}

	public bool PickTarget()
	{
		if (!Targets.IsNullOrEmpty())
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell.OnWorldMap())
		{
			return ParentObject.Fail("You cannot do that on the world map.");
		}
		List<GameObject> list = cell.ParentZone.FastCombatSquareVisibility(cell.X, cell.Y, 9999999, ParentObject, IsValidTarget, VisibleToPlayerOnly: false, IncludeWalls: true);
		Threatened.Clear();
		foreach (Cell item in cell.YieldAdjacentCells(1, LocalOnly: true))
		{
			if (item.HasCombatObject())
			{
				Threatened.Add(cell.GetDirectionFromCell(item));
			}
		}
		list.ShuffleInPlace();
		Algorithms.StableSortInPlace(list, CompareTarget);
		ClearTargets();
		int level = Level;
		int beams = GetBeams(level);
		int i = 0;
		int num = Math.Min(beams, list.Count);
		for (; i < beams; i++)
		{
			Cell cell2 = null;
			if (i >= num)
			{
				for (int j = 0; j < 25; j++)
				{
					cell2 = cell.ParentZone.GetRandomCell();
					if (!IsDirectionTargeted(cell2))
					{
						break;
					}
				}
			}
			else
			{
				cell2 = list[i].CurrentCell;
			}
			if (cell2 != null)
			{
				Targets.Add(cell2.Location);
			}
		}
		Delay = GetDelay(level);
		return true;
	}

	public bool IsDirectionTargeted(Cell Cell)
	{
		Cell cell = ParentObject.CurrentCell;
		string directionFrom = cell.GetDirectionFrom(Cell.Location);
		foreach (Location2D target in Targets)
		{
			if (cell.GetDirectionFrom(target) == directionFrom)
			{
				return true;
			}
		}
		return false;
	}

	public void Perform()
	{
		bool flag = ParentObject.IsVisible();
		IrisdualMolting effect = ParentObject.GetEffect<IrisdualMolting>();
		try
		{
			PausePaint = true;
			bool flag2 = false;
			RecalculateLines(Prospective: false, Force: true);
			if (flag)
			{
				FadeToBlack.SetTileMode();
				FadeToBlack.FadeOut(1f, new Color(0f, 0f, 0f, 0.25f));
				effect?.SetActive(State: false);
				PlayWorldSound("Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_windup", 0.5f, 0f, Combat: false, null, 0f, 1f, 0.2f, 20);
				CombatJuice.playPrefabAnimation(ParentObject, "Particles/BeamWarmUp", null, null, null, async: true);
				flag2 = flag2 || !The.Core.RenderDelay(1000);
				MissileWeaponVFXConfiguration VFXConfig = null;
				for (int i = 0; i < Lines.Count; i++)
				{
					CollectVFX(i, ref VFXConfig);
				}
				if (VFXConfig != null)
				{
					CombatJuice.missileWeaponVFX(VFXConfig, Async: true);
					CombatJuice.cameraShake(1.5f, Async: true);
				}
			}
			PlayBeamSound();
			for (int j = 0; j < Lines.Count; j++)
			{
				FireLine(j);
			}
			if (flag)
			{
				flag2 = flag2 || !The.Core.RenderDelay(1500);
				FadeToBlack.FadeIn(0.5f, new Color(0f, 0f, 0f, 0.25f));
				if (flag2 || !The.Core.RenderDelay(500))
				{
					CombatJuice.StopPrefabAnimation("Particles/BeamWarmUp");
				}
			}
		}
		finally
		{
			PausePaint = false;
			Delay = 0;
			ClearTargets();
			UseEnergy(1000);
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			effect?.SetActive(State: true);
			if (flag)
			{
				FadeToBlack.FadeIn(0f, new Color(0f, 0f, 0f, 0f));
				FadeToBlack.SetUIMode();
			}
		}
	}

	public void CollectVFX(int Index, ref MissileWeaponVFXConfiguration VFXConfig)
	{
		List<Cell> list = Lines[Index];
		if (Projectile.HasTagOrProperty("ProjectileVFX") && list.Count > 1)
		{
			string propertyOrTag = Projectile.GetPropertyOrTag("ProjectileVFXConfiguration");
			if (VFXConfig == null)
			{
				VFXConfig = MissileWeaponVFXConfiguration.next();
			}
			VFXConfig.addStep(Index, list[0].Location);
			VFXConfig.addStep(Index, list[list.Count - 1].Location);
			VFXConfig.setPathProjectileVFX(Index, Projectile.GetPropertyOrTag("ProjectileVFX"), propertyOrTag);
		}
	}

	public void FireLine(int Index)
	{
		List<GameObject> objects = Objects;
		List<Cell> list = Lines[Index];
		Projectile part = Projectile.GetPart<Projectile>();
		objects.Clear();
		int i = 1;
		for (int count = list.Count; i < count; i++)
		{
			foreach (GameObject @object in list[i].Objects)
			{
				if (@object != ParentObject && @object.IsReal && !@object.HasPart<RefractCosmic>())
				{
					objects.Add(@object);
				}
			}
			for (int num = objects.Count - 1; num >= 0; num--)
			{
				InflictDamage(objects[num], part);
				objects.RemoveAt(num);
			}
		}
	}

	public void PlayBeamSound()
	{
		Cell cell = ParentObject.CurrentCell;
		Cell playerCell = The.PlayerCell;
		if (playerCell != null && cell.ParentZone == playerCell.ParentZone)
		{
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			int i = 0;
			for (int count = Lines.Count; i < count; i++)
			{
				List<Cell> list = Lines[i];
				int j = 1;
				for (int count2 = list.Count; j < count2; j++)
				{
					Cell cell2 = list[j];
					int costAtPoint = Zone.SoundMap.GetCostAtPoint(cell2.Location);
					int num3 = cell2.PathDistanceTo(playerCell);
					if (costAtPoint < num)
					{
						cell = cell2;
						num = costAtPoint;
						num2 = cell2.PathDistanceTo(playerCell);
					}
					else if (costAtPoint == num && num3 < num2)
					{
						cell = cell2;
						num2 = cell2.PathDistanceTo(playerCell);
					}
				}
			}
		}
		cell.PlayWorldSound("sfx_creature_girshNephilim_irisdualBeam_attack", 0.5f, 0f, Combat: false, 0f, 1f, 0.5f, 20);
	}

	public void InflictDamage(GameObject Object, Projectile ProjectilePart)
	{
		int num = Stat.RollDamagePenetrations(Stats.GetCombatAV(Object), ProjectilePart.BasePenetration, ProjectilePart.BasePenetration);
		int Amount = 0;
		for (int i = 0; i < num; i++)
		{
			Amount += ProjectilePart.BaseDamage.RollCached();
		}
		if (Amount > 0)
		{
			Object.TakeDamage(ref Amount, null, null, null, null, ParentObject, null, null, null, "");
		}
		int j = 0;
		for (int num2 = DamageTypes.Length; j < num2; j++)
		{
			int num3 = Stat.Random(30, 40);
			Object.TakeDamage(ref num3, Attacker: ParentObject, Attributes: DamageTypes[j], DeathReason: null, ThirdPersonDeathReason: null, Owner: null, Source: null, Perspective: null, DescribeAsFrom: null, Message: "");
			Amount += num3;
		}
		if (Amount > 0)
		{
			Physics physics = Object.Physics;
			string preposition = Amount + " damage from";
			GameObject projectile = Projectile;
			GameObject parentObject = ParentObject;
			physics.DidXToY("take", preposition, projectile, null, null, null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, parentObject);
		}
	}

	public void ClearTargets()
	{
		Targets.Clear();
		ClearLines();
	}

	public void ClearLines()
	{
		RecalculateSegment = 0L;
		foreach (List<Cell> line in Lines)
		{
			line.Clear();
		}
	}

	public List<Cell> TakeLine()
	{
		foreach (List<Cell> line in Lines)
		{
			if (line.IsNullOrEmpty())
			{
				return line;
			}
		}
		List<Cell> list = new List<Cell>();
		Lines.Add(list);
		return list;
	}

	public void RecalculateLines(bool Prospective = false, bool Force = false)
	{
		long segments = The.Game.Segments;
		if (!Force && segments == RecalculateSegment)
		{
			return;
		}
		ClearLines();
		Cell cell = ParentObject.CurrentCell;
		foreach (Location2D target in Targets)
		{
			IDelayedLineMutation.FillLine(TakeLine(), cell, cell.ParentZone.GetCell(target), 80);
		}
		Objects.Clear();
		for (int i = 0; i < Lines.Count; i++)
		{
			Refract(i, Objects, Prospective);
		}
		RecalculateSegment = segments;
	}

	public void Refract(int Index, List<GameObject> Refractors, bool Prospective = false)
	{
		List<Cell> list = Lines[Index];
		for (int i = 1; i < list.Count; i++)
		{
			Cell cell = list[i];
			if (!cell.TryGetFirstObjectPart<RefractCosmic>(out var Part))
			{
				continue;
			}
			GameObject parentObject = Part.ParentObject;
			if (!GameObject.Validate(parentObject) || Refractors.Contains(parentObject))
			{
				continue;
			}
			ReadOnlySpan<float> directions = Part.GetDirections(list[0].Location);
			if (directions.IsEmpty)
			{
				continue;
			}
			Refractors.Add(parentObject);
			list.RemoveRange(i + 1, list.Count - i - 1);
			if (!Prospective)
			{
				cell.PlayWorldSound("sfx_light_refract", 0.5f, 0f, Combat: true);
				IComponent<GameObject>.XDidYToZ(parentObject, "refract", Projectile, null, "!", null, null, parentObject);
			}
			int j = 0;
			for (int length = directions.Length; j < length; j++)
			{
				List<Cell> list2 = TakeLine();
				list2.Add(cell);
				float num = directions[j];
				float num2 = cell.X;
				float num3 = cell.Y;
				float num4 = Mathf.Sin(num * (MathF.PI / 180f));
				float num5 = Mathf.Cos(num * (MathF.PI / 180f));
				Cell cell2 = cell;
				do
				{
					num2 += num4;
					num3 += num5;
					Cell cell3 = cell.ParentZone.GetCell((int)num2, (int)num3);
					if (cell3 == null)
					{
						break;
					}
					if (cell3 != cell2)
					{
						list2.Add(cell2 = cell3);
					}
				}
				while (num2 > 0f && num2 < 79f && num3 > 0f && num3 < 24f && list.Count < 400);
			}
		}
	}

	public void PreloadClips()
	{
		SoundManager.PreloadClipSet("Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_windup");
		SoundManager.PreloadClipSet("Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_attack");
		SoundManager.PreloadClipSet("Sounds/Creatures/Ability/sfx_creature_girshNephilim_irisdualBeam_molting_lp");
		Preloaded = true;
	}

	public void Stop()
	{
		Duration = 0;
		Delay = 0;
		PausePaint = false;
		ClearTargets();
		ParentObject.RemoveEffect(typeof(IrisdualMolting));
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandIrisdualBeam" && PickTarget())
		{
			Duration = GetDuration(Level);
			ParentObject.ApplyEffect(new IrisdualMolting(Duration + 1));
			CombatJuice.cameraShake(1f);
			ParentObject.ForfeitTurn();
			AlertTargetLine();
			LockRefractors();
			DidX("start", "to shine over the full spectrum", "!", null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true);
			if (ParentObject.IsInActiveZone())
			{
				AutoAct.Interrupt();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Irisdual Beam", "CommandIrisdualBeam", "Mutation", null, "è", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: false);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public override void OnPaint(ScreenBuffer SB)
	{
		if (PausePaint || !AllowPaint || Targets.IsNullOrEmpty())
		{
			return;
		}
		RecalculateLines(Prospective: true);
		int i = 0;
		for (int count = Lines.Count; i < count; i++)
		{
			List<Cell> list = Lines[i];
			if (list.IsNullOrEmpty())
			{
				break;
			}
			int num = (int)(IComponent<GameObject>.frameTimerMS % 500 / (500 / list.Count));
			for (int j = 0; j < list.Count; j++)
			{
				Cell cell = list[j];
				if (cell.IsVisible() && cell != ParentObject.CurrentCell && cell.ParentZone == ParentObject.CurrentZone)
				{
					ConsoleChar consoleChar = SB[cell];
					if (j == num)
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
	}

	public override bool FinalRender(RenderEvent E)
	{
		AllowPaint = !PausePaint && !Targets.IsNullOrEmpty();
		E.WantsToPaint = E.WantsToPaint || AllowPaint;
		return true;
	}

	public void LockRefractors()
	{
		Zone.ObjectEnumerator enumerator = ParentObject.CurrentZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.IsCombatObject() && current.HasPart<RefractCosmic>())
			{
				if (current.Brain.Goals.Peek() is Wait wait)
				{
					wait.TicksLeft = Math.Max(wait.TicksLeft, Duration + 1);
					continue;
				}
				current.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
				current.Brain.PushGoal(new Wait(Duration + 1, "there's an incoming beam to refract"));
			}
		}
	}

	public void AlertTargetLine()
	{
		if (Targets.IsNullOrEmpty())
		{
			return;
		}
		RecalculateLines(Prospective: true);
		int i = 0;
		for (int count = Lines.Count; i < count; i++)
		{
			int j = 1;
			for (int count2 = Lines[i].Count; j < count2; j++)
			{
				Cell cell = Lines[i][j];
				int k = 0;
				for (int count3 = cell.Objects.Count; k < count3; k++)
				{
					GameObject gameObject = cell.Objects[k];
					if (gameObject.IsCombatObject())
					{
						if (gameObject.IsPlayer())
						{
							AutoAct.Interrupt("you are in the path of " + ParentObject.poss("irisdual beam"), cell, null, IsThreat: true);
						}
						else if (gameObject.IsPotentiallyMobile() && !gameObject.HasPart<RefractCosmic>() && !(gameObject.Brain.Goals.Peek() is FleeLocation))
						{
							gameObject.Brain.RemoveGoalsDescendedFrom<IMovementGoal>();
							gameObject.Brain.PushGoal(new FleeLocation(cell, 2));
						}
					}
				}
			}
		}
	}
}
