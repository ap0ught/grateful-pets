using XRL.UI.Framework;
using XRL.World.Anatomy;

namespace Qud.UI;

public class EquipmentLineData : PooledFrameworkDataElement<EquipmentLineData>
{
	public bool showCybernetics;

	public BodyPart bodyPart;

	public InventoryAndEquipmentStatusScreen screen;

	public EquipmentLine line;

	public HotkeySpread spread;

	public EquipmentLineData set(bool showCybernetics, BodyPart bodyPart, InventoryAndEquipmentStatusScreen screen, HotkeySpread spread)
	{
		this.showCybernetics = showCybernetics;
		this.bodyPart = bodyPart;
		this.screen = screen;
		this.spread = spread;
		return this;
	}

	public override void free()
	{
		screen = null;
		bodyPart = null;
		showCybernetics = false;
		line = null;
		spread = null;
		base.free();
	}
}
