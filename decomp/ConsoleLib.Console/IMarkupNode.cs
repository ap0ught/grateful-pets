using System.Text;
using Cysharp.Text;

namespace ConsoleLib.Console;

public abstract class IMarkupNode
{
	public abstract void release();

	public abstract void ToStringBuilder(StringBuilder SB, bool RefreshAtNewline, ref char? lastForeground, ref char? lastBackground);

	public abstract void ToStringBuilder(StringBuilder SB, bool RefreshAtNewline);

	public abstract string ToString(bool RefreshAtNewline);

	public abstract string DebugDump(int depth = 0);

	public abstract void DebugDump(ref Utf16ValueStringBuilder SB, int depth);
}
