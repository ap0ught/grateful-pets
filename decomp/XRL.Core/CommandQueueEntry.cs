using System;
using XRL.World;

namespace XRL.Core;

[Serializable]
[Obsolete]
public class CommandQueueEntry : IComposite
{
	public string Action;

	public object Target;

	public int SegmentDelay;
}
