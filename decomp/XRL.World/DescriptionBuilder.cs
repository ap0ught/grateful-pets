using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.World;

public class DescriptionBuilder : Dictionary<string, int>
{
	public const int ORDER_BASE = 10;

	public const int ORDER_ADJECTIVE = -500;

	public const int ORDER_CLAUSE = 600;

	public const int ORDER_TAG = 1100;

	public const int ORDER_MARK = -800;

	public const int ORDER_ADJUST_EXTREMELY_EARLY = -60;

	public const int ORDER_ADJUST_VERY_EARLY = -40;

	public const int ORDER_ADJUST_EARLY = -20;

	public const int ORDER_ADJUST_SLIGHTLY_EARLY = -5;

	public const int ORDER_ADJUST_SLIGHTLY_LATE = 5;

	public const int ORDER_ADJUST_LATE = 20;

	public const int ORDER_ADJUST_VERY_LATE = 40;

	public const int ORDER_ADJUST_EXTREMELY_LATE = 60;

	public const int SHORT_CUTOFF = 1040;

	public const int PRIORITY_VERY_LOW = 5;

	public const int PRIORITY_LOW = 10;

	public const int PRIORITY_MEDIUM = 20;

	public const int PRIORITY_HIGH = 30;

	public const int PRIORITY_OVERRIDE = 40;

	public const int PRIORITY_ADJUST_SMALL = 1;

	public const int PRIORITY_ADJUST_MEDIUM = 2;

	public const int PRIORITY_ADJUST_LARGE = 3;

	public GameObject Object;

	public int Cutoff = int.MaxValue;

	public string PrimaryBase;

	public string LastAdded;

	public string Color;

	public int ColorPriority;

	public string SizeAdjective;

	public int SizeAdjectivePriority;

	public int SizeAdjectiveOrderAdjust;

	public List<string> Epithets;

	public Dictionary<string, int> EpithetOrder;

	public List<string> Titles;

	public Dictionary<string, int> TitleOrder;

	public List<string> WithClauses;

	public bool BaseOnly;

	private static List<string> Descs = new List<string>();

	private static StringBuilder SB = new StringBuilder();

	public DescriptionBuilder()
	{
	}

	public DescriptionBuilder(int Cutoff)
		: this()
	{
		this.Cutoff = Cutoff;
	}

	public DescriptionBuilder(int Cutoff, bool BaseOnly)
		: this(Cutoff)
	{
		this.BaseOnly = BaseOnly;
	}

	public new void Add(string Desc, int Order = 0)
	{
		if (Order >= Cutoff)
		{
			return;
		}
		if (ContainsKey(Desc))
		{
			if (base[Desc] < Order)
			{
				base[Desc] = Order;
				LastAdded = Desc;
			}
		}
		else
		{
			base.Add(Desc, Order);
			LastAdded = Desc;
		}
	}

	public new void Remove(string Desc)
	{
		base.Remove(Desc);
		if (Desc == PrimaryBase)
		{
			PrimaryBase = null;
		}
	}

	public new void Clear()
	{
		base.Clear();
		PrimaryBase = null;
		LastAdded = null;
		Color = null;
		SizeAdjective = null;
		Epithets?.Clear();
		EpithetOrder?.Clear();
		Titles?.Clear();
		TitleOrder?.Clear();
		WithClauses?.Clear();
	}

	public void AddBase(string Base, int OrderAdjust = 0, bool Secondary = false)
	{
		Add(Base, 10 + OrderAdjust);
		if (PrimaryBase == null && !Secondary)
		{
			PrimaryBase = Base;
		}
	}

	public void ReplacePrimaryBase(string Base, int OrderAdjust = 0)
	{
		if (PrimaryBase != null)
		{
			Remove(PrimaryBase);
		}
		Add(Base, 10 + OrderAdjust);
		PrimaryBase = Base;
	}

	public void AddAdjective(string Adjective, int OrderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(Adjective, -500 + OrderAdjust);
		}
	}

	public void ApplySizeAdjective(string Adjective, int Priority = 10, int OrderAdjust = 0)
	{
		if (SizeAdjective == null || Priority > SizeAdjectivePriority)
		{
			SizeAdjective = Adjective;
			SizeAdjectivePriority = Priority;
			SizeAdjectiveOrderAdjust = OrderAdjust;
		}
	}

	public void AddClause(string Clause, int OrderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(Clause, 600 + OrderAdjust);
		}
	}

	public void AddHonorific(string Honorific, int OrderAdjust = 0)
	{
		if (!BaseOnly)
		{
			AddAdjective(Honorific, 60);
		}
	}

	public void AddEpithet(string Epithet, int OrderAdjust = 0)
	{
		if (BaseOnly)
		{
			return;
		}
		if (Epithets == null)
		{
			Epithets = new List<string>();
		}
		Epithets.Add(Epithet);
		if (OrderAdjust != 0)
		{
			if (EpithetOrder == null)
			{
				EpithetOrder = new Dictionary<string, int>();
			}
			EpithetOrder[Epithet] = OrderAdjust;
		}
	}

	public void AddTitle(string Title, int OrderAdjust = 0)
	{
		if (BaseOnly)
		{
			return;
		}
		if (Titles == null)
		{
			Titles = new List<string>();
		}
		Titles.Add(Title);
		if (OrderAdjust != 0)
		{
			if (TitleOrder == null)
			{
				TitleOrder = new Dictionary<string, int>();
			}
			TitleOrder[Title] = OrderAdjust;
		}
	}

	public void AddWithClause(string Clause)
	{
		if (!BaseOnly)
		{
			if (WithClauses == null)
			{
				WithClauses = new List<string>();
			}
			WithClauses.Add(Clause);
		}
	}

	public void AddTag(string Tag, int OrderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(Tag, 1100 + OrderAdjust);
		}
	}

	public void AddMark(string Mark, int OrderAdjust = 0)
	{
		if (!BaseOnly)
		{
			Add(Mark, -800 + OrderAdjust);
		}
	}

	public void AddColor(string Color, int Priority = 0)
	{
		if (Priority >= ColorPriority)
		{
			this.Color = Color;
			ColorPriority = Priority;
		}
	}

	public void AddColor(char Color, int Priority = 0)
	{
		if (Priority >= ColorPriority)
		{
			this.Color = Color.ToString() ?? "";
			ColorPriority = Priority;
		}
	}

	public void Reset()
	{
		Clear();
		Object = null;
		Cutoff = int.MaxValue;
		Color = null;
		ColorPriority = int.MinValue;
		SizeAdjectivePriority = int.MinValue;
		SizeAdjectiveOrderAdjust = 0;
		BaseOnly = false;
	}

	private int SortEpithets(string A, string B)
	{
		if (EpithetOrder != null && EpithetOrder.Count > 0)
		{
			EpithetOrder.TryGetValue(A, out var value);
			EpithetOrder.TryGetValue(B, out var value2);
			int num = value.CompareTo(value2);
			if (num != 0)
			{
				return num;
			}
		}
		return ColorUtility.CompareExceptFormattingAndCase(A, B);
	}

	private int SortTitles(string A, string B)
	{
		if (TitleOrder != null && TitleOrder.Count > 0)
		{
			TitleOrder.TryGetValue(A, out var value);
			TitleOrder.TryGetValue(B, out var value2);
			int num = value.CompareTo(value2);
			if (num != 0)
			{
				return num;
			}
		}
		return ColorUtility.CompareExceptFormattingAndCase(A, B);
	}

	public void Resolve()
	{
		if (!SizeAdjective.IsNullOrEmpty())
		{
			AddAdjective(SizeAdjective, SizeAdjectiveOrderAdjust);
			SizeAdjective = null;
			SizeAdjectivePriority = int.MinValue;
			SizeAdjectiveOrderAdjust = 0;
		}
		if (Epithets != null && Epithets.Count > 0)
		{
			string text;
			if (Epithets.Count > 1)
			{
				Epithets.Sort(SortEpithets);
				text = string.Join(", ", Epithets.ToArray());
			}
			else
			{
				text = Epithets[0];
			}
			AddBase(text, 60);
			Epithets.Clear();
			EpithetOrder?.Clear();
		}
		if (Titles != null && Titles.Count > 0)
		{
			string clause;
			if (Titles.Count <= 1)
			{
				clause = (((Epithets == null || Epithets.Count <= 0) && (Object == null || !Object.HasProperName)) ? ("and " + Titles[0]) : (", " + Titles[0]));
			}
			else
			{
				Titles.Sort(SortTitles);
				clause = ", " + Grammar.MakeAndList(Titles, Serial: false);
			}
			AddClause(clause, -60);
			Titles.Clear();
			TitleOrder?.Clear();
		}
		if (WithClauses != null && WithClauses.Count > 0)
		{
			if (WithClauses.Count > 1)
			{
				ColorUtility.SortExceptFormattingAndCase(WithClauses);
			}
			AddClause("with " + Grammar.MakeAndList(WithClauses));
			WithClauses.Clear();
		}
	}

	public int descOrderComparison(string a, string b)
	{
		int num = base[a].CompareTo(base[b]);
		if (num != 0)
		{
			return num;
		}
		return ColorUtility.CompareExceptFormattingAndCase(a, b);
	}

	public override string ToString()
	{
		Resolve();
		switch (Count)
		{
		case 0:
			return "";
		case 1:
			if (Color.IsNullOrEmpty())
			{
				return LastAdded;
			}
			SB.Clear();
			SB.Append("{{").Append(Color).Append('|')
				.Append(LastAdded)
				.Append("}}");
			return SB.ToString();
		default:
		{
			Descs.Clear();
			Descs.AddRange(Keys);
			if (Descs.Count > 1)
			{
				Descs.Sort(descOrderComparison);
			}
			SB.Clear();
			bool flag = false;
			if (!Color.IsNullOrEmpty())
			{
				SB.Append("{{").Append(Color).Append('|');
				flag = true;
			}
			int i = 0;
			for (int count = Descs.Count; i < count; i++)
			{
				string text = Descs[i];
				if (i > 0)
				{
					if (flag && base[text] > 600)
					{
						SB.Append("}}");
						flag = false;
					}
					if (text.Length < 1 || (text[0] != ':' && text[0] != ',' && text[0] != '-'))
					{
						SB.Append(' ');
					}
				}
				SB.Append(text);
			}
			if (flag)
			{
				SB.Append("}}");
				flag = false;
			}
			return SB.ToString();
		}
		}
	}

	public string GetDebugInfo()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		bool flag = true;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, int> current = enumerator.Current;
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(current.Key).Append(':').Append(current.Value);
			}
		}
		stringBuilder.Append(";Cutoff:").Append(Cutoff).Append(";PrimaryBase:")
			.Append(PrimaryBase)
			.Append(";LastAdded:")
			.Append(LastAdded)
			.Append(";Color:")
			.Append(Color)
			.Append(";ColorPriority:")
			.Append(ColorPriority)
			.Append(";SizeAdjective:")
			.Append(SizeAdjective)
			.Append(";SizeAdjectivePriority:")
			.Append(SizeAdjectivePriority)
			.Append(";SizeAdjectiveOrderAdjust:")
			.Append(SizeAdjectiveOrderAdjust)
			.Append(";BaseOnly:")
			.Append(BaseOnly);
		if (Epithets != null && Epithets.Count > 0)
		{
			stringBuilder.Append(";Epithets:");
			bool flag2 = true;
			foreach (string epithet in Epithets)
			{
				if (flag2)
				{
					flag2 = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(epithet);
			}
		}
		if (Titles != null && Titles.Count > 0)
		{
			stringBuilder.Append(";Titles:");
			bool flag3 = true;
			foreach (string title in Titles)
			{
				if (flag3)
				{
					flag3 = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(title);
			}
		}
		if (WithClauses != null && WithClauses.Count > 0)
		{
			stringBuilder.Append(";WithClauses:");
			bool flag4 = true;
			foreach (string withClause in WithClauses)
			{
				if (flag4)
				{
					flag4 = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(withClause);
			}
		}
		return stringBuilder.ToString();
	}
}
