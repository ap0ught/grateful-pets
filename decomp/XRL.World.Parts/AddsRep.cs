using System;
using System.Collections.Generic;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class AddsRep : IActivePart
{
	public string Faction = "";

	public int Value;

	public bool AppliedBonus;

	public AddsRep()
	{
		WorksOnEquipper = true;
	}

	public AddsRep(string Faction)
		: this()
	{
		if (!Factions.Exists(Faction))
		{
			if (Faction.Contains(','))
			{
				List<string> list = Faction.CachedCommaExpansion();
				bool flag = false;
				foreach (string item in list)
				{
					if (!Factions.Exists(item))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list = new List<string>(list.ToArray());
					bool flag2 = false;
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						string text = list[i];
						if (!Factions.Exists(text))
						{
							string newFaction = CompatManager.GetNewFaction(text);
							if (newFaction != null)
							{
								MetricsManager.LogWarning("faction name " + Faction + " should be " + newFaction + ", support will be removed after Q2 2024");
								list[i] = newFaction;
								flag2 = true;
							}
						}
					}
					if (flag2)
					{
						Faction = string.Join(",", list.ToArray());
					}
				}
			}
			else
			{
				string newFaction2 = CompatManager.GetNewFaction(Faction);
				if (newFaction2 != null)
				{
					MetricsManager.LogWarning("faction name " + Faction + " should be " + newFaction2 + ", support will be removed after Q2 2024");
					Faction = newFaction2;
				}
			}
		}
		this.Faction = Faction;
	}

	public AddsRep(string Faction, int Value)
		: this(Faction)
	{
		this.Value = Value;
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		if (Factions.Exists(Faction))
		{
			return;
		}
		if (Faction.Contains(','))
		{
			List<string> list = Faction.CachedCommaExpansion();
			bool flag = false;
			foreach (string item in list)
			{
				if (!Factions.Exists(item))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			list = new List<string>(list.ToArray());
			bool flag2 = false;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				string text = list[i];
				if (Factions.Exists(text))
				{
					continue;
				}
				string newFaction = CompatManager.GetNewFaction(text);
				if (newFaction != null)
				{
					if (Reader.FileVersion >= 350)
					{
						MetricsManager.LogWarning("faction name " + Faction + " should be " + newFaction + ", support will be removed after Q2 2024");
					}
					list[i] = newFaction;
					flag2 = true;
				}
				else
				{
					Faction byDisplayName = Factions.GetByDisplayName(Faction);
					if (byDisplayName != null)
					{
						list[i] = byDisplayName.Name;
						flag2 = true;
					}
				}
			}
			if (flag2)
			{
				Faction = string.Join(",", list.ToArray());
			}
			return;
		}
		string newFaction2 = CompatManager.GetNewFaction(Faction);
		if (newFaction2 != null)
		{
			if (Reader.FileVersion >= 350)
			{
				MetricsManager.LogWarning("faction name " + Faction + " should be " + newFaction2 + ", support will be removed after Q2 2024");
			}
			Faction = newFaction2;
		}
		else
		{
			Faction byDisplayName2 = Factions.GetByDisplayName(Faction);
			if (byDisplayName2 != null)
			{
				Faction = byDisplayName2.Name;
			}
		}
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		AddsRep obj = base.DeepCopy(Parent, MapInv) as AddsRep;
		obj.AppliedBonus = false;
		return obj;
	}

	public override bool SameAs(IPart Part)
	{
		AddsRep addsRep = Part as AddsRep;
		if (addsRep.Faction != Faction)
		{
			return false;
		}
		if (addsRep.Value != Value)
		{
			return false;
		}
		if (addsRep.AppliedBonus != AppliedBonus)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckApplyBonus(null, UseCharge: true, Amount);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID && ID != PowerSwitchFlippedEvent.ID && ID != UnequippedEvent.ID && ID != PooledEvent<AfterPlayerBodyChangeEvent>.ID && (!WorksOnCarrier || ID != TakenEvent.ID))
		{
			if (WorksOnCarrier)
			{
				return ID == DroppedEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ParentObject.IsTakeable())
		{
			string prefix = ((IsPowerSwitchSensitive && ParentObject.HasPart<PowerSwitch>()) ? "When activated, " : "");
			string statusSummary = GetStatusSummary();
			AppendDescription(Postfix: (statusSummary != null) ? (" (" + statusSummary + ")") : "", SB: E.Postfix, Faction: Faction, Value: Value, Prefix: prefix, Rules: true);
		}
		return base.HandleEvent(E);
	}

	public static void AppendDescription(StringBuilder SB, string Faction, int Value, string Prefix = null, string Postfix = null, bool Rules = false, bool SignedRules = false)
	{
		DelimitedEnumeratorChar enumerator = Faction.DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			int value = Value;
			string value2 = "";
			current.Split(':', out var First, out var Second, out var Third);
			if (Third.Length <= 0 || !Third.SequenceEqual("hidden"))
			{
				if (Second.Length > 0 && int.TryParse(Second, out var result))
				{
					value = result;
				}
				if (First.SequenceEqual("*allvisiblefactions"))
				{
					value2 = "every faction";
				}
				else if (First.SequenceEqual("*alloldfactions"))
				{
					value2 = "every faction that existed during the time of the sultanate";
				}
				if (value2.IsNullOrEmpty())
				{
					value2 = XRL.World.Faction.GetFormattedName(new string(First));
				}
				SB.Append(Rules ? "\n{{rules|" : "\n");
				if (!Prefix.IsNullOrEmpty())
				{
					SB.Append(Prefix);
				}
				if (SignedRules)
				{
					SB.AppendSigned(value, "rules");
				}
				else
				{
					SB.AppendSigned(value);
				}
				SB.Append(" reputation with ").Append(value2);
				if (!Postfix.IsNullOrEmpty())
				{
					SB.Append(Postfix);
				}
				if (Rules)
				{
					SB.Append("}}");
				}
			}
		}
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		CheckApplyBonus(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		CheckApplyBonus();
		return base.HandleEvent(E);
	}

	public static AddsRep AddModifier(GameObject Object, string Faction)
	{
		if (Object.TryGetPart<AddsRep>(out var Part))
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				if (Part.Faction != "")
				{
					Part.Faction += ",";
				}
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					string text2 = array2[0];
					int num = Convert.ToInt32(array2[1]);
					if (num == Part.Value && array2.Length <= 2)
					{
						Part.Faction += text2;
					}
					else
					{
						Part.Faction += text;
					}
					if (Part.AppliedBonus)
					{
						The.Game.PlayerReputation.Modify(text2, num, "AddsRepApply", null, null, Silent: true, Transient: true);
					}
				}
				else
				{
					Part.Faction += text;
					if (Part.AppliedBonus)
					{
						The.Game.PlayerReputation.Modify(text, Part.Value, "AddsRepApply", null, null, Silent: true, Transient: true);
					}
				}
			}
		}
		else
		{
			Part = new AddsRep(Faction);
			Object.AddPart(Part);
		}
		return Part;
	}

	public static AddsRep AddModifier(GameObject Object, string Faction, int Value)
	{
		if (Object.TryGetPart<AddsRep>(out var Part))
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				if (Part.Faction != "")
				{
					Part.Faction += ",";
				}
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					string text2 = array2[0];
					int num = Convert.ToInt32(array2[1]);
					if (num == Part.Value && array2.Length <= 2)
					{
						Part.Faction += text2;
					}
					else
					{
						Part.Faction += text;
					}
					if (Part.AppliedBonus)
					{
						The.Game.PlayerReputation.Modify(text2, num, "AddsRepApply", null, null, Silent: true, Transient: true);
					}
				}
				else
				{
					if (Value == Part.Value)
					{
						Part.Faction += text;
					}
					else
					{
						AddsRep addsRep = Part;
						addsRep.Faction = addsRep.Faction + text + ":" + Value;
					}
					if (Part.AppliedBonus)
					{
						The.Game.PlayerReputation.Modify(text, Value, "AddsRepApply", null, null, Silent: true, Transient: true);
					}
				}
			}
		}
		else
		{
			Part = new AddsRep(Faction, Value);
			Object.AddPart(Part);
		}
		return Part;
	}

	private void ApplyBonus(GameObject Subject)
	{
		if (AppliedBonus || Subject == null || !Subject.IsPlayer() || !IsObjectActivePartSubject(Subject))
		{
			return;
		}
		if (Faction.StartsWith("*allvisiblefactions:"))
		{
			int amount = Convert.ToInt32(Faction.Split(':')[1]);
			foreach (string visibleFactionName in Factions.GetVisibleFactionNames())
			{
				The.Game.PlayerReputation.Modify(visibleFactionName, amount, "AddsRepApply", null, null, Silent: true, Transient: true);
			}
		}
		else if (Faction == "*alloldfactions")
		{
			int value = Value;
			foreach (Faction item in Factions.Loop())
			{
				if (item.Visible && item.Old)
				{
					The.Game.PlayerReputation.Modify(item, value, null, null, "AddsRepApply", Silent: true, Transient: true);
				}
			}
		}
		else
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				string faction = text;
				int amount2 = Value;
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					faction = array2[0];
					amount2 = Convert.ToInt32(array2[1]);
				}
				The.Game.PlayerReputation.Modify(faction, amount2, "AddsRepApply", null, null, Silent: true, Transient: true);
			}
		}
		AppliedBonus = true;
	}

	private void UnapplyBonus()
	{
		if (!AppliedBonus)
		{
			return;
		}
		if (Faction.StartsWith("*allvisiblefactions:"))
		{
			int num = Convert.ToInt32(Faction.Split(':')[1]);
			foreach (string visibleFactionName in Factions.GetVisibleFactionNames())
			{
				The.Game.PlayerReputation.Modify(visibleFactionName, -num, "AddsRepUnapply", null, null, Silent: true, Transient: true);
			}
		}
		else if (Faction == "*alloldfactions")
		{
			int value = Value;
			foreach (Faction item in Factions.Loop())
			{
				if (item.Visible && item.Old)
				{
					The.Game.PlayerReputation.Modify(item, -value, null, null, "AddsRepUnapply", Silent: true, Transient: true);
				}
			}
		}
		else
		{
			string[] array = Faction.Split(',');
			foreach (string text in array)
			{
				string faction = text;
				int num2 = Value;
				if (text.Contains(':'))
				{
					string[] array2 = text.Split(':');
					faction = array2[0];
					num2 = Convert.ToInt32(array2[1]);
				}
				The.Game.PlayerReputation.Modify(faction, -num2, "AddsRepUnapply", null, null, Silent: true, Transient: true);
			}
		}
		AppliedBonus = false;
	}

	public void CheckApplyBonus(GameObject Subject = null, bool UseCharge = false, int MultipleCharge = 1)
	{
		if (Subject == null)
		{
			Subject = GetActivePartFirstSubject();
		}
		if (AppliedBonus)
		{
			if (IsDisabled(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L) || !Subject.IsPlayer())
			{
				UnapplyBonus();
			}
		}
		else if (IsReady(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyBonus(Subject);
		}
	}
}
