using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: the range, number of bounces, and damage roll are
/// increased by the standard power load bonus, i.e. 2 for the standard
/// overload power load of 400, and charge usage is adjusted using power
/// load as a percentage.
/// </remarks>
[Serializable]
public class GeomagneticDisc : IPoweredPart
{
	public string Damage = "2d6";

	public string Bounces = "5";

	public string Range = "12";

	static GeomagneticDisc()
	{
	}

	public GeomagneticDisc()
	{
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		GeomagneticDisc geomagneticDisc = p as GeomagneticDisc;
		if (geomagneticDisc.Damage != Damage)
		{
			return false;
		}
		if (geomagneticDisc.Bounces != Bounces)
		{
			return false;
		}
		if (geomagneticDisc.Range != Range)
		{
			return false;
		}
		return base.SameAs(geomagneticDisc);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != PooledEvent<GetThrownWeaponPerformanceEvent>.ID)
		{
			return ID == PooledEvent<GetThrownWeaponRangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetThrownWeaponPerformanceEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.BaseDamage = Damage;
			int num = MyPowerLoadBonus();
			if (num != 0)
			{
				E.DamageModifier += num;
			}
			E.Vorpal = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetThrownWeaponRangeEvent E)
	{
		E.MaxRange = (E.Actor?.GetBaseThrowRange(E.Object) ?? 0) + ParentObject.GetIntProperty("ThrowRangeBonus") + MyPowerLoadBonus() + Range.RollCached();
		return false;
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeThrown");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeThrown")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Thrower");
			if (gameObjectParameter == null)
			{
				return false;
			}
			if (ParentObject.CurrentCell != null)
			{
				return false;
			}
			GameObject gameObject = E.GetGameObjectParameter("ApparentTarget");
			Cell cell = E.GetParameter("TargetCell") as Cell;
			int num = MyPowerLoadLevel();
			int num2 = IComponent<GameObject>.PowerLoadBonus(num);
			int num3 = gameObjectParameter.GetBaseThrowRange(ParentObject, null, cell, gameObjectParameter.DistanceTo(cell)) + ParentObject.GetIntProperty("ThrowRangeBonus") + num2 + Range.RollCached();
			int? powerLoadLevel = num;
			bool flag = IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel);
			IThrownWeaponFlexPhaseProvider thrownWeaponFlexPhaseProvider = GetThrownWeaponFlexPhaseProviderEvent.GetFor(ParentObject, gameObjectParameter);
			thrownWeaponFlexPhaseProvider?.ThrownWeaponFlexPhaseStart(ParentObject);
			try
			{
				FindPath findPath = null;
				if (gameObject == null || gameObject == gameObjectParameter)
				{
					List<GameObject> list = gameObjectParameter.CurrentCell.FastFloodSearch("Brain", num3);
					if (list.Count > 0)
					{
						list.ShuffleInPlace();
						foreach (GameObject item in list)
						{
							if (item != gameObjectParameter && (gameObjectParameter.IsHostileTowards(item) || item.IsHostileTowards(gameObjectParameter)))
							{
								FindPath findPath2 = new FindPath(gameObjectParameter.CurrentCell, item.CurrentCell, PathGlobal: false, PathUnlimited: true, ParentObject, 99, ExploredOnly: false, Juggernaut: false, IgnoreCreatures: true, IgnoreGases: true, thrownWeaponFlexPhaseProvider?.ThrownWeaponFlexPhaseIsActive(ParentObject) ?? false);
								if (findPath2.Usable && findPath2.Directions.Count <= num3)
								{
									gameObject = item;
									findPath = findPath2;
									break;
								}
							}
						}
					}
					if (gameObject == null)
					{
						if (flag)
						{
							SignalFailure(gameObjectParameter);
							return false;
						}
						return gameObjectParameter.IsPlayer();
					}
				}
				if (!flag)
				{
					return true;
				}
				if (gameObjectParameter.DistanceTo(gameObject) > num3)
				{
					SignalFailure(gameObjectParameter);
					return false;
				}
				if (findPath == null)
				{
					findPath = new FindPath(gameObjectParameter.CurrentCell, gameObject.CurrentCell, PathGlobal: false, PathUnlimited: true, ParentObject, 99, ExploredOnly: false, Juggernaut: false, IgnoreCreatures: true, IgnoreGases: true, thrownWeaponFlexPhaseProvider?.ThrownWeaponFlexPhaseIsActive(ParentObject) ?? false);
					if (!findPath.Usable || findPath.Directions.Count > num3)
					{
						SignalFailure(gameObjectParameter);
						return false;
					}
				}
				powerLoadLevel = num;
				if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
				{
					SignalLowPower(gameObjectParameter);
					return false;
				}
				if (gameObjectParameter.IsPlayer())
				{
					gameObjectParameter.Target = gameObject;
				}
				GameObject firstThrownWeapon = gameObjectParameter.GetFirstThrownWeapon((GameObject o) => o != ParentObject && o.TryGetPart<GeomagneticDisc>(out var Part) && Part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L));
				gameObjectParameter.UseEnergy(1000, "Physical Skill Throwing Item");
				if (firstThrownWeapon != null)
				{
					IComponent<GameObject>.EmitMessage(gameObjectParameter, ParentObject.T() + " and " + firstThrownWeapon.t() + " collide and fall to the ground.");
					gameObjectParameter.CurrentCell.AddObject(ParentObject);
					gameObjectParameter.CurrentCell.AddObject(firstThrownWeapon);
					return false;
				}
				List<GameObject> list2 = Event.NewGameObjectList();
				List<FindPath> list3 = new List<FindPath>();
				list2.Add(gameObject);
				list3.Add(findPath);
				int num4 = Bounces.RollCached() + num2;
				for (int num5 = 0; num5 < num4 && num5 < list2.Count; num5++)
				{
					List<GameObject> list4 = list2[num5].CurrentCell.FastFloodSearch("Brain", num3);
					if (list4.Count <= 0)
					{
						continue;
					}
					list4.ShuffleInPlace();
					foreach (GameObject item2 in list4)
					{
						if (item2 == gameObjectParameter || list2.Contains(item2) || (!gameObjectParameter.IsHostileTowards(item2) && !item2.IsHostileTowards(gameObjectParameter)))
						{
							continue;
						}
						FindPath findPath3 = new FindPath(list2[num5].CurrentCell, item2.CurrentCell, PathGlobal: false, PathUnlimited: true, ParentObject, 99, ExploredOnly: false, Juggernaut: false, IgnoreCreatures: true, IgnoreGases: true, thrownWeaponFlexPhaseProvider?.ThrownWeaponFlexPhaseIsActive(ParentObject) ?? false);
						if (findPath3.Usable && findPath3.Directions.Count <= num3)
						{
							powerLoadLevel = num;
							if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
							{
								list2.Add(item2);
								list3.Add(findPath3);
							}
							break;
						}
					}
				}
				DoThrow(gameObjectParameter, list3, UnidentifiedMessage: false, UsePopups: false, list2, null, E.GetIntParameter("Phase"), num2, thrownWeaponFlexPhaseProvider, E);
				return false;
			}
			finally
			{
				thrownWeaponFlexPhaseProvider?.ThrownWeaponFlexPhaseEnd(ParentObject);
			}
		}
		return base.FireEvent(E);
	}

	private void DoThrow(GameObject Actor, List<FindPath> Paths, bool UnidentifiedMessage = false, bool UsePopups = false, List<GameObject> Targets = null, GameObject LastTarget = null, int Phase = 0, int? Bonus = null, IThrownWeaponFlexPhaseProvider FlexPhaseProvider = null, IEvent ParentEvent = null)
	{
		if (Paths == null || Paths.Count <= 0)
		{
			return;
		}
		ParentObject.GetContext(out var ObjectContext, out var CellContext, out var BodyPartContext, out var Relation, out var RelationManager);
		try
		{
			PlayWorldSound(ParentObject.GetTagOrStringProperty("ThrownSound", "Sounds/Throw/sfx_throwing_geoMagneticDisc_throw"));
			Cell cell = CellContext ?? ParentObject.GetCurrentCell();
			bool flag = Actor.CurrentZone.IsActive();
			bool flag2 = flag && Options.UseParticleVFX;
			string text = ParentObject.t();
			string text2 = "from " + text + ".";
			int num = Bonus ?? MyPowerLoadBonus();
			if (Phase == 0)
			{
				Phase = XRL.World.Capabilities.Phase.getWeaponPhase(Actor, GetActivationPhaseEvent.GetFor(ParentObject));
			}
			int num2 = Actor.StatMod("Agility") + Actor.GetIntProperty("ThrowToHitBonus") + Actor.GetIntProperty("ThrowToHitSkillBonus");
			if (RelationManager != null)
			{
				ParentObject.RemoveFromContext();
			}
			FindPath findPath = new FindPath(Paths.Last().Steps.Last(), Paths[0].Steps[0], PathGlobal: false, PathUnlimited: true, ParentObject, 99, ExploredOnly: false, Juggernaut: false, IgnoreCreatures: true, IgnoreGases: true, FlexPhaseProvider?.ThrownWeaponFlexPhaseIsActive(ParentObject) ?? false);
			if (findPath.Usable)
			{
				Paths.Add(findPath);
			}
			List<GameObject> objectsThatWantEvent = Actor.CurrentZone.GetObjectsThatWantEvent(PooledEvent<ProjectileMovingEvent>.ID, ProjectileMovingEvent.CascadeLevel);
			ProjectileMovingEvent projectileMovingEvent = null;
			if (objectsThatWantEvent.Count > 0)
			{
				projectileMovingEvent = PooledEvent<ProjectileMovingEvent>.FromPool();
				projectileMovingEvent.Attacker = Actor;
				projectileMovingEvent.Projectile = ParentObject;
				projectileMovingEvent.Throw = true;
			}
			TextConsole textConsole = Look._TextConsole;
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
			ScreenBuffer scrapBuffer2 = TextConsole.ScrapBuffer2;
			if (flag && !flag2)
			{
				scrapBuffer.Draw();
				The.Core.RenderMapToBuffer(scrapBuffer);
				scrapBuffer2.Copy(scrapBuffer);
			}
			RenderEvent r = ((flag && !flag2) ? ParentObject.RenderForUI(null, AsIfKnown: true) : null);
			int i = 0;
			for (int count = Paths.Count; i < count; i++)
			{
				FindPath findPath2 = Paths[i];
				GameObject gameObject = null;
				if (findPath2 == findPath)
				{
					gameObject = LastTarget;
				}
				else if (Targets != null && Targets.Count > i)
				{
					gameObject = Targets[i];
				}
				if (projectileMovingEvent != null)
				{
					projectileMovingEvent.ApparentTarget = gameObject;
					projectileMovingEvent.TargetCell = gameObject?.CurrentCell ?? Actor.CurrentCell;
					projectileMovingEvent.Path = findPath2.Steps.Select((Cell c) => c.Point).ToList();
					projectileMovingEvent.ScreenBuffer = scrapBuffer;
				}
				bool showUninvolved = false;
				int num3 = 0;
				for (int num4 = findPath2.Steps.Count - 1; num3 <= num4; num3++)
				{
					findPath2.Steps[num3].WakeCreaturesInArea();
					if (FlexPhaseProvider != null)
					{
						if (!FlexPhaseProvider.ThrownWeaponFlexPhaseTraversal(Actor, (gameObject != null && findPath2.Steps[num3].Objects.Contains(gameObject)) ? gameObject : null, gameObject, ParentObject, Phase, cell, findPath2.Steps[num3], out var _, out var RecheckPhase, HasDynamicTargets: true))
						{
							return;
						}
						if (RecheckPhase)
						{
							Phase = ParentObject.GetPhase();
						}
					}
					if (RelationManager != null)
					{
						findPath2.Steps[num3].AddObject(ParentObject, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Thrown", null, Actor, null, null, null, ParentEvent);
						if (!GameObject.Validate(ParentObject) || ParentObject.CurrentCell != findPath2.Steps[num3])
						{
							return;
						}
					}
					if (projectileMovingEvent != null)
					{
						bool flag3 = false;
						projectileMovingEvent.Cell = findPath2.Steps[num3];
						projectileMovingEvent.PathIndex = num3;
						foreach (GameObject item in objectsThatWantEvent)
						{
							bool flag4 = item.HandleEvent(projectileMovingEvent);
							if (!GameObject.Validate(ParentObject) || ParentObject.CurrentCell != findPath2.Steps[num3])
							{
								return;
							}
							if (projectileMovingEvent.ActivateShowUninvolved)
							{
								showUninvolved = true;
								projectileMovingEvent.ActivateShowUninvolved = false;
							}
							if (projectileMovingEvent.RecheckPhase)
							{
								Phase = ParentObject.GetPhase();
							}
							if (projectileMovingEvent.HitOverride != null)
							{
								gameObject = projectileMovingEvent.HitOverride;
								projectileMovingEvent.HitOverride = null;
								flag3 = true;
								break;
							}
							if (!flag4)
							{
								flag3 = true;
								break;
							}
						}
						if (flag3)
						{
							i = count;
							if (findPath2 != findPath)
							{
								findPath = new FindPath(findPath2.Steps[num3], Paths[0].Steps[0], PathGlobal: false, PathUnlimited: true, ParentObject, 99, ExploredOnly: false, Juggernaut: false, IgnoreCreatures: true, IgnoreGases: true, FlexPhaseProvider?.ThrownWeaponFlexPhaseIsActive(ParentObject) ?? false);
								if (findPath.Usable)
								{
									Paths[Paths.Count - 1] = findPath;
									i--;
								}
							}
							break;
						}
					}
					if (flag && !flag2)
					{
						if (num3 == num4 && findPath2 != findPath)
						{
							scrapBuffer.Copy(scrapBuffer2);
							scrapBuffer.Goto(findPath2.Steps[num3].X, findPath2.Steps[num3].Y);
							scrapBuffer.Write("&RX");
						}
						else
						{
							scrapBuffer.Copy(scrapBuffer2);
							scrapBuffer.Goto(findPath2.Steps[num3].X, findPath2.Steps[num3].Y);
							scrapBuffer.Write(r);
						}
						scrapBuffer.Draw();
						textConsole.WaitFrame();
					}
					cell = findPath2.Steps[num3];
				}
				if (flag2 && cell != null && findPath2.Steps[0] != cell)
				{
					The.Core.RenderBase();
					CombatJuiceEntryMissileWeaponVFX entry = CombatJuice.Throw(Actor, ParentObject, findPath2.Steps[0].Location, cell.Location);
					if (i == count - 1)
					{
						CombatJuice.BlockUntilFinished((CombatJuiceEntry)entry, (IList<GameObject>)null, 1500, true);
					}
				}
				if (gameObject == null)
				{
					continue;
				}
				int num5 = Stat.Random(1, 20);
				int num6 = num2 + num5;
				int combatDV = Stats.GetCombatDV(gameObject);
				if (num6 > combatDV || num5 == 20)
				{
					if (gameObject.PhaseMatches(Phase))
					{
						gameObject.PlayWorldSound(ParentObject.GetTagOrStringProperty("ImpactSound", "Sounds/Throw/sfx_throwing_geoMagneticDisc_impact"));
						bool isCreature = gameObject.IsCreature;
						string blueprint = gameObject.Blueprint;
						WeaponUsageTracking.TrackThrownWeaponHit(Actor, ParentObject, isCreature, blueprint, Accidental: false);
						if (IComponent<GameObject>.Visible(gameObject))
						{
							gameObject.ParticleBlip("&C\u0003", 10, 0L);
						}
						int combatAV = Stats.GetCombatAV(gameObject);
						string Damage = ParentObject.GetPart<ThrownWeapon>()?.Damage ?? "2d6";
						int Penetration = combatAV;
						int PenetrationBonus = 0;
						int PenetrationModifier = combatAV;
						bool Vorpal = true;
						GetThrownWeaponPerformanceEvent.GetFor(ParentObject, ref Damage, ref Penetration, ref PenetrationBonus, ref PenetrationModifier, ref Vorpal, Prospective: false, Actor, gameObject);
						int value = Damage.GetCachedDieRoll().Resolve() + num;
						Event obj = Event.New("WeaponPseudoThrowHit");
						obj.SetParameter("Damage", value);
						obj.SetParameter("Owner", Actor);
						obj.SetParameter("Attacker", Actor);
						obj.SetParameter("Defender", gameObject);
						obj.SetParameter("Weapon", ParentObject);
						obj.SetParameter("Projectile", ParentObject);
						obj.SetParameter("ApparentTarget", gameObject);
						ParentObject.FireEvent(obj);
						value = obj.GetIntParameter("Damage");
						bool flag5 = IComponent<GameObject>.Visible(gameObject);
						GameObject gameObject2 = gameObject;
						string message = (UnidentifiedMessage ? ("from " + text + " flying into " + gameObject.them + "!") : text2);
						GameObject parentObject = ParentObject;
						int phase = Phase;
						gameObject2.TakeDamage(ref value, "Thrown PseudoThrown Vorpal Cudgel", null, null, Actor, null, parentObject, null, null, message, Accidental: false, Environmental: false, Indirect: false, showUninvolved, IgnoreVisibility: false, ShowForInanimate: true, SilentIfNoDamage: false, NoSetTarget: false, UsePopups, phase);
						WeaponUsageTracking.TrackThrownWeaponDamage(Actor, ParentObject, isCreature, blueprint, Accidental: false, value);
						if (flag5)
						{
							ParentObject.MakeUnderstood(ShowMessage: true);
						}
					}
					else if (IComponent<GameObject>.Visible(gameObject))
					{
						gameObject.ParticleBlip("&b\t", 10, 0L);
						DidXToY("pass", "through", gameObject, null, "!", null, null, gameObject);
					}
				}
				else
				{
					gameObject.ParticleBlip("&K\t", 10, 0L);
					IComponent<GameObject>.XDidY(gameObject, "flinch", "out of the way of " + text, "!", null, null, gameObject);
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GeomagneticDisc DoThrow", x);
		}
		finally
		{
			if (RelationManager != null && GameObject.Validate(ParentObject))
			{
				ParentObject.RemoveFromContext();
				RelationManager.RestoreContextRelation(ParentObject, ObjectContext, CellContext, BodyPartContext, Relation);
			}
		}
	}

	private void SignalFailure(GameObject actor)
	{
		if (actor != null && actor.IsPlayer())
		{
			Popup.ShowFail("A loud buzz is emitted. The failure glyph flashes on the side of " + ParentObject.t() + ".");
		}
	}

	private void SignalLowPower(GameObject actor)
	{
		if (actor != null && actor.IsPlayer())
		{
			Popup.ShowFail("A loud buzz is emitted. The low power glyph flashes on the side of " + ParentObject.t() + ".");
		}
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100())
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				Cell cell = ParentObject.CurrentCell ?? E.Actor.CurrentCell;
				if (cell != null)
				{
					int num2 = IComponent<GameObject>.PowerLoadBonus(num);
					int num3 = 0;
					List<Cell> list = Event.NewCellList();
					list.Add(cell);
					List<FindPath> list2 = new List<FindPath>();
					int num4 = Bounces.RollCached() + num2;
					int radius = Range.RollCached() + num2;
					List<Cell> passableConnectedAdjacentCells = cell.GetPassableConnectedAdjacentCells(radius);
					while (list2.Count <= num4 && ++num3 < 100)
					{
						Cell cell2 = null;
						cell2 = ((list2.Count != 0 || cell == E.Actor.CurrentCell) ? passableConnectedAdjacentCells.GetRandomElement() : E.Actor.CurrentCell);
						if (cell2 == null)
						{
							break;
						}
						passableConnectedAdjacentCells.Remove(cell2);
						if (!list.Contains(cell2) && (cell2 == E.Actor.CurrentCell || cell2.IsEmptyOfSolid()))
						{
							FindPath findPath = new FindPath(list.Last(), cell2, PathGlobal: false, PathUnlimited: true, ParentObject, 99, ExploredOnly: false, Juggernaut: false, IgnoreCreatures: true, IgnoreGases: true);
							if (findPath.Usable)
							{
								list.Add(cell2);
								list2.Add(findPath);
							}
						}
					}
					if (list2.Count > 0)
					{
						if (E.Actor.IsPlayer())
						{
							Popup.Show(ParentObject.T() + " suddenly" + ParentObject.GetVerb("start") + " flying around!");
						}
						DoThrow(E.Actor, list2, UnidentifiedMessage: true, LastTarget: E.Actor, UsePopups: E.Actor.IsPlayer(), Targets: null, Phase: 0, Bonus: num2, FlexPhaseProvider: null, ParentEvent: E);
						E.Identify = true;
						return true;
					}
				}
			}
		}
		return false;
	}
}
