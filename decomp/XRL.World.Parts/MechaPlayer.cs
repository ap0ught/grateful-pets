using System;
using System.Collections.Generic;
using XRL.World.Anatomy;
using XRL.World.ObjectBuilders;

namespace XRL.World.Parts;

[Serializable]
public class MechaPlayer : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 90;

	public string NamePrefix = "{{c|mechanical}}";

	public string DescriptionPostfix = "There is a low, persistent hum emanating outward.";

	public bool KeepNatural = true;

	public bool InheritDescription = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		GameObject thePlayer = IComponent<GameObject>.ThePlayer;
		if (thePlayer == null)
		{
			return true;
		}
		ParentObject.Render.DisplayName = thePlayer.Render.DisplayName;
		ParentObject.Render.Tile = thePlayer.Render.Tile;
		ParentObject.Render.RenderString = thePlayer.Render.RenderString;
		ParentObject.Render.DetailColor = thePlayer.Render.DetailColor;
		Body body = ParentObject.Body;
		body.ForeachPart((BodyPart p) => p.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", p, "IsSilent", 1)));
		try
		{
			ParentObject.DeepCopyInventoryObjectMap = new Dictionary<GameObject, GameObject>();
			Body body2 = thePlayer.Body.DeepCopy(ParentObject, null) as Body;
			ParentObject.DeepCopyInventoryObjectMap = null;
			ParentObject.RemovePart(body);
			ParentObject.AddPart(body2);
			body = body2;
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error copying player body, setting anatomy instead", x);
			if (thePlayer.Body.Anatomy != body.Anatomy)
			{
				body.Anatomy = thePlayer.Body.Anatomy;
			}
		}
		body.HandleEvent(StripContentsEvent.FromPool(ParentObject, KeepNatural, Silent: true));
		body.MarkAllNative();
		Description part = thePlayer.GetPart<Description>();
		if (InheritDescription && part != null)
		{
			ParentObject.RequirePart<Description>()._Short = part._Short;
		}
		XRL.World.ObjectBuilders.Roboticized.Roboticize(ParentObject, NamePrefix, DescriptionPostfix);
		ParentObject.Brain.DoReequip = true;
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors("&c", ICON_COLOR_PRIORITY);
		return base.Render(E);
	}
}
