using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MoteProperties : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 50;

	public int Fuel = 5000;

	public int FrameOffset;

	[NonSerialized]
	private LightSource _pLight;

	public LightSource pLight
	{
		get
		{
			if (_pLight == null)
			{
				_pLight = ParentObject.GetPart<LightSource>();
			}
			return _pLight;
		}
	}

	public override bool SameAs(IPart p)
	{
		if ((p as MoteProperties).Fuel != Fuel)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != AfterThrownEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		LightSource lightSource = pLight;
		if (lightSource != null && lightSource.Lit)
		{
			if (Fuel > 3500)
			{
				pLight.Radius = 6;
			}
			else if (Fuel > 2000)
			{
				pLight.Radius = 5;
			}
			else if (Fuel > 500)
			{
				pLight.Radius = 4;
			}
			else
			{
				pLight.Radius = 3;
			}
			if (Fuel > 0)
			{
				Fuel--;
			}
			if (Fuel <= 0)
			{
				GameObject equipped = ParentObject.Equipped;
				if (equipped != null && equipped.IsPlayer())
				{
					pLight.Lit = false;
					IComponent<GameObject>.AddPlayerMessage(The.Player.Poss(ParentObject) + ParentObject.GetVerb("dissipate") + ".");
				}
				ParentObject.Destroy();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		Light();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		LightSource lightSource = pLight;
		if (lightSource != null && lightSource.Lit && ParentObject.IsReal && ParentObject.IsTakeable())
		{
			E.AddTag("(" + getLitDescription() + ")");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void Light()
	{
		if (Fuel > 0 && pLight != null)
		{
			pLight.Lit = true;
		}
	}

	private string getLitDescription()
	{
		if (Fuel > 3500)
		{
			return "{{Y|blazing}}";
		}
		if (Fuel > 2000)
		{
			return "{{W|bright}}";
		}
		if (Fuel > 500)
		{
			return "dim";
		}
		return "{{K|faint}}";
	}

	public override bool Render(RenderEvent E)
	{
		LightSource lightSource = pLight;
		if (lightSource != null && lightSource.Lit)
		{
			int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				FrameOffset += Stat.Random(1, 5);
			}
			string text = null;
			if (num < 15)
			{
				text = "&Y";
			}
			else if (num < 30)
			{
				text = "&W";
			}
			else
			{
				text = ((num >= 45) ? "&W" : "&C");
			}
			E.ApplyColors(text, ICON_COLOR_PRIORITY);
		}
		return base.Render(E);
	}
}
