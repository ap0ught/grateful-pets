using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class EjectionSeat : IActivePart
{
	public const string ABL_CMD = "CommandSeatEject";

	public const string INV_CMD = "CommandSingleSeatEject";

	public string Sound;

	public int MinRange = 3;

	public int MaxRange = 8;

	public bool CanEject
	{
		get
		{
			if (GetSlot() != null)
			{
				return GetSitter() != null;
			}
			return false;
		}
	}

	public bool WithinInterior => ParentObject.CurrentZone is InteriorZone;

	public EjectionSeat()
	{
		WorksOnSelf = true;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSeatEject")
		{
			return !Eject(AllInZone: true);
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeInteriorCollapseEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetKineticResistanceEvent>.ID && ID != InventoryActionEvent.ID)
		{
			return ID == PooledEvent<AfterPlayerBodyChangeEvent>.ID;
		}
		return true;
	}

	public GameObject GetSlot()
	{
		return ParentObject.CurrentCell?.GetFirstObject(IsOurSlot);
	}

	public GameObject GetSitter()
	{
		return ParentObject.CurrentCell?.GetFirstObject(IsOurSitter);
	}

	public bool IsOurSlot(GameObject Object)
	{
		return Object.HasPart(typeof(EjectionSlot));
	}

	public bool IsOurSitter(GameObject Object)
	{
		return Object.HasEffect(IsOurEffect);
	}

	public bool IsOurEffect(Effect FX)
	{
		if (FX is Sitting sitting)
		{
			return sitting.SittingOn == ParentObject;
		}
		return false;
	}

	public override bool HandleEvent(GetKineticResistanceEvent E)
	{
		if (!ParentObject.Physics.Takeable && GetSlot() != null)
		{
			E.LinearIncrease += 800;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (CanEject)
		{
			E.AddAction("Eject", "eject", "CommandSingleSeatEject", null, 'j');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "CommandSingleSeatEject")
		{
			E.RequestInterfaceExit();
			GameManager.Instance.gameQueue.queueTask(() =>
			{
				_ = ParentObject.CurrentZone;
				Eject();
			});
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeInteriorCollapseEvent E)
	{
		Eject(AllInZone: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		if (ParentObject.CurrentZone is InteriorZone interiorZone && interiorZone.ParentObject == E.NewBody && E.OldBody.InSameCellAs(ParentObject) && IsOurSitter(E.OldBody))
		{
			ActivatedAbilities activatedAbilities = E.NewBody.RequirePart<ActivatedAbilities>();
			ActivatedAbilityEntry activatedAbilityEntry = activatedAbilities.GetAbilityByCommand("CommandSeatEject");
			if (activatedAbilityEntry == null)
			{
				Guid iD = activatedAbilities.AddAbility("Eject", "CommandSeatEject", E.NewBody.HasPart(typeof(Vehicle)) ? "Vehicle" : "Maneuvers", null, "ø", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: true);
				activatedAbilityEntry = activatedAbilities.GetAbility(iD);
			}
			if (CanEject)
			{
				activatedAbilityEntry.Enabled = true;
				activatedAbilityEntry.DisabledMessage = null;
			}
			else
			{
				activatedAbilityEntry.Enabled = false;
				activatedAbilityEntry.DisabledMessage = "Your seat is unable to eject.";
			}
			E.NewBody.RegisterPartEvent(this, "CommandSeatEject");
		}
		return base.HandleEvent(E);
	}

	public bool TryGetEjectionSeats(bool AllInZone, out List<EjectionSeat> Seats, out List<GameObject> Sitters)
	{
		Seats = new List<EjectionSeat>();
		Sitters = new List<GameObject>();
		if (AllInZone)
		{
			foreach (GameObject item in ParentObject.CurrentZone.YieldObjects())
			{
				if (item.TryGetPart<EjectionSeat>(out var Part) && Part.CanEject)
				{
					Seats.Add(Part);
					Sitters.Add(Part.GetSitter());
				}
			}
		}
		else if (CanEject)
		{
			Seats.Add(this);
			Sitters.Add(GetSitter());
		}
		return !Seats.IsNullOrEmpty();
	}

	public List<Cell> GetTargetsFor(List<EjectionSeat> Seats, List<GameObject> Sitters)
	{
		List<Cell> list = new List<Cell>();
		BallBag<Cell> ballBag = new BallBag<Cell>();
		for (int i = 0; i < Seats.Count; i++)
		{
			ballBag.Clear();
			EjectionSeat ejectionSeat = Seats[i];
			GameObject gameObject = Sitters[i];
			Cell ejectionOrigin = ejectionSeat.GetEjectionOrigin();
			foreach (Cell item in ejectionOrigin.YieldAdjacentCells(MaxRange, LocalOnly: true))
			{
				if (ejectionOrigin.PathDistanceTo(item) >= MinRange && item.IsPassable(ejectionSeat.ParentObject) && item.IsPassable(gameObject) && !list.Contains(item))
				{
					int navigationWeightFor = item.GetNavigationWeightFor(gameObject);
					int weight = 300;
					if (navigationWeightFor > 30)
					{
						weight = 1;
					}
					else if (navigationWeightFor > 10)
					{
						weight = 10;
					}
					else if (navigationWeightFor > 1)
					{
						weight = 100;
					}
					else if (navigationWeightFor == 1)
					{
						weight = 200;
					}
					ballBag.Add(item, weight);
				}
			}
			list.Add(ballBag.PeekOne());
		}
		return list;
	}

	public Cell GetEjectionOrigin()
	{
		return (ParentObject.CurrentZone as InteriorZone)?.ParentCell ?? ParentObject.CurrentCell;
	}

	public GameObject GetEjectionSource()
	{
		return (ParentObject.CurrentZone as InteriorZone)?.ParentObject ?? ParentObject;
	}

	public void PlayAnimation(List<EjectionSeat> Seats, List<GameObject> Sitters, List<Cell> Targets)
	{
		CombatJuiceSynchronizedEntry combatJuiceSynchronizedEntry = new CombatJuiceSynchronizedEntry();
		for (int i = 0; i < Sitters.Count; i++)
		{
			Cell cell = Sitters[i].CurrentCell;
			Cell ejectionOrigin = Seats[i].GetEjectionOrigin();
			if (cell.IsVisible() || ejectionOrigin.IsVisible())
			{
				combatJuiceSynchronizedEntry.Entries.Add(CombatJuice.Jump(Sitters[i], ejectionOrigin.Location, Targets[i].Location, Stat.Random(0.95f, 1.15f), Stat.Random(1.8f, 2.2f), 0.35f, Focus: false, Enqueue: false));
			}
		}
		if (combatJuiceSynchronizedEntry.Entries.IsNullOrEmpty())
		{
			return;
		}
		Zone zone = GetEjectionOrigin().ParentZone;
		bool hide = ParentObject.IsInActiveZone();
		CombatJuiceManager.enqueueEntry(combatJuiceSynchronizedEntry);
		CombatJuice.BlockUntilFinished(combatJuiceSynchronizedEntry, () =>
		{
			ScreenBuffer scrapBuffer = TextConsole.GetScrapBuffer1();
			zone.Render(scrapBuffer);
			XRLCore.ParticleManager.Frame();
			XRLCore.ParticleManager.Render(scrapBuffer);
			if (hide)
			{
				foreach (EjectionSeat Seat in Seats)
				{
					Seat.HidePlayer(scrapBuffer);
				}
			}
			scrapBuffer.Draw();
		});
	}

	public bool Eject(bool AllInZone = false)
	{
		if (ParentObject.CurrentZone is InteriorZone interiorZone && interiorZone.ParentZone.IsWorldMap())
		{
			return false;
		}
		if (!TryGetEjectionSeats(AllInZone, out var Seats, out var Sitters))
		{
			return false;
		}
		List<Cell> targetsFor = GetTargetsFor(Seats, Sitters);
		for (int num = Seats.Count - 1; num >= 0; num--)
		{
			if (targetsFor[num] == null || Seats[num].IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				Seats.RemoveAt(num);
				Sitters.RemoveAt(num);
				targetsFor.RemoveAt(num);
			}
			else if (!Seats[num].WithinInterior)
			{
				Blast(Seats[num].currentCell);
				Seats[num].PlayWorldSound(Sound);
			}
		}
		if (Seats.IsNullOrEmpty())
		{
			return false;
		}
		GameObject ejectionSource = GetEjectionSource();
		if (WithinInterior)
		{
			Cell ejectionOrigin = GetEjectionOrigin();
			Blast(ejectionOrigin);
			ejectionOrigin.PlayWorldSound(Sound);
		}
		if (Options.UseOverlayCombatEffects)
		{
			PlayAnimation(Seats, Sitters, targetsFor);
		}
		for (int i = 0; i < Seats.Count; i++)
		{
			GameObject parentObject = Seats[i].ParentObject;
			parentObject.SystemMoveTo(targetsFor[i]);
			parentObject.Physics.Takeable = true;
		}
		Message(ejectionSource, Sitters);
		Puff(targetsFor);
		The.Core.RenderBase();
		return true;
	}

	public void Message(GameObject Source, List<GameObject> Sitters)
	{
		bool flag = Sitters.Any((GameObject x) => x.IsPlayer());
		if (!(Source.IsVisible() | flag))
		{
			return;
		}
		foreach (GameObject Sitter in Sitters)
		{
			Sitter.Physics.DidXToY("eject", (Source == ParentObject) ? "with" : "from", Source, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, Source, !flag, DescribeSubjectDirectionLate: false, AlwaysVisible: true);
		}
	}

	public static void Blast(Cell Cell, int Count = 7, int Life = 25)
	{
		Blast(Cell.X, Cell.Y, Count, Life);
	}

	public static void Blast(int X, int Y, int Count = 7, int Life = 25)
	{
		string[] list = new string[3] { "&r", "&R", "&W" };
		for (int i = 0; i < Count; i++)
		{
			float num = (float)Stat.RandomCosmetic(0, 359) / 58f;
			float xDel = (float)Math.Sin(num) / ((float)Life / 3f);
			float yDel = (float)Math.Cos(num) / ((float)Life / 3f);
			string text = list.GetRandomElementCosmetic() + (char)(219 + Stat.RandomCosmetic(0, 4));
			XRLCore.ParticleManager.Add(text, X, Y, xDel, yDel, Life, 0f, 0f, 0L);
		}
	}

	public static void Puff(IEnumerable<Cell> Cells, int Count = 10, int Life = 12)
	{
		foreach (Cell Cell in Cells)
		{
			Puff(Cell, Count, Life);
		}
	}

	public static void Puff(Cell Cell, int Count = 10, int Life = 12)
	{
		if (Cell.IsVisible())
		{
			Puff(Cell.X, Cell.Y, Count, Life);
		}
	}

	public static void Puff(int X, int Y, int Count = 10, int Life = 12)
	{
		for (int i = 0; i < Count; i++)
		{
			float f = (float)Stat.Rnd2.Next(360) * (MathF.PI / 180f);
			float xDel = Mathf.Sin(f) / ((float)Life / 3f);
			float yDel = Mathf.Cos(f) / ((float)Life / 3f);
			string text = ((Stat.RandomCosmetic(1, 4) <= 3) ? "&y." : "&y±");
			XRLCore.ParticleManager.Add(text, X, Y, xDel, yDel, Life, 0f, 0f, 0L);
		}
	}

	public bool IsTertiary(GameObject Object)
	{
		if (!Object.IsPlayer())
		{
			return Object != ParentObject;
		}
		return false;
	}

	public void HidePlayer(ScreenBuffer SB)
	{
		Cell cell = ParentObject.CurrentCell;
		IRenderable renderable = cell.GetHighestRenderLayerObject(IsTertiary)?.Render;
		if (renderable == null)
		{
			renderable = cell;
		}
		SB.Goto(cell.X, cell.Y);
		SB.Write(renderable);
	}
}
