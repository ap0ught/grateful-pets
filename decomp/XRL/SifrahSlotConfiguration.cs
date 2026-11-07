namespace XRL;

/// This class is not used in the base game.
public class SifrahSlotConfiguration : SifrahRenderable
{
	public string Description;

	public int UptakeOrder;

	public SifrahSlotConfiguration()
	{
	}

	public SifrahSlotConfiguration(string Description, string Tile, string RenderString, string ColorString, char DetailColor, int UptakeOrder = 0)
	{
		this.Description = Description;
		base.Tile = Tile;
		base.RenderString = RenderString;
		base.ColorString = ColorString;
		base.DetailColor = DetailColor;
		this.UptakeOrder = UptakeOrder;
	}

	public SifrahSlotConfiguration(SifrahSlotConfiguration From)
	{
		Description = From.Description;
		Tile = From.Tile;
		RenderString = From.RenderString;
		ColorString = From.ColorString;
		DetailColor = From.DetailColor;
		UptakeOrder = From.UptakeOrder;
	}
}
