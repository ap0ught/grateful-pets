using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class LiquidStained : Effect
{
	public LiquidVolume Liquid;

	[NonSerialized]
	private static Dictionary<string, int> CleanseAmounts = new Dictionary<string, int>();

	public override string RemoveSound => null;

	public LiquidStained()
	{
		DisplayName = "stained by liquid";
		Duration = 1;
	}

	public LiquidStained(LiquidVolume Liquid)
	{
		this.Liquid = Liquid;
		Duration = 9999;
	}

	public LiquidStained(LiquidVolume From, int Drams, int Duration = 9999)
	{
		Liquid = From.Split(Drams);
		base.Duration = Duration;
	}

	public LiquidStained(string LiquidSpec, int Drams, int Duration = 9999)
	{
		Liquid = new LiquidVolume(LiquidSpec, Drams);
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 67109888;
	}

	public override bool SameAs(Effect e)
	{
		LiquidStained liquidStained = e as LiquidStained;
		if (Liquid == null != (liquidStained.Liquid == null))
		{
			return false;
		}
		if (Liquid != null && !liquidStained.Liquid.SameAs(Liquid))
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool CanApplyToStack()
	{
		return true;
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Liquid = IPart.Load(null, Reader) as LiquidVolume;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		if (Liquid == null)
		{
			Liquid = new LiquidVolume();
		}
		IPart.Save(Liquid, Writer);
	}

	public override string GetDescription()
	{
		if (IsConcealedByLiquid())
		{
			return null;
		}
		if (Liquid == null)
		{
			return "stained by liquid";
		}
		return Liquid.StainedName.Color(Liquid.StainedColor);
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Stained by ").Append((Liquid == null) ? "liquid" : Liquid.GetLiquidName()).Append('.');
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (Liquid == null)
		{
			return false;
		}
		Liquid.Update();
		LiquidStained effect = Object.GetEffect<LiquidStained>();
		if (effect != null && effect.Liquid != null)
		{
			effect.Liquid.MixWith(Liquid);
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyLiquidStained", "Liquid", Liquid)))
		{
			return false;
		}
		Object.FireEvent(Event.New("AppliedLiquidStained", "Liquid", Liquid));
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CheckAnythingToCleanEvent>.ID && ID != PooledEvent<CleanItemsEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckAnythingToCleanEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CleanItemsEvent E)
	{
		E.RegisterObject(Object);
		E.RegisterType("stains");
		if (Liquid != null)
		{
			Liquid.Volume = 1;
			Liquid.FlowIntoCell(-1, Object.GetCurrentCell(), E.Actor);
		}
		Object.RemoveEffect(this, NeedStackCheck: false);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (Liquid != null && !IsConcealedByLiquid())
		{
			Liquid.ProcessStain(E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (Liquid != null && E.IsRelevantObject(Object))
		{
			foreach (string key in Liquid.ComponentLiquids.Keys)
			{
				LiquidVolume.GetLiquid(key).StainElements(Liquid, E);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0 && Object != null && Object.Render != null && Liquid != null && !IsConcealedByLiquid())
		{
			Liquid.RenderStain(E, Object);
		}
		return true;
	}

	public bool IsConcealedByLiquid()
	{
		if (Liquid == null)
		{
			return false;
		}
		LiquidCovered effect = Object.GetEffect<LiquidCovered>();
		if (effect == null || effect.Liquid == null)
		{
			return false;
		}
		if (Liquid.Primary == null)
		{
			return true;
		}
		if (effect.Liquid.ComponentLiquids.ContainsKey(Liquid.Primary))
		{
			if (Liquid.Secondary == null)
			{
				return true;
			}
			if (effect.Liquid.ComponentLiquids.ContainsKey(Liquid.Secondary))
			{
				return true;
			}
		}
		return false;
	}

	public bool Cleanse(int Amount)
	{
		if (Liquid == null)
		{
			return false;
		}
		CleanseAmounts.Clear();
		int total = 0;
		for (int i = 0; i < Amount; i++)
		{
			string randomElement = Liquid.ComponentLiquids.GetRandomElement(ref total);
			if (randomElement == null)
			{
				MetricsManager.LogError("LiquidStained on " + Object.DebugName + " had null selected liquid from " + Liquid.GetLiquidDebugDesignation() + " on cleanse dram " + (i + 1) + " of " + Amount);
				return false;
			}
			if (CleanseAmounts.ContainsKey(randomElement))
			{
				CleanseAmounts[randomElement]++;
			}
			else
			{
				CleanseAmounts.Add(randomElement, 1);
			}
		}
		Liquid.UseDrams(CleanseAmounts);
		if (Liquid.Volume <= 0)
		{
			Object.RemoveEffect(this);
		}
		return true;
	}

	public override void WasUnstackedFrom(GameObject obj)
	{
		base.WasUnstackedFrom(obj);
		if (Liquid == null)
		{
			return;
		}
		LiquidStained effect = obj.GetEffect<LiquidStained>();
		if (effect == null || effect.Liquid == null || effect.Liquid.Volume != Liquid.Volume)
		{
			return;
		}
		int count = Object.Count;
		int count2 = obj.Count;
		int num = Liquid.Volume * count / (count + count2);
		int num2 = Liquid.Volume * count2 / (count + count2);
		if (num + num2 < Liquid.Volume)
		{
			if (count > count2)
			{
				num++;
				if (num + num2 < Liquid.Volume)
				{
					num2++;
				}
			}
			else
			{
				num2++;
				if (num + num2 < Liquid.Volume)
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			Liquid.Volume = num;
		}
		else
		{
			Liquid.Empty();
			Duration = 0;
		}
		if (num2 > 0)
		{
			Liquid.Volume = num2;
			return;
		}
		effect.Liquid.Empty();
		effect.Duration = 0;
	}
}
