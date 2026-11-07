using System;

namespace XRL.World.Parts;

[Serializable]
public class BackdropWhenVisible : IPart
{
	public string Backdrop;

	public bool BackdropBleedthrough;

	public override bool SameAs(IPart p)
	{
		if ((p as WantsBackdrop).Backdrop != Backdrop)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (ParentObject.IsVisible())
		{
			E.WantsBackdrop = Backdrop;
			E.BackdropBleedthrough = BackdropBleedthrough;
		}
		return base.FinalRender(E);
	}
}
