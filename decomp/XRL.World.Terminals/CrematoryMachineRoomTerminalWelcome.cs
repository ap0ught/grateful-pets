using System.Text;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Terminals;

public class CrematoryMachineRoomTerminalWelcome : GenericTerminalScreen
{
	public CrematoryMachineRoomTerminalWelcome()
	{
		mainText = "                            \r\n            .   .          .                                       \r\n      .     |\\  |  o       |                                .      \r\n    __|__   | \\ |  .  .--. |--. .  . .--..--. .-.  .-..   __|__     \r\n      |     |  \\|  |  |  | |  | |  | |   `--.(   )(   |     |       \r\n      '     '   '-' `-'  `-'  `-`--`-'   `--' `-'`-`-`|     '   \r\n                                                   ._.'           \r\n                                                                      ";
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < mainText.Length; i++)
		{
			if (Stat.RandomCosmetic(1, 100) <= 2)
			{
				stringBuilder.Append((char)Stat.RandomCosmetic(45, 230));
			}
			else
			{
				stringBuilder.Append(mainText[i]);
			}
		}
		mainText = stringBuilder.ToString();
		Options.Add("...");
	}

	public override void Back()
	{
		terminal.currentScreen = null;
	}

	public override void Activate()
	{
		terminal.currentScreen = null;
	}
}
