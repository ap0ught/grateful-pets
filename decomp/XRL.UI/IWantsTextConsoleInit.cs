using ConsoleLib.Console;

namespace XRL.UI;

/// <summary>
/// If implemented by a class with <c>UIView</c> it will call Init passing the TextConsole and ScreenBuffer at load time.
/// </summary>
public interface IWantsTextConsoleInit
{
	void Init(TextConsole console, ScreenBuffer buffer);
}
