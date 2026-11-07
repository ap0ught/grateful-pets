using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Skills;

[Serializable]
public class PowerEntry : IBaseSkillEntry
{
	public string Minimum;

	public string Requires;

	public string Exclusion;

	public SkillEntry ParentSkill;

	[NonSerialized]
	private List<PowerEntryRequirement> _requirements;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	public List<PowerEntryRequirement> requirements
	{
		get
		{
			InitRequirements();
			return _requirements;
		}
	}

	public bool IsSkillInitiatory => ParentSkill?.Initiatory ?? false;

	public void InitRequirements()
	{
		if (_requirements != null)
		{
			return;
		}
		_requirements = new List<PowerEntryRequirement>();
		if (Attribute.IsNullOrEmpty() || Minimum.IsNullOrEmpty())
		{
			return;
		}
		DelimitedEnumeratorChar delimitedEnumeratorChar = Attribute.DelimitedBy('|');
		DelimitedEnumeratorChar delimitedEnumeratorChar2 = Minimum.DelimitedBy('|');
		while (delimitedEnumeratorChar.MoveNext() && delimitedEnumeratorChar2.MoveNext())
		{
			PowerEntryRequirement powerEntryRequirement = new PowerEntryRequirement(this);
			DelimitedEnumeratorChar delimitedEnumeratorChar3 = delimitedEnumeratorChar.Current.DelimitedBy(',');
			DelimitedEnumeratorChar delimitedEnumeratorChar4 = delimitedEnumeratorChar2.Current.DelimitedBy(',');
			while (delimitedEnumeratorChar3.MoveNext() && delimitedEnumeratorChar4.MoveNext())
			{
				powerEntryRequirement.Attributes.Add(new string(delimitedEnumeratorChar3.Current));
				powerEntryRequirement.Minimums.Add(int.Parse(delimitedEnumeratorChar4.Current));
			}
			_requirements.Add(powerEntryRequirement);
		}
	}

	public override bool MeetsRequirements(GameObject Object, bool ShowPopup = false)
	{
		if (!MeetsAttributeMinimum(Object, ShowPopup))
		{
			return false;
		}
		if (!Exclusion.IsNullOrEmpty())
		{
			foreach (string item in Exclusion.CachedCommaExpansion())
			{
				MutationEntry Entry2;
				if (SkillFactory.Factory.TryGetFirstEntry(item, out var Entry))
				{
					if (Object.HasSkill(item))
					{
						if (ShowPopup)
						{
							Popup.Show("You may not learn this skill if you already have " + Entry.Name + ".");
						}
						return false;
					}
				}
				else if (MutationFactory.TryGetMutationEntry(item, out Entry2) && Object.HasPart(Entry2.Class))
				{
					if (ShowPopup)
					{
						Popup.Show("You may not learn this skill if you have " + Entry2.GetDisplayName() + ".");
					}
					return true;
				}
			}
		}
		if (!Requires.IsNullOrEmpty())
		{
			foreach (string item2 in Requires.CachedCommaExpansion())
			{
				MutationEntry Entry4;
				if (SkillFactory.Factory.TryGetFirstEntry(item2, out var Entry3))
				{
					if (!Object.HasSkill(item2))
					{
						if (ShowPopup)
						{
							Popup.Show("You may not learn this skill until you have " + Entry3.Name + ".");
						}
						return false;
					}
				}
				else if (MutationFactory.TryGetMutationEntry(item2, out Entry4) && !Object.HasPart(Entry4.Class))
				{
					if (ShowPopup)
					{
						Popup.Show("You may not learn this skill until you have " + Entry4.GetDisplayName() + ".");
					}
					return false;
				}
			}
		}
		return Generic.MeetsRequirements(Object, ShowPopup);
	}

	public bool MeetsAttributeMinimum(GameObject Object, bool ShowPopup = false)
	{
		if (Attribute.IsNullOrEmpty() || Minimum.IsNullOrEmpty())
		{
			return true;
		}
		InitRequirements();
		for (int num = requirements.Count - 1; num >= 0; num--)
		{
			if (_requirements[num].MeetsRequirement(Object, ShowPopup && num == 0))
			{
				return true;
			}
		}
		return false;
	}

	public string Render(GameObject GO)
	{
		if (SB == null)
		{
			SB = new StringBuilder();
		}
		else
		{
			SB.Clear();
		}
		if (MeetsRequirements(GO))
		{
			if (IsSkillInitiatory)
			{
				SB.Append("{{g|" + Name + "}}");
			}
			else if (Cost <= GO.Stat("SP"))
			{
				SB.Append("{{g|" + Name + "}} [{{C|" + Cost + "}}sp]");
			}
			else
			{
				SB.Append("{{g|" + Name + "}} [{{R|" + Cost + "}}sp]");
			}
		}
		else if (IsSkillInitiatory)
		{
			SB.Append("{{K|" + Name + "}}");
		}
		else
		{
			SB.Append("{{K|" + Name + "}} [{{K|" + Cost + "}}sp]");
		}
		InitRequirements();
		int num = 0;
		foreach (PowerEntryRequirement requirement in requirements)
		{
			if (num == 0)
			{
				SB.Append(' ');
			}
			else
			{
				SB.Append(" or ");
			}
			num++;
			requirement.Render(GO, SB);
		}
		return SB.ToString();
	}

	public override void HandleXMLNode(XmlDataHelper Reader)
	{
		base.HandleXMLNode(Reader);
		Exclusion = Reader.ParseAttribute("Exclusion", Exclusion);
		Minimum = Reader.ParseAttribute("Minimum", Minimum);
		Requires = Reader.ParseAttribute("Prereq", Requires);
		Requires = Reader.ParseAttribute("Requires", Requires);
		Tile = Reader.ParseAttribute<string>("Tile", null);
		Foreground = Reader.ParseAttribute("Foreground", "w");
		Detail = Reader.ParseAttribute("Detail", "B");
		if (Requires.IsNullOrEmpty())
		{
			return;
		}
		if (Requires.Contains(","))
		{
			string[] array = Requires.Split(',');
			bool flag = false;
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				string text = CompatManager.ProcessSkill(array[i]);
				if (text != array[i])
				{
					array[i] = text;
					flag = true;
				}
			}
			if (flag)
			{
				Requires = string.Join(",", array);
			}
		}
		else
		{
			CompatManager.ProcessSkill(ref Requires);
		}
	}
}
