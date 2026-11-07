using System;

namespace XRL.World.Parts;

[Serializable]
public class ImportantOverlayObject : IPart
{
	public override void Attach()
	{
		ParentObject.Flags |= 256;
	}
}
