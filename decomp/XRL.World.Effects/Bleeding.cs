using System;
using XRL.Core;
using XRL.Liquids;
using XRL.Rules;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
[HasWishCommand]
public class Bleeding : Effect, ITierInitialized
{
	public string Damage = "1";

	public int SaveTarget = 20;

	public GameObject Owner;

	public bool Stack = true;

	public bool Internal;

	public bool StartMessageUsePopup;

	public bool StopMessageUsePopup;

	public bool Bandaged;

	public Bleeding()
	{
		DisplayName = "{{r|bleeding}}";
		Duration = 1;
	}

	public Bleeding(string Damage = "1", int SaveTarget = 20, GameObject Owner = null, bool Stack = true, bool Internal = false, bool StartMessageUsePopup = false, bool StopMessageUsePopup = false)
		: this()
	{
		this.Damage = Damage;
		this.SaveTarget = SaveTarget;
		this.Owner = Owner;
		this.Stack = Stack;
		this.Internal = Internal;
		this.StartMessageUsePopup = StartMessageUsePopup;
		this.StopMessageUsePopup = StopMessageUsePopup;
	}

	public void Initialize(int Tier)
	{
		Tier = XRL.World.Capabilities.Tier.Constrain(Stat.Random(Tier - 2, Tier + 2));
		SaveTarget = Math.Max(25, (Tier + 1) * 5);
		if (Tier >= 7)
		{
			Damage = "3-4";
		}
		else if (Tier >= 5)
		{
			Damage = "2-3";
		}
		else if (Tier >= 3)
		{
			Damage = "1-2";
		}
		else if (Tier >= 1)
		{
			Damage = "0-1";
		}
	}

	public override int GetEffectType()
	{
		return 117440528;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		GameObject.Validate(ref Owner);
		base.Write(Basis, Writer);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return DisplayName;
	}

	public override string GetDetails()
	{
		string text = Damage + " damage per turn.";
		if (Internal)
		{
			text += " Internal.";
		}
		if (Bandaged)
		{
			text += " Has been bandaged.";
		}
		return text;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.GetIntProperty("Bleeds") <= 0)
		{
			return false;
		}
		if (Stack)
		{
			foreach (Effect effect in Object.Effects)
			{
				if (effect is Bleeding bleeding && bleeding.GetType() == GetType() && bleeding.Stack)
				{
					if (bleeding.SaveTarget < SaveTarget)
					{
						bleeding.SaveTarget = SaveTarget;
						bleeding.Bandaged = false;
					}
					if (bleeding.Damage.GetCachedDieRoll().Average() < Damage.GetCachedDieRoll().Average())
					{
						bleeding.Damage = Damage;
						bleeding.Bandaged = false;
					}
					return false;
				}
			}
		}
		if (!Object.FireEvent(Event.New("Apply" + ClassName, "Effect", this)))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, ClassName, this))
		{
			return false;
		}
		SyncVersion();
		ApplyingBleeding(Object);
		StartMessage(Object);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		StopMessage(Object);
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != SingletonEvent<GeneralAmnestyEvent>.ID)
		{
			return ID == PooledEvent<GetCompanionStatusEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("bleeding", 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		ProcessBleeding();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Recuperating");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		_ = Object.Render;
		int num = XRLCore.CurrentFrame % 60;
		if (num > 45 && num < 55)
		{
			E.RenderString = "\u0003";
			E.ApplyColors("&r", Effect.ICON_COLOR_PRIORITY);
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			Object.RemoveEffect(this);
		}
		return base.FireEvent(E);
	}

	public virtual string DamageAttributes()
	{
		return "Bleeding Unavoidable";
	}

	public virtual string SaveAttribute()
	{
		return "Toughness";
	}

	public virtual string SaveVs()
	{
		return "Bleeding";
	}

	public virtual bool CanApplyBleeding(GameObject Object)
	{
		return true;
	}

	public virtual void ApplyingBleeding(GameObject Object)
	{
	}

	public virtual bool RecoveryChance(int ReduceSaveTargetBy = 1, bool ReduceFirst = false)
	{
		if (ReduceFirst)
		{
			SaveTarget -= ReduceSaveTargetBy;
		}
		if (Object.MakeSave(SaveAttribute(), SaveTarget, null, null, SaveVs()))
		{
			Object.RemoveEffect(this);
			return true;
		}
		if (!ReduceFirst)
		{
			SaveTarget -= ReduceSaveTargetBy;
		}
		return false;
	}

	public void ProcessBleeding()
	{
		GameObject.Validate(ref Owner);
		if (Duration <= 0 || !GameObject.Validate(Object) || RecoveryChance())
		{
			return;
		}
		Object.TakeDamage(Damage.RollCached(), Attributes: DamageAttributes(), Attacker: Owner, Message: DamageMessage(), DeathReason: null, ThirdPersonDeathReason: null, Owner: null, Source: null, Perspective: null, DescribeAsFrom: null, Accidental: false, Environmental: false, Indirect: true);
		if (Internal)
		{
			return;
		}
		Cell cell = Object.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			return;
		}
		bool flag = false;
		if (50.in100())
		{
			foreach (GameObject item in cell.GetObjectsWithPartReadonly("Render"))
			{
				LiquidVolume liquidVolume = item.LiquidVolume;
				if (liquidVolume != null && liquidVolume.IsOpenVolume())
				{
					LiquidVolume liquidVolume2 = new LiquidVolume();
					liquidVolume2.InitialLiquid = Object.GetBleedLiquid();
					liquidVolume2.Volume = 2;
					liquidVolume.MixWith(liquidVolume2);
					flag = true;
				}
				else
				{
					item.MakeBloody(Object.GetBleedLiquid(), Stat.Random(1, 3));
				}
			}
		}
		if (!flag && 5.in100())
		{
			GameObject gameObject = GameObject.Create("BloodSplash");
			LiquidVolume liquidVolume3 = gameObject.LiquidVolume;
			if (liquidVolume3 != null)
			{
				liquidVolume3.InitialLiquid = Object.GetBleedLiquid();
				SplashCreated(gameObject);
				cell.AddObject(gameObject);
			}
			else
			{
				MetricsManager.LogError("generated " + gameObject.Blueprint + " with no LiquidVolume");
				gameObject.Obliterate();
			}
		}
	}

	public virtual void SplashCreated(GameObject Object)
	{
	}

	public virtual void SyncVersion()
	{
		DisplayName = GetVersionOfBleedingForLiquidSpecification();
	}

	public virtual void StartMessage(GameObject Object)
	{
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_physicalRupture");
		if (Object.HasEffectOtherThan((Effect fx) => fx is Bleeding, this))
		{
			DidX("begin", DisplayNameStripped + " from another wound", "!", null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
		}
		else
		{
			DidX("begin", DisplayNameStripped, "!", null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
		}
	}

	public virtual void StopMessage(GameObject Object)
	{
		if (Object.HasEffectOtherThan((Effect fx) => fx is Bleeding, this))
		{
			EmitMessage("One of " + Object.poss("wounds") + " stops " + DisplayNameStripped + ".", ' ', FromDialog: false, StopMessageUsePopup, AlwaysVisible: false, Object);
		}
		else
		{
			DidX("stop", DisplayNameStripped, null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StopMessageUsePopup);
		}
	}

	public virtual string DamageMessage()
	{
		return "from " + DisplayNameStripped + ".";
	}

	public static string GetPrimaryLiquidNameForLiquidSpecification(string Spec)
	{
		if (!Spec.Contains(","))
		{
			if (!Spec.Contains("-"))
			{
				return Spec;
			}
			if (Spec.EndsWith("-1000"))
			{
				return Spec.Substring(0, Spec.Length - 5);
			}
		}
		return new LiquidVolume(Spec, 1).GetPrimaryLiquidID();
	}

	public static BaseLiquid GetPrimaryLiquidForLiquidSpecification(string Spec)
	{
		return LiquidVolume.GetLiquid(GetPrimaryLiquidNameForLiquidSpecification(Spec));
	}

	public static string GetVersionOfBleedingForLiquidSpecification(string Spec)
	{
		return GetPrimaryLiquidForLiquidSpecification(Spec).ColoredCirculatoryLossTerm;
	}

	public BaseLiquid GetPrimaryLiquidForLiquidSpecification()
	{
		return GetPrimaryLiquidForLiquidSpecification(Object.GetBleedLiquid());
	}

	public string GetPrimaryLiquidNameForLiquidSpecification()
	{
		return GetPrimaryLiquidNameForLiquidSpecification(Object.GetBleedLiquid());
	}

	public string GetVersionOfBleedingForLiquidSpecification()
	{
		return GetVersionOfBleedingForLiquidSpecification(Object.GetBleedLiquid());
	}

	[WishCommand(null, null, Command = "bleed")]
	public void HandleBleedWish()
	{
		IComponent<GameObject>.ThePlayer.ApplyEffect(new Bleeding("1d2-1"));
	}
}
