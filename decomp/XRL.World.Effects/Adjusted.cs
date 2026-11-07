using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Adjusted : Effect
{
	public string Spec;

	public bool Stackable;

	public bool Refreshes = true;

	public bool Suppress;

	public List<string> StatAdjustOrder;

	public Dictionary<string, int> StatAdjusts;

	public List<string> SaveAdjustOrder;

	public Dictionary<string, int> SaveAdjusts;

	public List<string> PropertyAdjustOrder;

	public Dictionary<string, int> PropertyAdjusts;

	public bool Animate;

	public string AnimateRenderString;

	public string AnimateTile;

	public string AnimateColorString;

	public string AnimateDetailColor;

	public int AnimateFirstFrame = 25;

	public int AnimateLastFrame = 45;

	public string SourceID;

	public Adjusted()
	{
		DisplayName = "{{g|adjusted}}";
	}

	public Adjusted(string Spec, int Duration)
		: this()
	{
		this.Spec = Spec;
		base.Duration = Duration;
	}

	public Adjusted(string Spec, int Duration, GameObject Source)
		: this(Spec, Duration)
	{
		SourceID = Source.ID;
	}

	public Adjusted(string Spec, int Duration, string SourceID)
		: this(Spec, Duration)
	{
		this.SourceID = SourceID;
	}

	public Adjusted(Adjusted Source)
	{
		Spec = Source.Spec;
		DisplayName = Source.DisplayName;
		Duration = Source.Duration;
		Stackable = Source.Stackable;
		Refreshes = Source.Refreshes;
		StatAdjustOrder = Source.StatAdjustOrder;
		StatAdjusts = Source.StatAdjusts;
		SaveAdjustOrder = Source.SaveAdjustOrder;
		SaveAdjusts = Source.SaveAdjusts;
		PropertyAdjustOrder = Source.PropertyAdjustOrder;
		PropertyAdjusts = Source.PropertyAdjusts;
		Animate = Source.Animate;
		AnimateRenderString = Source.AnimateRenderString;
		AnimateTile = Source.AnimateTile;
		AnimateColorString = Source.AnimateColorString;
		AnimateDetailColor = Source.AnimateDetailColor;
		AnimateFirstFrame = Source.AnimateFirstFrame;
		AnimateLastFrame = Source.AnimateLastFrame;
		SourceID = Source.SourceID;
	}

	public override bool SuppressInLookDisplay()
	{
		return Suppress;
	}

	public override bool SuppressInStageDisplay()
	{
		return Suppress;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		int num = 0;
		int num2 = 0;
		if (StatAdjustOrder != null)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (string item in StatAdjustOrder)
			{
				num2 += (Statistic.IsInverseBenefit(item) ? (-StatAdjusts[item]) : StatAdjusts[item]);
				if (Statistic.IsMental(item))
				{
					flag2 = true;
				}
				else
				{
					flag = true;
				}
				if (flag2)
				{
					num |= 2;
				}
				if (flag)
				{
					num |= 4;
				}
			}
		}
		if (SaveAdjustOrder != null)
		{
			foreach (int value in SaveAdjusts.Values)
			{
				num2 += value;
			}
			num |= 0x80;
		}
		if (PropertyAdjustOrder != null)
		{
			foreach (string item2 in PropertyAdjustOrder)
			{
				num2 += (PropertyDescription.IsInverseBenefit(item2) ? (-PropertyAdjusts[item2]) : PropertyAdjusts[item2]);
				num |= PropertyDescription.GetPropertyEffectType(item2);
			}
		}
		if (num2 < 0)
		{
			num |= 0x2000000;
		}
		if (Math.Abs(num2) <= 3)
		{
			num |= 0x1000000;
		}
		return num;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (StatAdjustOrder != null)
		{
			foreach (string item in StatAdjustOrder)
			{
				stringBuilder.Compound(Statistic.GetStatAdjustDescription(item, StatAdjusts[item]), "\n");
			}
		}
		if (SaveAdjustOrder != null)
		{
			foreach (string item2 in SaveAdjustOrder)
			{
				SavingThrows.AppendSaveBonusDescription(stringBuilder, SaveAdjusts[item2], item2);
			}
		}
		if (PropertyAdjustOrder != null)
		{
			foreach (string item3 in PropertyAdjustOrder)
			{
				stringBuilder.Compound(PropertyDescription.GetPropertyAdjustDescription(item3, PropertyAdjusts[item3]), "\n");
			}
		}
		return stringBuilder.ToString();
	}

	public override bool SameAs(Effect e)
	{
		Adjusted adjusted = e as Adjusted;
		if (adjusted.Spec != Spec)
		{
			return false;
		}
		if (adjusted.Stackable != Stackable)
		{
			return false;
		}
		if (adjusted.Refreshes != Refreshes)
		{
			return false;
		}
		if (adjusted.Animate != Animate)
		{
			return false;
		}
		if (adjusted.AnimateRenderString != AnimateRenderString)
		{
			return false;
		}
		if (adjusted.AnimateTile != AnimateTile)
		{
			return false;
		}
		if (adjusted.AnimateColorString != AnimateColorString)
		{
			return false;
		}
		if (adjusted.AnimateDetailColor != AnimateDetailColor)
		{
			return false;
		}
		if (adjusted.AnimateFirstFrame != AnimateFirstFrame)
		{
			return false;
		}
		if (adjusted.AnimateLastFrame != AnimateLastFrame)
		{
			return false;
		}
		if (adjusted.SourceID != SourceID)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public void ApplySpec()
	{
		if (Spec == null)
		{
			MetricsManager.LogError("spec was null");
			return;
		}
		string[] array = Spec.Split('|');
		if (array.Length < 1)
		{
			MetricsManager.LogError("invalid spec '" + Spec + "'");
			return;
		}
		int i = 0;
		DisplayName = array[i++];
		bool flag;
		do
		{
			flag = false;
			if (i < array.Length && array[i] == "stackable")
			{
				Stackable = true;
				i++;
				flag = true;
			}
			if (i < array.Length && array[i] == "nostack")
			{
				Stackable = false;
				i++;
				flag = true;
			}
			if (i < array.Length && array[i] == "refresh")
			{
				Refreshes = true;
				i++;
				flag = true;
			}
			if (i < array.Length && array[i] == "norefresh")
			{
				Refreshes = false;
				i++;
				flag = true;
			}
			if (i < array.Length && array[i] == "suppress")
			{
				Suppress = true;
				i++;
				flag = true;
			}
			if (i < array.Length && array[i] == "nosuppress")
			{
				Suppress = false;
				i++;
				flag = true;
			}
		}
		while (flag);
		for (; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length != 3)
			{
				MetricsManager.LogError("invalid spec part " + array[i]);
				continue;
			}
			string text = array2[0];
			string text2 = array2[1];
			string text3 = array2[2];
			int result = 0;
			bool flag2 = false;
			switch (text)
			{
			case "Mult":
			case "Stat":
			case "Save":
			case "Property":
				flag2 = true;
				break;
			case "Animate":
				if (text2 == "FirstFrame" || text2 == "LastFrame")
				{
					flag2 = true;
				}
				break;
			}
			if (flag2)
			{
				if (!int.TryParse(text3, out result))
				{
					MetricsManager.LogError("invalid spec part value " + text3);
					continue;
				}
				if (text == "Mult")
				{
					result = (int)((float)Object.GetStat(text2).BaseValue * ((float)result / 100f));
				}
			}
			switch (text)
			{
			case "Mult":
			case "Stat":
				if (StatAdjustOrder == null)
				{
					StatAdjustOrder = new List<string>();
				}
				if (StatAdjusts == null)
				{
					StatAdjusts = new Dictionary<string, int>();
				}
				StatAdjustOrder.Add(text2);
				StatAdjusts.Add(text2, result);
				break;
			case "Save":
				if (SaveAdjustOrder == null)
				{
					SaveAdjustOrder = new List<string>();
				}
				if (SaveAdjusts == null)
				{
					SaveAdjusts = new Dictionary<string, int>();
				}
				SaveAdjustOrder.Add(text2);
				SaveAdjusts.Add(text2, result);
				break;
			case "Property":
				if (PropertyAdjustOrder == null)
				{
					PropertyAdjustOrder = new List<string>();
				}
				if (PropertyAdjusts == null)
				{
					PropertyAdjusts = new Dictionary<string, int>();
				}
				PropertyAdjustOrder.Add(text2);
				PropertyAdjusts.Add(text2, result);
				break;
			case "Animate":
				switch (text2)
				{
				case "RenderString":
					AnimateRenderString = text3;
					break;
				case "Tile":
					AnimateTile = text3;
					break;
				case "ColorString":
					AnimateColorString = text3;
					break;
				case "DetailColor":
					AnimateDetailColor = text3;
					break;
				case "FirstFrame":
					AnimateFirstFrame = result;
					break;
				case "LastFrame":
					AnimateLastFrame = result;
					break;
				default:
					MetricsManager.LogError("Animate spec part subtype must be RenderString, Tile, ColorString, DetailColor, FirstFrame, or LastFrame, had '" + text + "'");
					goto end_IL_0234;
				}
				Animate = true;
				break;
			default:
				{
					MetricsManager.LogError("spec part type must be Stat, Save, Property, or Animate, had '" + text + "'");
					break;
				}
				end_IL_0234:
				break;
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (Animate && Duration > 0 && XRLCore.CurrentFrame >= AnimateFirstFrame && XRLCore.CurrentFrame < AnimateLastFrame)
		{
			if (!string.IsNullOrEmpty(AnimateRenderString))
			{
				E.Tile = null;
				E.RenderString = AnimateRenderString;
			}
			else if (!string.IsNullOrEmpty(AnimateTile))
			{
				E.Tile = AnimateTile;
			}
			if (!string.IsNullOrEmpty(AnimateColorString))
			{
				E.ColorString = AnimateColorString;
			}
			if (!string.IsNullOrEmpty(AnimateDetailColor))
			{
				E.DetailColor = AnimateDetailColor;
			}
		}
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Stackable && Object.HasEffect<Adjusted>())
		{
			foreach (Adjusted item in Object.YieldEffects<Adjusted>())
			{
				if (item != null && item.Spec == Spec)
				{
					if (Refreshes && item.Duration < Duration)
					{
						item.Duration = Duration;
					}
					return false;
				}
			}
		}
		if (Object.FireEvent(Event.New("ApplyAdjusted", "Effect", this, "Duration", Duration)))
		{
			ApplyAdjustments();
			return true;
		}
		return false;
	}

	private void ApplyAdjustments()
	{
		if (StatAdjustOrder == null && PropertyAdjustOrder == null && SaveAdjustOrder == null && PropertyAdjustOrder == null)
		{
			ApplySpec();
		}
		if (StatAdjustOrder != null)
		{
			foreach (string item in StatAdjustOrder)
			{
				StatShifter.SetStatShift(item, StatAdjusts[item], item == "Hitpoints");
			}
		}
		if (PropertyAdjustOrder == null)
		{
			return;
		}
		foreach (string item2 in PropertyAdjustOrder)
		{
			Object.ModIntProperty(item2, PropertyAdjusts[item2], RemoveIfZero: true);
		}
	}

	private void UnapplyAdjustments()
	{
		if (StatAdjustOrder != null)
		{
			StatShifter.RemoveStatShifts(Object);
		}
		if (PropertyAdjustOrder == null)
		{
			return;
		}
		foreach (string item in PropertyAdjustOrder)
		{
			Object.ModIntProperty(item, -PropertyAdjusts[item], RemoveIfZero: true);
		}
	}

	public override void Remove(GameObject Object)
	{
		UnapplyAdjustments();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == ModifyDefendingSaveEvent.ID)
			{
				return SaveAdjustOrder != null;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SaveAdjustOrder != null)
		{
			foreach (string item in SaveAdjustOrder)
			{
				if (SavingThrows.Applicable(item, E))
				{
					E.Roll += SaveAdjusts[item];
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyAdjustments();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyAdjustments();
		}
		return base.FireEvent(E);
	}
}
