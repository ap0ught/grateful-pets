using System;
using Cysharp.Text;
using XRL.World.Text;

namespace ConsoleLib.Console;

public abstract class IZMarkupNode
{
	public abstract void release();

	public abstract void ToTextBuilder(TextBuilder SB, ReadOnlySpan<char> Source, bool RefreshAtNewline, ref char? lastForeground, ref char? lastBackground);

	public abstract void ToTextBuilder(TextBuilder SB, ReadOnlySpan<char> Source, bool RefreshAtNewline);

	public abstract string DebugDump(int depth = 0);

	public abstract void DebugDump(ref Utf16ValueStringBuilder SB, int depth);
}
