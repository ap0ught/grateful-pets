using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Qud.UI;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
public abstract class GolemMaterialSelection<M, S> : GolemQuestSelection
{
	public delegate IEnumerable<GameObjectUnit> UnitGenerator(M Material);

	[NonSerialized]
	public static Dictionary<S, UnitGenerator> Units;

	public abstract M Material { get; set; }

	public override bool IsValid()
	{
		return IsValid(Material);
	}

	public virtual bool IsValid(M Material)
	{
		return Material != null;
	}

	public abstract List<M> GetValidMaterials();

	public abstract string GetNameFor(M Material);

	public abstract IRenderable GetIconFor(M Material);

	public virtual IEnumerable<GameObjectUnit> YieldEffectsOf(M Material)
	{
		return Enumerable.Empty<GameObjectUnit>();
	}

	public override IEnumerable<GameObjectUnit> YieldAllEffects()
	{
		if (Units == null)
		{
			yield break;
		}
		foreach (KeyValuePair<S, UnitGenerator> unit in Units)
		{
			foreach (GameObjectUnit item in unit.Value(default))
			{
				yield return item;
			}
		}
	}

	public override IEnumerable<GameObjectUnit> YieldRandomEffects()
	{
		if (Units == null)
		{
			return Enumerable.Empty<GameObjectUnit>();
		}
		S[] list = new S[Units.Count];
		list.Fill(Units.Keys);
		S randomElement = list.GetRandomElement();
		return Units[randomElement](default);
	}

	public override string GetOptionChoice()
	{
		if (Material != null)
		{
			return GetNameFor(Material);
		}
		return null;
	}

	public override void Pick()
	{
		List<M> validMaterials = GetValidMaterials();
		if (validMaterials.IsNullOrEmpty())
		{
			Popup.ShowFail("You have nothing that meets the requirement of the " + DisplayName + ".");
			return;
		}
		int num = 0;
		int num2 = 250;
		int capacity = Math.Min(validMaterials.Count, num2);
		List<QudMenuItem> list = new List<QudMenuItem>(2);
		QudMenuItem item = new QudMenuItem
		{
			text = "Previous Page",
			command = "option:-2"
		};
		QudMenuItem item2 = new QudMenuItem
		{
			text = "Next Page",
			command = "option:-3"
		};
		StringBuilder stringBuilder = Event.NewStringBuilder();
		List<string> list2 = new List<string>(capacity);
		List<IRenderable> list3 = new List<IRenderable>(capacity);
		int num4;
		while (true)
		{
			list2.Clear();
			list3.Clear();
			int num3 = Math.Min(validMaterials.Count - num, num2);
			for (int i = num; i < num + num3; i++)
			{
				stringBuilder.Clear().Append(GetNameFor(validMaterials[i]));
				AppendEffectsOf(validMaterials[i], stringBuilder);
				list2.Add(stringBuilder.ToString());
				list3.Add(GetIconFor(validMaterials[i]));
			}
			list.Clear();
			if (num > 0)
			{
				list.Add(item);
			}
			if (validMaterials.Count - num > num2)
			{
				list.Add(item2);
			}
			num4 = Popup.PickOption(Title, null, "", "Sounds/UI/ui_notification", list2, null, list3, list, null, null, null, 0, 60, 0, -1, AllowEscape: true, RespectOptionNewlines: true);
			if (num4 == -2)
			{
				num = Math.Max(num - num2, 0);
				continue;
			}
			if (num4 != -3)
			{
				break;
			}
			num += num2;
		}
		if (num4 >= 0)
		{
			Material = validMaterials[num + num4];
		}
	}

	public void AppendEffectsOf(M Material, StringBuilder SB, int Indent = 0)
	{
		if (Material == null)
		{
			return;
		}
		bool flag = true;
		foreach (GameObjectUnit item in YieldEffectsOf(Material))
		{
			string description = item.GetDescription();
			int num = 0;
			int num2 = description.IndexOf('\n');
			if (SB.Length != 0)
			{
				SB.Append('\n');
			}
			SB.Append('ÿ', Indent).Append(flag ? "{{rules|--}}" : "{{rules|OR}}").Append(" {{rules|");
			if (num2 == -1)
			{
				SB.Append(description);
			}
			else
			{
				do
				{
					SB.Append(description, num, num2 - num + 1).Append('ÿ', 3 + Indent);
					num = num2 + 1;
					num2 = description.IndexOf('\n', num);
				}
				while (num2 != -1);
				SB.Append(description, num, description.Length - num);
			}
			SB.Append("}}");
			flag = false;
		}
	}

	public override void AppendEffects(StringBuilder SB, int Indent = 0)
	{
		AppendEffectsOf(Material, SB, Indent);
	}

	public override void Apply(GameObject Object)
	{
		RulesDescription rulesDescription = Object.RequirePart<RulesDescription>();
		StringBuilder sB = Event.NewStringBuilder(rulesDescription.Text);
		if (GolemQuestSelection.WISH_ALL)
		{
			foreach (GameObjectUnit item in YieldAllEffects())
			{
				item.Apply(Object);
				AppendUnitDesc(sB, item);
			}
		}
		else if (GolemQuestSelection.WISH_RANDOM)
		{
			foreach (GameObjectUnit item2 in YieldRandomEffects())
			{
				item2.Apply(Object);
				AppendUnitDesc(sB, item2);
			}
		}
		else
		{
			GameObjectUnit randomElement = YieldEffectsOf(Material).ToArray().GetRandomElement();
			if (randomElement != null)
			{
				randomElement.Apply(Object);
				AppendUnitDesc(sB, randomElement);
			}
		}
		rulesDescription.Text = Event.FinalizeString(sB);
	}

	private void AppendUnitDesc(StringBuilder SB, GameObjectUnit Unit)
	{
		if (Unit.CanInscribe())
		{
			SB.Append('\n').Append(Unit.GetDescription(Inscription: true));
		}
	}
}
