using System;
using XRL.World;

namespace XRL.Core;

public struct ActionCommandEntry(Guid ID, IActionCommand Command, int SegmentDelay) : IComposite
{
	public Guid ID = ID;

	public IActionCommand Command = Command;

	public int SegmentDelay = SegmentDelay;

	public ActionCommandEntry(IActionCommand Action, int SegmentDelay)
		: this(Guid.NewGuid(), Action, SegmentDelay)
	{
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.Write(ID);
		Writer.Write(Command);
		Writer.WriteOptimized(SegmentDelay);
	}

	public void Read(SerializationReader Reader)
	{
		ID = Reader.ReadGuid();
		Command = Reader.ReadComposite() as IActionCommand;
		SegmentDelay = Reader.ReadOptimizedInt32();
	}
}
