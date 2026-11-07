using System;
using System.Collections.Generic;
using ConsoleLib.Console;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the item's effective pull force is increased
/// by a percentage equal to ((power load - 100) / 10), i.e. 30% for
/// the standard overload power load of 400.
/// </remarks>
[Serializable]
public class GrapplingGun : IPoweredPart
{
	public int Force = 4500;

	public int MaxPenetrations = 1;

	public string Color;

	public GrapplingGun()
	{
		ChargeUse = 20;
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		GrapplingGun grapplingGun = p as GrapplingGun;
		if (grapplingGun.Force != Force)
		{
			return false;
		}
		if (grapplingGun.MaxPenetrations != MaxPenetrations)
		{
			return false;
		}
		if (grapplingGun.Color != Color)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNamePenetrationColorEvent>.ID)
		{
			return ID == PooledEvent<MissilePenetrateEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNamePenetrationColorEvent E)
	{
		if (MaxPenetrations <= 1)
		{
			E.Color = "K";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MissilePenetrateEvent E)
	{
		if (E.Penetrations <= 0)
		{
			E.OutcomeMessageFragment = ", but" + (E.Projectile?.GetVerb("manage") ?? "manages") + " to latch on anyway";
		}
		else if (MaxPenetrations > 0 && E.Penetrations > MaxPenetrations)
		{
			E.Penetrations = MaxPenetrations;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LauncherProjectileHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LauncherProjectileHit")
		{
			PerformGrapple(E.GetGameObjectParameter("Attacker"), E.GetGameObjectParameter("Defender"));
		}
		return base.FireEvent(E);
	}

	public bool PerformGrapple(GameObject Actor, GameObject Target)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		int num3 = MyPowerLoadLevel();
		int num4 = Force;
		int num5 = IComponent<GameObject>.PowerLoadBonus(num3, 100, 10);
		if (num5 != 0)
		{
			num4 = num4 * (100 + num5) / 100;
		}
		Actor.PlayWorldSound("Sounds/Missile/Special/sfx_missile_grapplingGun_pull");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		int num6 = 0;
		int num7 = 0;
		while (GameObject.Validate(ref Actor) && GameObject.Validate(ref Target) && Actor != Target && !Target.InSameOrAdjacentCellTo(Actor) && ++num6 < 100)
		{
			The.Core.RenderBaseToBuffer(scrapBuffer);
			List<Tuple<Cell, char>> lineTo = Actor.GetLineTo(Target);
			string text = null;
			int i = 1;
			for (int num8 = lineTo.Count - 2; i <= num8; i++)
			{
				int x = lineTo[i].Item1.X;
				int y = lineTo[i].Item1.Y;
				int x2 = lineTo[i - 1].Item1.X;
				int y2 = lineTo[i - 1].Item1.Y;
				int x3 = lineTo[i + 1].Item1.X;
				int y3 = lineTo[i + 1].Item1.Y;
				if (y == y3 && y == y2)
				{
					text = "-";
				}
				else if (x == x3 && x == x2)
				{
					text = "|";
				}
				else if ((x == x3 && x != x2 && y != y3 && y == x2) || (x != x3 && x == x2 && y == y3 && y != x2))
				{
					text = null;
				}
				else if (y3 > y2)
				{
					text = ((x3 > x2) ? "\\" : "/");
				}
				else if (y3 < y2)
				{
					text = ((x3 > x2) ? "/" : "\\");
				}
				if (!text.IsNullOrEmpty())
				{
					if (!Color.IsNullOrEmpty())
					{
						text = "{{" + Color + "|" + text + "}}";
					}
					scrapBuffer.WriteAt(x, y, text);
				}
			}
			scrapBuffer.Draw();
			The.ParticleManager.Frame();
			int kineticResistance = Actor.GetKineticResistance();
			int num9 = (Target.CanBeInvoluntarilyMoved() ? Target.GetKineticResistance() : int.MaxValue);
			int num10 = kineticResistance - num;
			int num11 = num9 - num2;
			if (num10 > num4 && num11 > num4)
			{
				break;
			}
			int? powerLoadLevel = num3;
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				break;
			}
			int num12 = Math.Min(Math.Min(num10, num11), num4);
			num = Math.Min(num + num12, num4);
			num2 = Math.Min(num2 + num12, num4);
			int num13 = num - kineticResistance;
			int num14 = num2 - num9;
			Cell cell = Actor.CurrentCell;
			Cell cell2 = Target.CurrentCell;
			if (num13 > num14)
			{
				string generalDirectionFromCell = cell.GetGeneralDirectionFromCell(cell2);
				Cell cellFromDirection = cell.GetCellFromDirection(generalDirectionFromCell);
				if (cellFromDirection == null || cellFromDirection == cell)
				{
					break;
				}
				int num15 = cell.DistanceTo(cell2);
				if (!Actor.Move(generalDirectionFromCell, Forced: true, System: false, IgnoreGravity: true, NoStack: false, AllowDashing: false, DoConfirmations: false, ParentObject, EnergyCost: 0, Actor: Actor, NearestAvailable: true, Type: "Grapple", MoveSpeed: null, Peaceful: true))
				{
					break;
				}
				flag = true;
				num -= kineticResistance;
				if (Actor.DistanceTo(cell2) >= num15 && ++num7 >= 3)
				{
					break;
				}
			}
			else
			{
				string generalDirectionFromCell2 = cell2.GetGeneralDirectionFromCell(cell);
				Cell cellFromDirection2 = cell2.GetCellFromDirection(generalDirectionFromCell2);
				if (cellFromDirection2 == null || cellFromDirection2 == cell2)
				{
					break;
				}
				int num16 = cell.DistanceTo(cell2);
				if (!Target.Move(generalDirectionFromCell2, Forced: true, System: false, IgnoreGravity: true, NoStack: false, AllowDashing: false, DoConfirmations: false, ParentObject, EnergyCost: 0, Actor: Actor, NearestAvailable: true, Type: "Grapple", MoveSpeed: null, Peaceful: true))
				{
					break;
				}
				flag2 = true;
				num2 -= num9;
				if (cell.DistanceTo(Target) >= num16 && ++num7 >= 3)
				{
					break;
				}
			}
			if (cell.DistanceTo(Actor.CurrentCell) > 1 || cell2.DistanceTo(Target.CurrentCell) > 1)
			{
				break;
			}
		}
		if (flag && GameObject.Validate(ref Actor))
		{
			Actor.Gravitate();
		}
		if (flag2 && GameObject.Validate(ref Target))
		{
			Target.Gravitate();
		}
		return flag | flag2;
	}
}
