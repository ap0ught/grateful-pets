using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class MultiNavigationBonus : IPoweredPart
{
	public string DefaultApplicationKey;

	public bool ShowInShortDescription = true;

	public string _Bonuses;

	[NonSerialized]
	private List<string> _TravelClasses;

	[NonSerialized]
	private Dictionary<string, string> _TravelClassBonuses;

	[NonSerialized]
	private Dictionary<string, string> _TravelClassApplicationKeys;

	public string Bonuses
	{
		get
		{
			return _Bonuses;
		}
		set
		{
			_Bonuses = value;
			_TravelClasses = null;
			_TravelClassBonuses = null;
			_TravelClassApplicationKeys = null;
		}
	}

	public List<string> TravelClasses
	{
		get
		{
			if (_TravelClasses == null)
			{
				ParseBonuses();
			}
			return _TravelClasses;
		}
	}

	public Dictionary<string, string> TravelClassBonuses
	{
		get
		{
			if (_TravelClassBonuses == null)
			{
				ParseBonuses();
			}
			return _TravelClassBonuses;
		}
	}

	public Dictionary<string, string> TravelClassApplicationKeys
	{
		get
		{
			if (_TravelClassApplicationKeys == null)
			{
				ParseBonuses();
			}
			return _TravelClassApplicationKeys;
		}
	}

	public MultiNavigationBonus()
	{
		WorksOnEquipper = true;
	}

	public void ParseBonuses()
	{
		if (Bonuses == null)
		{
			Debug.LogError("no bonuses defined");
			return;
		}
		_TravelClasses = new List<string>();
		_TravelClassBonuses = new Dictionary<string, string>();
		_TravelClassApplicationKeys = new Dictionary<string, string>();
		string[] array = Bonuses.Split(',');
		foreach (string text in array)
		{
			string[] array2 = text.Split(':');
			if (array2.Length < 2 || array2.Length > 3)
			{
				Debug.LogError("Invalid bonus subpart specification, " + text);
				continue;
			}
			if (_TravelClasses.Contains(array2[0]))
			{
				Debug.LogError("Bonuses contain multiple specifications for " + array2[0]);
				continue;
			}
			if (_TravelClasses.Contains("*"))
			{
				Debug.LogError("Bonuses contain specifications after a * specification");
				continue;
			}
			_TravelClasses.Add(array2[0]);
			_TravelClassBonuses.Add(array2[0], array2[1]);
			if (array2.Length >= 3)
			{
				_TravelClassApplicationKeys.Add(array2[0], array2[2]);
			}
		}
	}

	public override bool SameAs(IPart p)
	{
		MultiNavigationBonus multiNavigationBonus = p as MultiNavigationBonus;
		if (multiNavigationBonus._Bonuses != _Bonuses)
		{
			return false;
		}
		if (multiNavigationBonus.DefaultApplicationKey != DefaultApplicationKey)
		{
			return false;
		}
		if (multiNavigationBonus.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetLostChanceEvent.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return ShowInShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		if (WasReady() && IsObjectActivePartSubject(E.Actor))
		{
			string text = null;
			string value = null;
			if (!string.IsNullOrEmpty(E.TravelClass) && TravelClasses.Contains(E.TravelClass))
			{
				text = TravelClassBonuses[E.TravelClass];
				TravelClassApplicationKeys.TryGetValue(E.TravelClass, out value);
			}
			else if (TravelClasses.Contains("*"))
			{
				text = TravelClassBonuses["*"];
				TravelClassApplicationKeys.TryGetValue("*", out value);
			}
			if (!string.IsNullOrEmpty(text))
			{
				int num = text.RollCached();
				if (num != 0)
				{
					int applied = E.GetApplied(value);
					int value2 = num;
					if (applied != 0)
					{
						if (num > 0 && applied > 0)
						{
							num = Math.Max(num - applied, 0);
							value2 = applied + num;
						}
						else if (num < 0 && applied < 0)
						{
							num = Math.Min(num - applied, 0);
							value2 = applied + num;
						}
						else if (Math.Abs(num) > Math.Abs(applied))
						{
							value2 = num;
							num -= applied;
						}
						else
						{
							num = 0;
							value2 = applied;
						}
					}
					if (num != 0)
					{
						E.PercentageBonus += num;
						E.SetApplied(value, value2);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			foreach (string travelClass in TravelClasses)
			{
				string dice = TravelClassBonuses[travelClass];
				int num = Stat.RollMin(dice);
				int num2 = Stat.RollMax(dice);
				if (num == 0 && num2 == 0)
				{
					continue;
				}
				E.Postfix.Append("\n{{rules|Chance of becoming lost ");
				if (!string.IsNullOrEmpty(travelClass))
				{
					if (travelClass == "*")
					{
						if (TravelClasses.Count > 1)
						{
							E.Postfix.Append("in other terrain ");
						}
					}
					else
					{
						Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + travelClass);
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
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (ChargeUse > 0)
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null && activePartFirstSubject.OnWorldMap())
			{
				ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}
}
