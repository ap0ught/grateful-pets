using System;

namespace XRL.World.Parts;

[Serializable]
public class EmergencyTeleporter : IPoweredPart
{
	public int Chance = 100;

	public EmergencyTeleporter()
	{
		WorksOnEquipper = true;
		ChargeUse = 100;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsTechScannable = true;
	}

	public EmergencyTeleporter(int Chance)
		: this()
	{
		this.Chance = Chance;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("travel", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Equipped");
		Registrar.Register("Unequipped");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects())
				{
					if (!activePartSubject.OnWorldMap() && IComponent<GameObject>.CheckRealityDistortionUsability(activePartSubject, null, null, ParentObject) && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
					{
						activePartSubject.RandomTeleport(Swirl: true);
					}
				}
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "BeforeApplyDamage");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "BeforeApplyDamage");
		}
		return base.FireEvent(E);
	}
}
