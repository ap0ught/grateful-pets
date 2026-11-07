using System;
using System.Text;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsBiodynamicPowerPlant : IPoweredPart
{
	public int ChargeRate = 5000;

	public bool ConsiderLive = true;

	public int Charge;

	public int LastCharge;

	public CyberneticsBiodynamicPowerPlant()
	{
		WorksOnImplantee = true;
		IsBootSensitive = false;
		ChargeUse = 0;
		NameForStatus = "BiodynamicPowerPlant";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != EffectAppliedEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "HasPowerConnectors");
		E.Implantee.RegisterPartEvent(this, "OwnerGetShortDescription");
		E.Implantee.RegisterPartEvent(this, "QueryCharge");
		E.Implantee.RegisterPartEvent(this, "QueryChargeProduction");
		E.Implantee.RegisterPartEvent(this, "TestCharge");
		E.Implantee.RegisterPartEvent(this, "UseCharge");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "HasPowerConnectors");
		E.Implantee.UnregisterPartEvent(this, "OwnerGetShortDescription");
		E.Implantee.UnregisterPartEvent(this, "QueryCharge");
		E.Implantee.UnregisterPartEvent(this, "QueryChargeProduction");
		E.Implantee.UnregisterPartEvent(this, "TestCharge");
		E.Implantee.UnregisterPartEvent(this, "UseCharge");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (E.Object == ParentObject.Implantee)
		{
			LastCharge = Charge;
			Charge = 0;
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				AddCharge(ChargeRate);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Current charge", Charge);
		E.AddEntry(this, "Charge left after last turn", LastCharge);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Charge = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "HasPowerConnectors")
		{
			return false;
		}
		if (E.ID == "QueryCharge")
		{
			if ((ConsiderLive || !E.HasFlag("LiveOnly")) && E.HasFlag("IncludeTransient") && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				int num = GetCharge() - ChargeUse;
				if (num > 0)
				{
					E.SetParameter("Charge", E.GetIntParameter("Charge") + num);
				}
			}
		}
		else if (E.ID == "TestCharge")
		{
			if ((ConsiderLive || !E.HasFlag("LiveOnly")) && E.HasFlag("IncludeTransient") && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				int intParameter = E.GetIntParameter("Charge");
				int num2 = Math.Min(intParameter, GetCharge() - ChargeUse);
				if (num2 > 0)
				{
					E.SetParameter("Charge", intParameter - num2);
					if (num2 >= intParameter)
					{
						return false;
					}
				}
			}
		}
		else if (E.ID == "UseCharge")
		{
			if ((ConsiderLive || !E.HasFlag("LiveOnly")) && E.HasFlag("IncludeTransient") && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				int intParameter2 = E.GetIntParameter("Charge");
				int num3 = Math.Min(intParameter2, GetCharge() - ChargeUse);
				if (num3 > 0)
				{
					E.SetParameter("Charge", intParameter2 - num3);
					UseCharge(num3 + ChargeUse);
					if (num3 >= intParameter2)
					{
						return false;
					}
				}
			}
		}
		else if (E.ID == "OwnerGetShortDescription")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			IntegratedPowerSystems part = gameObjectParameter.GetPart<IntegratedPowerSystems>();
			if (part != null && part.RequiresEvent == "HasPowerConnectors" && !gameObjectParameter.HasPart<ModJacked>())
			{
				string text;
				if (gameObjectParameter.UseBareIndicative)
				{
					text = (gameObjectParameter.IsPlural ? "these devices" : "this device");
				}
				else
				{
					text = gameObjectParameter.it;
				}
				if (E.GetParameter("PostfixBuilder") is StringBuilder sB)
				{
					sB.AppendRules("Integrated power systems: When equipped, " + text + " can be powered by " + (ParentObject.HasProperName ? ParentObject.DisplayNameOnlyStripped : ("your " + ParentObject.DisplayNameOnlyDirectAndStripped)) + ".");
				}
			}
		}
		else if (E.ID == "QueryChargeProduction" && (ConsiderLive || !E.HasFlag("LiveOnly")) && E.HasFlag("IncludeTransient") && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.SetParameter("Charge", E.GetIntParameter("Charge") + ChargeRate);
		}
		return base.FireEvent(E);
	}

	public bool HasCharge(int Amount)
	{
		return Charge >= Amount;
	}

	public int GetCharge()
	{
		return Charge;
	}

	public void UseCharge(int Amount)
	{
		Charge -= Amount;
		if (Charge < 0)
		{
			Charge = 0;
		}
	}

	public void AddCharge(int Amount)
	{
		Charge += Amount;
		if (Charge < 0)
		{
			Charge = 0;
		}
	}

	public int GetChargeLevel()
	{
		return EnergyStorage.GetChargeLevel(GetCharge(), ChargeRate);
	}
}
