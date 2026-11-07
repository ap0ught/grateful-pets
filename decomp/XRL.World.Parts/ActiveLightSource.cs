using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, light radius is increased by the standard
/// power load bonus, i.e. 2 for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class ActiveLightSource : IActivePart, ILightSource
{
	public bool Darkvision;

	public int Radius = 5;

	public bool ShowInShortDescription = true;

	public ActiveLightSource()
	{
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		ActiveLightSource activeLightSource = p as ActiveLightSource;
		if (activeLightSource.Darkvision != Darkvision)
		{
			return false;
		}
		if (activeLightSource.Radius != Radius)
		{
			return false;
		}
		if (activeLightSource.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		RadiateLight();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			List<string> list = new List<string>();
			bool flag = false;
			if (!WorksOnSelf)
			{
				if (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnImplantee)
				{
					list.Add("equipped");
				}
				else
				{
					flag = true;
				}
			}
			if (ChargeUse > 0 || ChargeMinimum > 0)
			{
				list.Add("powered");
			}
			if (flag || !string.IsNullOrEmpty(NeedsOtherActivePartOperational) || !string.IsNullOrEmpty(NeedsOtherActivePartEngaged))
			{
				list.Add("in use");
			}
			stringBuilder.Append((list.Count > 0) ? "When" : "While").Append(' ').Append((list.Count > 0) ? Grammar.MakeAndList(list) : "operational")
				.Append(", provides ")
				.Append(Darkvision ? "night vision" : "light");
			if (GetEffectiveRadius() != 9999)
			{
				stringBuilder.Append(" in radius ").Append(GetEffectiveRadius()).Append('.');
			}
			E.Postfix.AppendRules(stringBuilder.ToString());
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount, null, UseChargeIfUnpowered: false, 0, NeedStatusUpdate: true);
		}
	}

	public int GetEffectiveRadius()
	{
		if (Radius == 9999)
		{
			return Radius;
		}
		return Radius + MyPowerLoadBonus();
	}

	public bool RadiateLight()
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return false;
		}
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		int effectiveRadius = GetEffectiveRadius();
		if (Darkvision)
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null && equipped.IsPlayer())
			{
				if (effectiveRadius == 9999)
				{
					cell.ParentZone.LightAll(LightLevel.Darkvision);
				}
				else
				{
					cell.ParentZone.AddLight(cell.X, cell.Y, effectiveRadius, LightLevel.Darkvision);
				}
			}
		}
		else if (effectiveRadius == 9999)
		{
			cell.ParentZone.LightAll();
		}
		else
		{
			cell.ParentZone.AddLight(cell.X, cell.Y, effectiveRadius, LightLevel.Light);
		}
		return true;
	}

	int ILightSource.GetRadius()
	{
		return Radius;
	}

	bool ILightSource.IsActive()
	{
		return IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
	}

	bool ILightSource.IsDarkvision()
	{
		return Darkvision;
	}
}
