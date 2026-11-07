using System;
using System.Text;
using ConsoleLib.Console;
using XRL.Rules;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetDisplayNameEvent : PooledEvent<GetDisplayNameEvent>
{
	public GameObject Object;

	public DescriptionBuilder DB = new DescriptionBuilder();

	public string Context;

	public bool AsIfKnown;

	public bool Single;

	public bool NoConfusion;

	public bool NoColor;

	public bool ColorOnly;

	public bool Visible;

	public bool UsingAdjunctNoun;

	public bool WithoutTitles;

	public bool ForSort;

	public bool Reference;

	public bool IncludeImplantPrefix;

	public int Cutoff
	{
		get
		{
			return DB.Cutoff;
		}
		set
		{
			DB.Cutoff = value;
		}
	}

	public bool BaseOnly
	{
		get
		{
			return DB.BaseOnly;
		}
		set
		{
			DB.BaseOnly = value;
		}
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		DB.Clear();
		Context = null;
		AsIfKnown = false;
		Single = false;
		NoConfusion = false;
		NoColor = false;
		ColorOnly = false;
		Visible = false;
		UsingAdjunctNoun = false;
		WithoutTitles = false;
		ForSort = false;
		Reference = false;
		IncludeImplantPrefix = false;
	}

	public static string GetFor(GameObject Object, string Base, int Cutoff = int.MaxValue, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool ColorOnly = false, bool Visible = true, bool BaseOnly = false, bool UsingAdjunctNoun = false, bool WithoutTitles = false, bool ForSort = false, bool Reference = false, bool IncludeImplantPrefix = true)
	{
		GetDisplayNameEvent E = PooledEvent<GetDisplayNameEvent>.FromPool();
		E.DB.Reset();
		E.Object = Object;
		E.Context = Context;
		E.AsIfKnown = AsIfKnown;
		E.Single = Single;
		E.NoConfusion = NoConfusion;
		E.NoColor = NoColor;
		E.ColorOnly = ColorOnly;
		E.Visible = Visible;
		E.UsingAdjunctNoun = UsingAdjunctNoun;
		E.WithoutTitles = WithoutTitles;
		E.ForSort = ForSort;
		E.Reference = Reference;
		E.IncludeImplantPrefix = IncludeImplantPrefix;
		E.AddBase(Base);
		E.Cutoff = Cutoff;
		E.BaseOnly = BaseOnly;
		E.DB.Object = Object;
		string color;
		if (ColorOnly)
		{
			if (!E.DB.Color.IsNullOrEmpty())
			{
				color = E.DB.Color;
			}
			color = ColorUtility.GetMainForegroundColor(E.ProcessFor(Object));
		}
		else
		{
			color = E.ProcessFor(Object);
		}
		E.DB.Reset();
		PooledEvent<GetDisplayNameEvent>.ResetTo(ref E);
		return color;
	}

	public static void ReplaceFor(GameObject Object, GetDisplayNameEvent E, string Base)
	{
		E.ReplacePrimaryBase(Base);
		E.ProcessFor(Object, NoReturn: true);
	}

	public void Add(string desc, int order = 0)
	{
		DB.Add(desc, order);
	}

	public void AddBase(string desc, int orderAdjust = 0, bool secondary = false)
	{
		DB.AddBase(desc, orderAdjust, secondary);
	}

	public void ReplaceEntirety(string Name)
	{
		DB.Clear();
		DB.AddBase(Name);
	}

	public void ReplacePrimaryBase(string desc, int orderAdjust = 0)
	{
		DB.ReplacePrimaryBase(desc, orderAdjust);
	}

	public string GetPrimaryBase()
	{
		return DB.PrimaryBase;
	}

	public void AddAdjective(string desc, int orderAdjust = 0)
	{
		DB.AddAdjective(desc, orderAdjust);
	}

	public void ApplySizeAdjective(string Adjective, int Priority = 10, int OrderAdjust = 0)
	{
		DB.ApplySizeAdjective(Adjective, Priority, OrderAdjust);
	}

	public void AddClause(string Clause, int OrderAdjust = 0)
	{
		DB.AddClause(Clause, OrderAdjust);
	}

	public void AddHonorific(string Honorific, int OrderAdjust = 0)
	{
		if (!WithoutTitles || OrderAdjust >= 20)
		{
			DB.AddHonorific(Honorific, OrderAdjust);
		}
	}

	public void AddEpithet(string Epithet, int OrderAdjust = 0)
	{
		if (!WithoutTitles)
		{
			DB.AddEpithet(Epithet, OrderAdjust);
		}
	}

	public void AddTitle(string Title, int OrderAdjust = 0)
	{
		if (!WithoutTitles)
		{
			DB.AddTitle(Title, OrderAdjust);
		}
	}

	public void AddWithClause(string Clause)
	{
		DB.AddWithClause(Clause);
	}

	public void AddTag(string Tag, int OrderAdjust = 0)
	{
		DB.AddTag(Tag, OrderAdjust);
	}

	public void AddMark(string Mark, int OrderAdjust = 0)
	{
		DB.AddMark(Mark, OrderAdjust);
	}

	public void AddColor(string Color, int Priority = 0)
	{
		if (!NoColor)
		{
			DB.AddColor(Color, Priority);
		}
	}

	public void AddColor(char Color, int Priority = 0)
	{
		if (!NoColor)
		{
			DB.AddColor(Color, Priority);
		}
	}

	public bool Understood()
	{
		if (!AsIfKnown && Object != null)
		{
			return Object.Understood();
		}
		return true;
	}

	public void AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (E.Attributes == null || !E.Attributes.Contains("NonPenetrating"))
		{
			string text;
			if (E.PenetrateWalls)
			{
				text = "m";
			}
			else
			{
				text = (E.PenetrateCreatures ? "W" : "c");
			}
			if (!GetDisplayNamePenetrationColorEvent.GetFor(Object, text).IsNullOrEmpty())
			{
				stringBuilder.Append("{{").Append(text).Append('|')
					.Append('\u001a')
					.Append("}}");
			}
			else
			{
				stringBuilder.Append('\u001a');
			}
			if (E.Attributes != null && E.Attributes.Contains("Vorpal"))
			{
				stringBuilder.Append('÷');
			}
			else
			{
				stringBuilder.Append(Math.Max(E.Penetration + RuleSettings.VISUAL_PENETRATION_BONUS, 1));
			}
		}
		if (E.DamageRoll != null || (!E.BaseDamage.IsNullOrEmpty() && E.BaseDamage != "0"))
		{
			stringBuilder.Compound("{{").Append(E.GetDamageColor()).Append('|')
				.Append('\u0003')
				.Append("}}")
				.Append((E.DamageRoll != null) ? E.DamageRoll.ToString() : E.BaseDamage);
		}
		if (stringBuilder.Length > 0)
		{
			AddTag(stringBuilder.ToString(), -20);
		}
	}

	public string ProcessFor(GameObject obj, bool NoReturn = false)
	{
		obj.HandleEvent(this);
		if (obj.HasRegisteredEvent("GetDisplayName"))
		{
			string text = DB.ToString();
			StringBuilder stringBuilder = Event.NewStringBuilder();
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			StringBuilder stringBuilder3 = Event.NewStringBuilder();
			StringBuilder stringBuilder4 = Event.NewStringBuilder();
			StringBuilder stringBuilder5 = Event.NewStringBuilder();
			stringBuilder.Append(text);
			Event obj2 = Event.New("GetDisplayName");
			obj2.SetParameter("Object", obj);
			obj2.SetParameter("DisplayName", stringBuilder);
			obj2.SetParameter("Prefix", stringBuilder2);
			obj2.SetParameter("Infix", stringBuilder3);
			obj2.SetParameter("Postfix", stringBuilder4);
			obj2.SetParameter("PostPostfix", stringBuilder5);
			obj2.SetParameter("Cutoff", Cutoff);
			obj2.SetParameter("Context", Context);
			obj2.SetFlag("BaseOnly", BaseOnly);
			obj2.SetFlag("AsIfKnown", AsIfKnown);
			obj2.SetFlag("Single", Single);
			obj2.SetFlag("NoConfusion", NoConfusion);
			obj2.SetFlag("NoColor", NoColor);
			obj2.SetFlag("ColorOnly", ColorOnly);
			obj2.SetFlag("Visible", Visible);
			obj2.SetFlag("UsingAdjunctNoun", UsingAdjunctNoun);
			obj2.SetFlag("WithoutTitles", WithoutTitles);
			obj2.SetFlag("ForSort", ForSort);
			obj2.SetFlag("Reference", Reference);
			obj.FireEvent(obj2);
			if (stringBuilder.Length != text.Length || stringBuilder.ToString() != text)
			{
				DB.Clear();
				DB.AddBase(stringBuilder.ToString());
			}
			if (!BaseOnly)
			{
				if (stringBuilder2.Length != 0)
				{
					AddAdjective(stringBuilder2.ToString().Trim());
				}
				if (stringBuilder3.Length != 0)
				{
					AddClause(stringBuilder3.ToString().Trim());
				}
				if (stringBuilder4.Length != 0)
				{
					AddTag(stringBuilder4.ToString().Trim());
				}
				if (stringBuilder5.Length != 0)
				{
					AddTag(stringBuilder5.ToString().Trim(), 20);
				}
			}
		}
		if (!NoReturn)
		{
			return Markup.Wrap(DB.ToString());
		}
		return null;
	}
}
