using System;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

/// <summary>
///   Data Structure for various UI elements to use.
/// </summary>
[Serializable]
public class AttributeDataElement : FrameworkDataElement
{
	public AttributeSelectionControl control;

	public QudAttributesModuleWindow window;

	public string Attribute;

	public string ShortAttributeName;

	public int BaseValue;

	public int Minimum;

	public int Maximum;

	public int Bonus;

	public string BonusSource = "";

	public int Purchased;

	public int Value => BaseValue + Bonus + Purchased;

	public int APRemaining => window.apRemaining;

	public int APSpent
	{
		get
		{
			int num = Math.Min(Purchased, Math.Max(0, BaseValue + Purchased - 18));
			return 2 * num + Math.Max(0, Purchased - num);
		}
	}

	public int APToRaise
	{
		get
		{
			if (Value - Bonus >= 18)
			{
				return 2;
			}
			return 1;
		}
	}

	public void Updated()
	{
		window.AttributeUpdated(this);
	}

	public void AddBonus(int bonus, string source)
	{
		Bonus += bonus;
		BonusSource += $"{bonus:+#;-#;0} from {source}\n";
	}

	public void raise()
	{
		if (window.apRemaining >= APToRaise && Value - Bonus < Maximum)
		{
			Purchased++;
			Updated();
		}
	}

	public void lower()
	{
		if (Purchased > 0 && Value - Bonus > Minimum)
		{
			Purchased--;
			Updated();
		}
	}
}
