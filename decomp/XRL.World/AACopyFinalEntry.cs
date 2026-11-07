using System;

namespace XRL.World;

public class AACopyFinalEntry
{
	public Guid ID;

	public IPart Part;

	public AACopyFinalEntry(Guid _ID, IPart _Part)
	{
		ID = _ID;
		Part = _Part;
	}
}
