using ConsoleLib.Console;
using XRL.Language;

namespace XRL.UI;

public class CyberneticsScreen : TerminalScreen
{
	public override void BeforeRender(ScreenBuffer buffer, ref string footerText)
	{
		footerText = $" Credits: {Terminal.Credits}  License Tier: {Terminal.Licenses}  Points Used: {Terminal.LicensesUsed}";
		if (buffer != null)
		{
			int num = 3;
			buffer.Goto(num + 3, 24);
			buffer.Write(" Credits: " + Terminal.Credits + " ");
			buffer.Goto(num + 38, 24);
			buffer.Write(" License Tier: " + Terminal.Licenses + " ");
			buffer.Write(" Points Used: " + Terminal.LicensesUsed + " ");
			bool hackActive = Terminal.HackActive;
			int hackLevel = Terminal.HackLevel;
			bool lowLevelHack = Terminal.LowLevelHack;
			if (hackActive || hackLevel > 0)
			{
				string text = ((hackActive & lowLevelHack) ? TextFilters.Leet("HACK LEVEL") : "Hack Level");
				buffer.Goto(num + 3, 2);
				buffer.Write(" {{G|" + text + ": " + hackLevel + "}} ");
			}
			int securityAlertLevel = Terminal.SecurityAlertLevel;
			if (hackActive || securityAlertLevel > 0)
			{
				string text2;
				if (securityAlertLevel <= 0)
				{
					text2 = "y";
				}
				else if (securityAlertLevel >= hackLevel - 1)
				{
					text2 = "R";
				}
				else
				{
					text2 = ((securityAlertLevel > hackLevel - 3) ? "r" : "W");
				}
				string text3 = ((hackActive & lowLevelHack) ? TextFilters.Leet("SECURITY ALERT LEVEL") : "Security Alert Level");
				buffer.Goto(72 - num - text3.StrippedLength(), 2);
				buffer.Write(" {{" + text2 + "|" + text3 + ": " + securityAlertLevel + "}} ");
			}
		}
	}
}
