using System;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsScreenUpgrade : CyberneticsScreen
{
	public int GetUpgradeCost()
	{
		int num = Terminal.Licenses - Terminal.FreeLicenses;
		if (num < 8)
		{
			return 1;
		}
		if (num < 16)
		{
			return 2;
		}
		if (num < 24)
		{
			return 3;
		}
		return 4;
	}

	protected override void OnUpdate()
	{
		ClearOptions();
		MainText = "Become a finer Aristocrat. Upgrade your license tier with cybernetics credits.\n\n{{C|1}} credit for license tiers 1-8\n{{C|2}} credits for license tiers 9-16\n{{C|3}} credits for license tiers 17-24\n{{C|4}} credits for license tiers 25+\n";
		if (Terminal.FreeLicenses > 0)
		{
			MainText = MainText + "\nRemember, Aristocrat, your base license tier is {{C|" + (Terminal.Licenses - Terminal.FreeLicenses) + "}}.";
		}
		int upgradeCost = GetUpgradeCost();
		string text = "Upgrade Your License [{{C|" + GetUpgradeCost() + "}} " + ((upgradeCost == 1) ? "credit" : "credits") + "]";
		if (Terminal.Credits < GetUpgradeCost())
		{
			text += " {{R|insufficent credits}}";
		}
		Options.Add(text);
		Options.Add("Return To Main Menu");
	}

	public override void Back()
	{
		Terminal.CheckSecurity(1, new CyberneticsScreenMainMenu());
	}

	public override void Activate()
	{
		if (Terminal.Selected == 0)
		{
			if (Terminal.Credits < GetUpgradeCost())
			{
				Terminal.CurrentScreen = new CyberneticsScreenSimpleText("{{R|Insufficient credits to upgrade}}", new CyberneticsScreenUpgrade());
				return;
			}
			int num = GetUpgradeCost();
			int i = 0;
			for (int count = Terminal.Wedges.Count; i < count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				CyberneticsCreditWedge cyberneticsCreditWedge = Terminal.Wedges[i];
				if (cyberneticsCreditWedge.ParentObject == null || !cyberneticsCreditWedge.ParentObject.IsValid())
				{
					continue;
				}
				int count2 = cyberneticsCreditWedge.ParentObject.Count;
				int credits = cyberneticsCreditWedge.Credits;
				int num2 = credits * count2;
				if (num2 > num)
				{
					int num3 = 0;
					int num4 = 0;
					while (num >= credits && num3 < count2)
					{
						cyberneticsCreditWedge.ParentObject.Destroy();
						num -= credits;
						num3++;
						if (++num4 >= 10000)
						{
							throw new Exception("infinite loop in license upgrade wedge use");
						}
					}
					if (num > 0 && num3 < count2)
					{
						cyberneticsCreditWedge.ParentObject.SplitStack(1);
						cyberneticsCreditWedge.Credits -= num;
						cyberneticsCreditWedge.ParentObject.CheckStack();
						num = 0;
					}
				}
				else
				{
					num -= num2;
					cyberneticsCreditWedge.ParentObject.Obliterate();
				}
			}
			SoundManager.PlayUISound("sfx_cybernetic_terminal_license_upgrade");
			Terminal.Subject.ModIntProperty("CyberneticsLicenses", 1);
			Terminal.CurrentScreen = new CyberneticsScreenSimpleText("You are Becoming, Aristocrat.", new CyberneticsScreenUpgrade(), 5);
		}
		else
		{
			Terminal.CheckSecurity(1, new CyberneticsScreenMainMenu());
		}
	}
}
