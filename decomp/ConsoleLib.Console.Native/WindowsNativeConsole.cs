using System.Runtime.InteropServices;

namespace ConsoleLib.Console.Native;

public static class WindowsNativeConsole
{
	[DllImport("Kernel32.dll")]
	private static extern bool AllocConsole();

	[DllImport("msvcrt.dll")]
	public static extern int system(string cmd);

	public static void Open()
	{
		AllocConsole();
	}

	public static void ClearAllocConsole()
	{
		system("CLS");
	}
}
