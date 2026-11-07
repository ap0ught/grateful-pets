using System.Buffers;
using System.Diagnostics;
using XRL.UI;

namespace XRL.World.Text;

public class CompositedTextBuilder : TextBuilder
{
	protected override bool Finalizable => Buffer == null;

	public CompositedTextBuilder(int Capacity = 64)
	{
		Buffer = ArrayPool<char>.Shared.Rent(Capacity);
		if (Options.TextBuilderDebug)
		{
			TextBuilder.Traces[this] = new StackTrace(1, fNeedFileInfo: true);
		}
	}

	public override void Dispose(bool Destroy)
	{
		if (Destroy)
		{
			base.Dispose(Destroy: true);
		}
		Length = 0;
	}
}
