using System;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Toolbox : IPoweredPart
{
	public int PoweredDisassembleBonus;

	public int UnpoweredDisassembleBonus;

	public int PoweredInspectBonus;

	public int UnpoweredInspectBonus;

	public int PoweredReverseEngineerBonus;

	public int UnpoweredReverseEngineerBonus;

	public int PoweredBonusModBonus;

	public int UnpoweredBonusModBonus;

	public bool TrackAsToolbox = true;

	public string BehaviorDescription;

	public float ComputePowerFactor;

	public Toolbox()
	{
		ChargeUse = 0;
		WorksOnEquipper = true;
		WorksOnCarrier = true;
		NameForStatus = "ToolSystems";
	}

	public override bool SameAs(IPart p)
	{
		Toolbox toolbox = p as Toolbox;
		if (toolbox.PoweredDisassembleBonus != PoweredDisassembleBonus)
		{
			return false;
		}
		if (toolbox.UnpoweredDisassembleBonus != UnpoweredDisassembleBonus)
		{
			return false;
		}
		if (toolbox.PoweredInspectBonus != PoweredInspectBonus)
		{
			return false;
		}
		if (toolbox.UnpoweredInspectBonus != UnpoweredInspectBonus)
		{
			return false;
		}
		if (toolbox.PoweredReverseEngineerBonus != PoweredReverseEngineerBonus)
		{
			return false;
		}
		if (toolbox.UnpoweredReverseEngineerBonus != UnpoweredReverseEngineerBonus)
		{
			return false;
		}
		if (toolbox.PoweredBonusModBonus != PoweredBonusModBonus)
		{
			return false;
		}
		if (toolbox.UnpoweredBonusModBonus != UnpoweredBonusModBonus)
		{
			return false;
		}
		if (toolbox.TrackAsToolbox != TrackAsToolbox)
		{
			return false;
		}
		if (toolbox.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		if (toolbox.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && (ID != GetShortDescriptionEvent.ID || string.IsNullOrEmpty(BehaviorDescription)))
		{
			return ID == GetTinkeringBonusEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "PoweredDisassembleBonus", PoweredDisassembleBonus);
		E.AddEntry(this, "UnpoweredDisassembleBonus", UnpoweredDisassembleBonus);
		E.AddEntry(this, "PoweredInspectBonus", PoweredInspectBonus);
		E.AddEntry(this, "UnpoweredInspectBonus", UnpoweredInspectBonus);
		E.AddEntry(this, "PoweredReverseEngineerBonus", PoweredReverseEngineerBonus);
		E.AddEntry(this, "UnpoweredReverseEngineerBonus", UnpoweredReverseEngineerBonus);
		E.AddEntry(this, "PoweredBonusModBonus", PoweredBonusModBonus);
		E.AddEntry(this, "UnpoweredBonusModBonus", UnpoweredBonusModBonus);
		E.AddEntry(this, "TrackAsToolbox", TrackAsToolbox);
		E.AddEntry(this, "BehaviorDescription", BehaviorDescription);
		E.AddEntry(this, "ComputePowerFactor", ComputePowerFactor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		switch (E.Type)
		{
		case "Disassemble":
			if (!HandleBonus(E, PoweredDisassembleBonus, UnpoweredDisassembleBonus))
			{
				return false;
			}
			break;
		case "Inspect":
			if (!HandleBonus(E, PoweredInspectBonus, UnpoweredInspectBonus))
			{
				return false;
			}
			break;
		case "ReverseEngineer":
			if (!HandleBonus(E, PoweredReverseEngineerBonus, UnpoweredReverseEngineerBonus))
			{
				return false;
			}
			break;
		case "BonusMod":
			if (!HandleBonus(E, PoweredBonusModBonus, UnpoweredBonusModBonus))
			{
				return false;
			}
			break;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription, GetEventSensitiveAddStatusSummary(E));
		}
		if (ComputePowerFactor > 0f)
		{
			E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		}
		else if (ComputePowerFactor < 0f)
		{
			E.Postfix.AppendRules("Compute power on the local lattice decreases this item's effectiveness.");
		}
		return base.HandleEvent(E);
	}

	private bool HandleBonus(GetTinkeringBonusEvent E, int PoweredBonus, int UnpoweredBonus)
	{
		if (PoweredBonus == 0 && UnpoweredBonus == 0)
		{
			return true;
		}
		if (ChargeUse > 0 && PoweredBonus != UnpoweredBonus)
		{
			if ((!TrackAsToolbox || ((PoweredBonus >= 0) ? (E.ToolboxBonus < PoweredBonus) : (E.ToolboxBonus > PoweredBonus))) && IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: true, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && IsObjectActivePartSubject(E.Actor))
			{
				ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, ParentObject.Count, null, UseChargeIfUnpowered: false, 0L);
				if (activePartStatus == ActivePartStatus.Operational)
				{
					ApplyBonus(E, PoweredBonus);
					if (E.Actor.IsPlayer())
					{
						ParentObject.RemoveIntProperty("ToolboxUseWhenInoperative");
					}
				}
				else
				{
					if (E.Interruptable && E.Actor.IsPlayer() && ParentObject.GetIntProperty("ToolboxUseWhenInoperative") == 0)
					{
						string text = activePartStatus switch
						{
							ActivePartStatus.Unpowered => "unpowered", 
							ActivePartStatus.Booting => "still starting up", 
							_ => "inoperative", 
						};
						string text2;
						if (UnpoweredBonus <= 0)
						{
							text2 = " without " + ParentObject.its + " benefits";
						}
						else
						{
							text2 = ((activePartStatus != ActivePartStatus.Unpowered) ? (" without " + ParentObject.its + " full benefits") : (", using " + ParentObject.them + " without power"));
						}
						if (Popup.ShowYesNo(ColorUtility.CapitalizeExceptFormatting((ParentObject.HasProperName ? ParentObject.The : "Your ") + ParentObject.DisplayNameOnly) + ParentObject.Is + " " + text + ". Do you want to continue" + text2 + "?") != DialogResult.Yes)
						{
							AutoAct.Interrupt();
							return false;
						}
						ParentObject.SetIntProperty("ToolboxUseWhenInoperative", 1);
					}
					ApplyBonus(E, UnpoweredBonus);
				}
			}
		}
		else if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: true, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && IsObjectActivePartSubject(E.Actor))
		{
			ApplyBonus(E, UnpoweredBonus);
		}
		return true;
	}

	private void ApplyBonus(GetTinkeringBonusEvent E, int Bonus)
	{
		int num = GetAvailableComputePowerEvent.AdjustUp(this, Bonus, ComputePowerFactor);
		if (!TrackAsToolbox)
		{
			E.Bonus += num * ParentObject.Count;
		}
		else if (num > 0)
		{
			int num2 = ((E.ToolboxBonus > 0) ? (num - E.ToolboxBonus) : num);
			if (num2 > 0)
			{
				E.Bonus += num2;
				E.ToolboxBonus += num2;
			}
		}
		else if (num < 0)
		{
			int num3 = ((E.ToolboxBonus < 0) ? (num - E.ToolboxBonus) : num);
			if (num3 < 0)
			{
				E.Bonus += num3;
				E.ToolboxBonus += num3;
			}
		}
	}
}
