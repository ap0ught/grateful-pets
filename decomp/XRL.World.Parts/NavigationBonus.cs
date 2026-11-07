using System;
using Qud.API;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class NavigationBonus : IPoweredPart
{
	public string PercentBonus = "10";

	public string SpeedPercentBonus;

	public string EncounterPercentBonus;

	public string EncounterType;

	public string EncounterTypePercentBonus;

	public string SingleApplicationKey;

	public string TravelClass;

	public bool ShowInShortDescription = true;

	public float ComputePowerFactor;

	public NavigationBonus()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		NavigationBonus navigationBonus = p as NavigationBonus;
		if (navigationBonus.PercentBonus != PercentBonus)
		{
			return false;
		}
		if (navigationBonus.SpeedPercentBonus != SpeedPercentBonus)
		{
			return false;
		}
		if (navigationBonus.EncounterPercentBonus != EncounterPercentBonus)
		{
			return false;
		}
		if (navigationBonus.EncounterType != EncounterType)
		{
			return false;
		}
		if (navigationBonus.EncounterTypePercentBonus != EncounterTypePercentBonus)
		{
			return false;
		}
		if (navigationBonus.SingleApplicationKey != SingleApplicationKey)
		{
			return false;
		}
		if (navigationBonus.TravelClass != TravelClass)
		{
			return false;
		}
		if (navigationBonus.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		if (navigationBonus.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return ChargeUse > 0;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		if (activePartFirstSubject != null && activePartFirstSubject.OnWorldMap())
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: true, Amount);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EncounterChanceEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != GetLostChanceEvent.ID && (ID != GetShortDescriptionEvent.ID || !ShowInShortDescription))
		{
			return ID == TravelSpeedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EncounterChanceEvent E)
	{
		if (!string.IsNullOrEmpty(EncounterPercentBonus) && WasReady() && (string.IsNullOrEmpty(TravelClass) || string.IsNullOrEmpty(E.TravelClass) || E.TravelClass == TravelClass) && IsObjectActivePartSubject(E.Actor))
		{
			int num = GetAvailableComputePowerEvent.AdjustUp(this, EncounterPercentBonus.RollCached(), ComputePowerFactor);
			if (num != 0)
			{
				int applied = E.GetApplied(SingleApplicationKey);
				int value = num;
				if (applied != 0)
				{
					if (num > 0 && applied > 0)
					{
						num = Math.Max(num - applied, 0);
						value = applied + num;
					}
					else if (num < 0 && applied < 0)
					{
						num = Math.Min(num - applied, 0);
						value = applied + num;
					}
					else if (Math.Abs(num) > Math.Abs(applied))
					{
						value = num;
						num -= applied;
					}
					else
					{
						num = 0;
						value = applied;
					}
				}
				if (num != 0)
				{
					E.PercentageBonus += num;
					E.SetApplied(SingleApplicationKey, value);
				}
			}
		}
		if (!string.IsNullOrEmpty(EncounterType) && !string.IsNullOrEmpty(EncounterTypePercentBonus) && E.Encounter != null && WasReady() && IsObjectActivePartSubject(E.Actor))
		{
			string text = E.Encounter?.secretID;
			IBaseJournalEntry baseJournalEntry = (string.IsNullOrEmpty(text) ? null : JournalAPI.GetMapNote(text));
			if (baseJournalEntry != null && baseJournalEntry.Has(EncounterType))
			{
				int num2 = GetAvailableComputePowerEvent.AdjustUp(this, EncounterTypePercentBonus.RollCached(), ComputePowerFactor);
				if (num2 != 0)
				{
					int applied2 = E.GetApplied(SingleApplicationKey);
					int value2 = num2;
					if (applied2 != 0)
					{
						if (num2 > 0 && applied2 > 0)
						{
							num2 = Math.Max(num2 - applied2, 0);
							value2 = applied2 + num2;
						}
						else if (num2 < 0 && applied2 < 0)
						{
							num2 = Math.Min(num2 - applied2, 0);
							value2 = applied2 + num2;
						}
						else if (Math.Abs(num2) > Math.Abs(applied2))
						{
							value2 = num2;
							num2 -= applied2;
						}
						else
						{
							num2 = 0;
							value2 = applied2;
						}
					}
					if (num2 != 0)
					{
						E.PercentageBonus += num2;
						E.SetApplied(SingleApplicationKey, value2);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		if (!string.IsNullOrEmpty(PercentBonus) && WasReady() && (string.IsNullOrEmpty(TravelClass) || string.IsNullOrEmpty(E.TravelClass) || E.TravelClass == TravelClass) && IsObjectActivePartSubject(E.Actor))
		{
			int num = GetAvailableComputePowerEvent.AdjustUp(this, PercentBonus.RollCached(), ComputePowerFactor);
			if (num != 0)
			{
				int applied = E.GetApplied(SingleApplicationKey);
				int value = num;
				if (applied != 0)
				{
					if (num > 0 && applied > 0)
					{
						num = Math.Max(num - applied, 0);
						value = applied + num;
					}
					else if (num < 0 && applied < 0)
					{
						num = Math.Min(num - applied, 0);
						value = applied + num;
					}
					else if (Math.Abs(num) > Math.Abs(applied))
					{
						value = num;
						num -= applied;
					}
					else
					{
						num = 0;
						value = applied;
					}
				}
				if (num != 0)
				{
					E.PercentageBonus += num;
					if (num > 0)
					{
						E.OverrideDefaultLimit = true;
					}
					E.SetApplied(SingleApplicationKey, value);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		if (!string.IsNullOrEmpty(SpeedPercentBonus) && WasReady() && (string.IsNullOrEmpty(TravelClass) || string.IsNullOrEmpty(E.TravelClass) || E.TravelClass == TravelClass) && IsObjectActivePartSubject(E.Actor))
		{
			int num = GetAvailableComputePowerEvent.AdjustUp(this, SpeedPercentBonus.RollCached(), ComputePowerFactor);
			if (num != 0)
			{
				int applied = E.GetApplied(SingleApplicationKey);
				int value = num;
				if (applied != 0)
				{
					if (num > 0 && applied > 0)
					{
						num = Math.Max(num - applied, 0);
						value = applied + num;
					}
					else if (num < 0 && applied < 0)
					{
						num = Math.Min(num - applied, 0);
						value = applied + num;
					}
					else if (Math.Abs(num) > Math.Abs(applied))
					{
						value = num;
						num -= applied;
					}
					else
					{
						num = 0;
						value = applied;
					}
				}
				if (num != 0)
				{
					E.PercentageBonus += num;
					E.SetApplied(SingleApplicationKey, value);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			if (!string.IsNullOrEmpty(PercentBonus))
			{
				int num = Stat.RollMin(PercentBonus);
				int num2 = Stat.RollMax(PercentBonus);
				if (num != 0 || num2 != 0)
				{
					E.Postfix.Append("\n{{rules|Chance of becoming lost ");
					if (!string.IsNullOrEmpty(TravelClass))
					{
						Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + TravelClass);
						BaseTerrainSurvivalSkill baseTerrainSurvivalSkill = ((type != null) ? (Activator.CreateInstance(type) as BaseTerrainSurvivalSkill) : null);
						if (baseTerrainSurvivalSkill == null)
						{
							E.Postfix.Append("in certain terrain ");
						}
						else
						{
							E.Postfix.Append("in ").Append(baseTerrainSurvivalSkill.GetTerrainName()).Append(' ');
						}
					}
					if (num >= 0 == num2 >= 0)
					{
						E.Postfix.Append((num >= 0) ? "reduced" : "increased").Append(" by ");
						if (num == num2)
						{
							E.Postfix.Append((num > 0) ? num : (-num));
						}
						else if (num <= 0)
						{
							E.Postfix.Append(-num2).Append('-').Append(-num);
						}
						else
						{
							E.Postfix.Append(num).Append('-').Append(num2);
						}
						E.Postfix.Append("%.");
					}
					else
					{
						E.Postfix.Append(" adjusted by ").Append(-num2).Append("% to ");
						if (num < 0)
						{
							E.Postfix.Append('+');
						}
						E.Postfix.Append(-num).Append("%.");
					}
					AddStatusSummary(E.Postfix);
					E.Postfix.Append("}}");
				}
			}
			if (!string.IsNullOrEmpty(EncounterPercentBonus))
			{
				int num3 = Stat.RollMin(EncounterPercentBonus);
				int num4 = Stat.RollMax(EncounterPercentBonus);
				if (num3 != 0 || num4 != 0)
				{
					E.Postfix.Append("\n{{rules|Chance of interesting encounters while traveling ");
					if (!string.IsNullOrEmpty(TravelClass))
					{
						Type type2 = ModManager.ResolveType("XRL.World.Parts.Skill." + TravelClass);
						BaseTerrainSurvivalSkill baseTerrainSurvivalSkill2 = ((type2 != null) ? (Activator.CreateInstance(type2) as BaseTerrainSurvivalSkill) : null);
						if (baseTerrainSurvivalSkill2 == null)
						{
							E.Postfix.Append("through certain terrain on the world map");
						}
						else
						{
							E.Postfix.Append("through ").Append(baseTerrainSurvivalSkill2.GetTerrainName()).Append(" on the world map ");
						}
					}
					if (num3 >= 0 == num4 >= 0)
					{
						E.Postfix.Append((num3 >= 0) ? "increased" : "reduced").Append(" by ");
						if (num3 == num4)
						{
							E.Postfix.Append((num3 > 0) ? num3 : (-num3));
						}
						else if (num3 <= 0)
						{
							E.Postfix.Append(-num3).Append('-').Append(num4);
						}
						else
						{
							E.Postfix.Append(-num4).Append('-').Append(num3);
						}
						E.Postfix.Append("%.");
					}
					else
					{
						E.Postfix.Append(" adjusted by ").Append(num3).Append("% to ");
						if (num3 < 0)
						{
							E.Postfix.Append('+');
						}
						E.Postfix.Append(num4).Append("%.");
					}
					AddStatusSummary(E.Postfix);
					E.Postfix.Append("}}");
				}
			}
			if (!string.IsNullOrEmpty(EncounterType) && !string.IsNullOrEmpty(EncounterTypePercentBonus))
			{
				int num5 = Stat.RollMin(EncounterTypePercentBonus);
				int num6 = Stat.RollMax(EncounterTypePercentBonus);
				if (num5 != 0 || num6 != 0)
				{
					E.Postfix.Append("\n{{rules|Chance of encountering ").Append(EncounterType).Append(" while traveling ");
					if (!string.IsNullOrEmpty(TravelClass))
					{
						E.Postfix.Append("through other terrain on the world map ");
					}
					else
					{
						E.Postfix.Append("on the world map ");
					}
					if (num5 >= 0 == num6 >= 0)
					{
						E.Postfix.Append((num5 >= 0) ? "increased" : "reduced").Append(" by ");
						if (num5 == num6)
						{
							E.Postfix.Append((num5 > 0) ? num5 : (-num5));
						}
						else if (num5 <= 0)
						{
							E.Postfix.Append(-num5).Append('-').Append(num6);
						}
						else
						{
							E.Postfix.Append(-num6).Append('-').Append(num5);
						}
						E.Postfix.Append("%.");
					}
					else
					{
						E.Postfix.Append(" adjusted by ").Append(num5).Append("% to ");
						if (num5 < 0)
						{
							E.Postfix.Append('+');
						}
						E.Postfix.Append(num6).Append("%.");
					}
					AddStatusSummary(E.Postfix);
					E.Postfix.Append("}}");
				}
			}
			if (!string.IsNullOrEmpty(SpeedPercentBonus))
			{
				int num7 = Stat.RollMin(SpeedPercentBonus);
				int num8 = Stat.RollMax(SpeedPercentBonus);
				if (num7 != 0 || num8 != 0)
				{
					E.Postfix.Append("\n{{rules|Speed while traveling on the world map ");
					if (!string.IsNullOrEmpty(TravelClass))
					{
						Type type3 = ModManager.ResolveType("XRL.World.Parts.Skill." + TravelClass);
						BaseTerrainSurvivalSkill baseTerrainSurvivalSkill3 = ((type3 != null) ? (Activator.CreateInstance(type3) as BaseTerrainSurvivalSkill) : null);
						if (baseTerrainSurvivalSkill3 == null)
						{
							E.Postfix.Append("in certain terrain ");
						}
						else
						{
							E.Postfix.Append("in ").Append(baseTerrainSurvivalSkill3.GetTerrainName()).Append(' ');
						}
					}
					if (num7 >= 0 == num8 >= 0)
					{
						E.Postfix.Append((num7 >= 0) ? "increased" : "reduced").Append(" by ");
						if (num7 == num8)
						{
							E.Postfix.Append((num7 > 0) ? num7 : (-num7));
						}
						else if (num7 <= 0)
						{
							E.Postfix.Append(-num7).Append('-').Append(num8);
						}
						else
						{
							E.Postfix.Append(-num8).Append('-').Append(num7);
						}
						E.Postfix.Append("%.");
					}
					else
					{
						E.Postfix.Append(" adjusted by ").Append(num7).Append("% to ");
						if (num7 < 0)
						{
							E.Postfix.Append('+');
						}
						E.Postfix.Append(num8).Append("%.");
					}
					AddStatusSummary(E.Postfix);
					E.Postfix.Append("}}");
				}
			}
			if (ComputePowerFactor > 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
			}
			else if (ComputePowerFactor < 0f)
			{
				E.Postfix.AppendRules("Compute power on the local lattice decreases this item's effectiveness.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "PercentBonus", PercentBonus);
		E.AddEntry(this, "SpeedPercentBonus", SpeedPercentBonus);
		E.AddEntry(this, "EncounterPercentBonus", EncounterPercentBonus);
		E.AddEntry(this, "EncounterType", EncounterType);
		E.AddEntry(this, "EncounterTypePercentBonus", EncounterTypePercentBonus);
		E.AddEntry(this, "SingleApplicationKey", SingleApplicationKey);
		E.AddEntry(this, "TravelClass", TravelClass);
		E.AddEntry(this, "ShowInShortDescription", ShowInShortDescription);
		E.AddEntry(this, "ComputePowerFactor", ComputePowerFactor);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}
}
