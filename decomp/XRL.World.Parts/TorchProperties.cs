using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TorchProperties : IPart
{
	public int Fuel = 5000;

	public bool InHand;

	public GameObject LastThrower;

	public string LightSound;

	public string ExtinguishSound;

	public int FrameOffset;

	public bool ChangeColorString;

	public bool ChangeDetailColor;

	public bool LiquidExtinguishes = true;

	private LightSource _pLight;

	private LightSource pLight => _pLight ?? (_pLight = ParentObject.GetPart<LightSource>());

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		GameObject.Validate(ref LastThrower);
		base.Write(Basis, Writer);
	}

	public override bool SameAs(IPart p)
	{
		TorchProperties torchProperties = p as TorchProperties;
		if (torchProperties.Fuel != Fuel)
		{
			return false;
		}
		if (torchProperties.ChangeColorString != ChangeColorString)
		{
			return false;
		}
		if (torchProperties.ChangeDetailColor != ChangeDetailColor)
		{
			return false;
		}
		if (torchProperties.LiquidExtinguishes != LiquidExtinguishes)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != AfterThrownEvent.ID && (ID != EffectAppliedEvent.ID || !LiquidExtinguishes) && ID != SingletonEvent<EndTurnEvent>.ID && ID != EquippedEvent.ID && (ID != EnteredCellEvent.ID || !LiquidExtinguishes) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != SingletonEvent<RadiatesHeatEvent>.ID && ID != SuspendingEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		Extinguish();
		InHand = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		LastThrower = E.Actor;
		InHand = false;
		if (!IsUnlightableBecauseOfSubmersion() && !IsUnlightableBecauseOfLiquidCovering())
		{
			Light();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (LiquidExtinguishes)
		{
			LightSource lightSource = pLight;
			if (lightSource != null && lightSource.Lit && IsUnlightableBecauseOfSubmersion(E.Cell))
			{
				Extinguish();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (LiquidExtinguishes && E.Effect is LiquidCovered fX)
		{
			LightSource lightSource = pLight;
			if (lightSource != null && lightSource.Lit && IsUnlightableBecauseOfLiquidCovering(fX))
			{
				Extinguish();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (pLight.Lit)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!ParentObject.HasPart<Flame>())
		{
			if (pLight.Lit)
			{
				E.AddAdjective("lit", -40);
				E.AddTag("(" + getLitDescription() + ")", -40);
			}
			else
			{
				E.AddTag("(" + getUnlitDescription() + ")", -40);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		LastThrower = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (E.Part != null && E.Part.Type == "Hand")
		{
			if (!IsUnlightableBecauseOfLiquidCovering())
			{
				Light();
			}
			InHand = true;
		}
		else
		{
			Extinguish();
			InHand = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		InHand = false;
		Extinguish();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.Validate(ref LastThrower);
		GameObject equipped = ParentObject.Equipped;
		if (equipped != null && equipped.OnWorldMap())
		{
			return true;
		}
		if (Options.AutoTorch && InHand)
		{
			if (pLight.Lit)
			{
				if (equipped != null && equipped.IsPlayer() && equipped.IsUnderSky() && IsDay())
				{
					Extinguish();
				}
			}
			else if (equipped != null && equipped.IsPlayer())
			{
				if (equipped.IsUnderSky() && IsNight())
				{
					if (!IsUnlightableBecauseOfLiquidCovering())
					{
						Light();
					}
				}
				else if (equipped.CurrentZone != null && equipped.CurrentZone.Z > 10 && !IsUnlightableBecauseOfLiquidCovering())
				{
					Light();
				}
			}
		}
		if (pLight.Lit)
		{
			if (Fuel > 0)
			{
				Fuel--;
			}
			if (Fuel <= 0)
			{
				Extinguish();
				if (equipped != null && equipped.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your torch burns out!");
					AutoAct.Interrupt();
				}
				ParentObject.Destroy(null, Silent: true);
			}
			else
			{
				if (ParentObject.Physics.CurrentCell != null)
				{
					int phase = ParentObject.GetPhase();
					foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPartReadonly("Physics"))
					{
						if (item == ParentObject)
						{
							continue;
						}
						if (ParentObject.Physics.IsReal)
						{
							if (item.Physics.Temperature < 400 && Stat.Random(1, 4) == 1)
							{
								item.TemperatureChange(400, LastThrower, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase);
							}
						}
						else if (item.Physics.Temperature < 400 && Stat.Random(1, 4) == 1)
						{
							item.TemperatureChange(100, LastThrower, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, phase);
						}
					}
				}
				if (ParentObject.Physics.IsReal && ParentObject.CurrentCell != null && 8.in100())
				{
					ParentObject.Smoke();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (InHand && ParentObject.Equipped != null)
		{
			if (pLight.Lit)
			{
				E.AddAction("Extinguish", "extinguish", "TorchExtinguish", null, 'x', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
			}
			else
			{
				E.AddAction("Light", "light", "TorchLight", null, 'i', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "TorchLight")
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (IsUnlightableBecauseOfLiquidCovering())
			{
				return E.Actor.Fail("You cannot light " + ParentObject.t() + " because of the liquid " + ParentObject.itis + " covered in.");
			}
			if (IsUnlightableBecauseOfSubmersion())
			{
				return E.Actor.Fail("You cannot light " + ParentObject.t() + " because of the liquid " + ParentObject.itis + " submerged in.");
			}
			E.Actor.UseEnergy(1000);
			Light();
		}
		else if (E.Command == "TorchExtinguish")
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			E.Actor.UseEnergy(1000);
			Extinguish();
		}
		return base.HandleEvent(E);
	}

	public void Light()
	{
		if (Fuel > 0)
		{
			PlayWorldSound(LightSound);
			pLight.Lit = true;
			ParentObject.Render.Tile = "Items/sw_torch_lit.png";
			ParentObject.Render.DetailColor = "W";
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
			return "{{w|dim}}";
		}
		return "{{r|smouldering}}";
	}

	private string getUnlitDescription()
	{
		if (Fuel > 3500)
		{
			return "unburnt";
		}
		if (Fuel > 2000)
		{
			return "half-burnt";
		}
		if (Fuel > 500)
		{
			return "mostly burnt";
		}
		return "nearly extinguished";
	}

	public void Extinguish()
	{
		PlayWorldSound(ExtinguishSound);
		pLight.Lit = false;
		ParentObject.Render.Tile = "Items/sw_torch.png";
		ParentObject.Render.DetailColor = "r";
	}

	public override bool Render(RenderEvent E)
	{
		if ((ChangeColorString || ChangeDetailColor) && pLight.Lit)
		{
			int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				FrameOffset += Stat.Random(1, 5);
			}
			char c = 'W';
			if (num < 15)
			{
				c = 'R';
			}
			else if (num >= 30 && num < 45)
			{
				c = 'r';
			}
			if (ChangeColorString || (ChangeDetailColor && !Options.UseTiles))
			{
				E.ColorString = "&" + c;
			}
			if (ChangeDetailColor)
			{
				E.DetailColor = c.ToString() ?? "";
			}
		}
		return true;
	}

	public bool IsUnlightableBecauseOfSubmersion(Cell Cell = null)
	{
		if (!LiquidExtinguishes)
		{
			return false;
		}
		if (Cell == null)
		{
			Cell = ParentObject.CurrentCell;
		}
		return Cell?.HasWadingDepthLiquid() ?? false;
	}

	public bool IsUnlightableBecauseOfLiquidCovering(LiquidCovered FX = null)
	{
		if (!LiquidExtinguishes)
		{
			return false;
		}
		if (FX == null)
		{
			FX = ParentObject.GetEffect<LiquidCovered>();
		}
		if (FX?.Liquid != null && FX.Liquid.GetLiquidCombustibility() < 30 && FX.Liquid.GetLiquidTemperature() < 100)
		{
			return true;
		}
		return false;
	}
}
