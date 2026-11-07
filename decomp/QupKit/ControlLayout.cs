using UnityEngine;

namespace QupKit;

public class ControlLayout
{
	public BaseControl Parent;

	public ControlAnchor Anchor;

	public Vector3 Offset;

	public LayoutSides Margin = new LayoutSides();

	public ControlLayout()
	{
		Anchor = ControlAnchor.Custom;
		Offset = new Vector3(0f, 0f, 0f);
	}

	public ControlLayout(ControlLayout Source)
	{
		Parent = null;
		Anchor = Source.Anchor;
		Offset = Source.Offset;
	}

	public ControlLayout(ControlAnchor Anchor)
	{
		this.Anchor = Anchor;
		Offset = new Vector3(0f, 0f, 0f);
	}

	public ControlLayout(ControlAnchor Anchor, Vector3 Offset)
	{
		this.Anchor = Anchor;
		this.Offset = Offset;
	}

	public void Apply()
	{
		Parent.ApplyLayout();
	}
}
