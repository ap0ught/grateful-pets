using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using XRL.World;
using XRL.World.Skills;

namespace XRL.UI;

public class SPNode
{
	public enum LearnedStatus
	{
		None,
		Partial,
		Learned
	}

	public Renderable _UIIcon;

	public SkillEntry Skill;

	public PowerEntry Power;

	public bool _Expand;

	public SPNode ParentNode;

	public string _Description;

	public string _SearchText;

	private static StringBuilder sb = new StringBuilder(80);

	public Renderable UIIcon
	{
		get
		{
			if (_UIIcon == null)
			{
				if (Skill != null)
				{
					_UIIcon = new Renderable(Skill.Tile, " ", "", "&" + Skill.Foreground, Skill.Detail[0]);
				}
				else if (Power != null)
				{
					_UIIcon = new Renderable(Power.Tile, " ", "", "&" + Power.Foreground, Power.Detail[0]);
				}
			}
			return _UIIcon;
		}
	}

	public IEnumerable<SPNode> powers
	{
		get
		{
			if (Skill == null)
			{
				yield break;
			}
			foreach (SPNode item in SkillsAndPowersScreen.Nodes.Where((SPNode n) => n.ParentNode == this))
			{
				yield return item;
			}
		}
	}

	public bool Expand
	{
		get
		{
			return _Expand;
		}
		set
		{
			_Expand = value;
			if (Skill != null && Skill.Name != null)
			{
				The.Game?.SetBooleanGameState("UI_Expand_Skill_" + Name, value);
			}
		}
	}

	public string Description
	{
		get
		{
			if (_Description == null)
			{
				if (Skill != null)
				{
					_Description = Skill.GetFormattedDescription();
				}
				else
				{
					_Description = Power.GetFormattedDescription();
				}
			}
			return _Description;
		}
	}

	public string Name
	{
		get
		{
			if (Skill != null)
			{
				return Skill.Name;
			}
			return Power.Name;
		}
	}

	public string SearchText
	{
		get
		{
			if (_SearchText == null)
			{
				if (Skill != null)
				{
					_SearchText = Skill.Name + " " + Skill.Description;
				}
				if (Power != null)
				{
					_SearchText = Power.Name + " " + Power.Description;
				}
				_SearchText = _SearchText?.ToLower();
			}
			return _SearchText;
		}
	}

	public bool Visible
	{
		get
		{
			if (ParentNode != null)
			{
				return ParentNode.Expand;
			}
			return true;
		}
	}

	public LearnedStatus IsLearned(GameObject go)
	{
		if (Power != null)
		{
			if (!go.HasPart(Power.Class))
			{
				return LearnedStatus.None;
			}
			return LearnedStatus.Learned;
		}
		if (Skill != null)
		{
			if (go.HasPart(Skill.Class))
			{
				return LearnedStatus.Learned;
			}
			if (SkillsAndPowersScreen.HasAnyPower(go, Skill))
			{
				return LearnedStatus.Partial;
			}
			return LearnedStatus.None;
		}
		return LearnedStatus.None;
	}

	public string ModernUIText(GameObject GO)
	{
		sb.Length = 0;
		if (Skill != null)
		{
			bool flag = SkillsAndPowersScreen.HasAnyPower(GO, Skill);
			if (GO.HasPart(Skill.Class))
			{
				sb.Append("{{W|" + Skill.Name + "}}");
			}
			else if (Skill.Initiatory)
			{
				sb.Append("{{w|" + Skill.Name + "}}");
			}
			else if (flag)
			{
				if (Skill.Cost <= GO.Stat("SP"))
				{
					sb.Append("{{W|" + Skill.Name + "}} [{{C|" + Skill.Cost + "}}sp]");
				}
				else
				{
					sb.Append("{{W|" + Skill.Name + "}} [{{R|" + Skill.Cost + "}}sp]");
				}
			}
			else if (Skill.Cost <= GO.Stat("SP"))
			{
				sb.Append("{{w|" + Skill.Name + "}} [{{C|" + Skill.Cost + "}}sp]");
			}
			else
			{
				sb.Append("{{w|" + Skill.Name + "}} [{{R|" + Skill.Cost + "}}sp]");
			}
		}
		else
		{
			sb.Append("    ");
			string text = "";
			PowerEntry power = Power;
			if (GO.HasPart(power.Class))
			{
				sb.Append("{{G|:" + power.Name + "}}");
			}
			else
			{
				if (power.Requires != null)
				{
					foreach (string item in power.Requires.CachedCommaExpansion())
					{
						string text2 = item;
						bool flag2 = false;
						if (SkillFactory.Factory.TryGetFirstEntry(item, out var Entry))
						{
							if (power.IsSkillInitiatory)
							{
								int num = power.ParentSkill.PowerList.IndexOf(power);
								if (num > 0 && power.ParentSkill.PowerList[num - 1] == Entry)
								{
									continue;
								}
							}
							text2 = Entry.Name;
							flag2 = GO.HasSkill(item);
						}
						else if (MutationFactory.HasMutation(item))
						{
							text2 = MutationFactory.GetMutationEntryByName(item).Name;
							flag2 = GO.HasPart(item);
						}
						text = ((!flag2) ? (text + ", {{R|" + text2 + "}}") : (text + ", {{G|" + text2 + "}}"));
					}
				}
				if (power.Exclusion != null)
				{
					foreach (string item2 in power.Exclusion.CachedCommaExpansion())
					{
						string text3 = item2;
						bool flag3 = false;
						if (SkillFactory.Factory.TryGetFirstEntry(item2, out var Entry2))
						{
							text3 = Entry2.Name;
							flag3 = !GO.HasSkill(item2);
						}
						else if (MutationFactory.HasMutation(item2))
						{
							text3 = MutationFactory.GetMutationEntryByName(item2).Name;
							flag3 = !GO.HasPart(item2);
						}
						text = ((!flag3) ? (text + ", Ex: {{R|" + text3 + "}}") : (text + ", Ex: {{g|" + text3 + "}}"));
					}
				}
				sb.Append("{{K|:}}");
				sb.Append(power.Render(GO));
				sb.Append(text);
			}
		}
		return sb.ToString();
	}

	public SPNode(SkillEntry Skill, PowerEntry Power, bool Expand, SPNode ParentNode)
	{
		this.Skill = Skill;
		this.Power = Power;
		this.Expand = Expand;
		this.ParentNode = ParentNode;
	}
}
