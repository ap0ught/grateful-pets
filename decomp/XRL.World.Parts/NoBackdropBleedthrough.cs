namespace XRL.World.Parts;

public class NoBackdropBleedthrough : IPart
{
	public string WantsBackdrop = "backdropBlack";

	public string ZoneFilter;

	public override void Initialize()
	{
		base.Initialize();
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (string.IsNullOrEmpty(ZoneFilter) || ParentObject?.CurrentZone?.ZoneID == ZoneFilter)
		{
			E.WantsBackdrop = WantsBackdrop;
			E.BackdropBleedthrough = false;
		}
		return false;
	}
}
