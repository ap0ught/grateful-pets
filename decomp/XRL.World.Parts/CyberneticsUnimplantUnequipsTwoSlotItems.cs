using System;

namespace XRL.World.Parts;

[Serializable]
[Obsolete]
public class CyberneticsUnimplantUnequipsTwoSlotItems : IPart
{
	public override void Attach()
	{
		ParentObject.RemovePart(this);
	}
}
