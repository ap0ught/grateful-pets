using System;
using XRL.Language;
using XRL.Liquids;

namespace XRL.World.Effects;

[Serializable]
public class Nosebleed : Bleeding, ITierInitialized
{
	public string Color = "r";

	public string CirculatoryLossTerm = "bleeding";

	public string CirculatoryLossNoun = "bleed";

	public bool Hemorrhage;

	public Nosebleed()
	{
		DisplayName = "{{r|nosebleed}}";
		Internal = true;
	}

	public Nosebleed(string Damage = "1", int SaveTarget = 20, GameObject Owner = null, bool Stack = true, bool StartMessageUsePopup = false, bool StopMessageUsePopup = false)
		: this()
	{
		base.Damage = Damage;
		base.SaveTarget = SaveTarget;
		base.Owner = Owner;
		base.Stack = Stack;
		base.StartMessageUsePopup = StartMessageUsePopup;
		base.StopMessageUsePopup = StopMessageUsePopup;
	}

	public override string GetStateDescription()
	{
		if (Hemorrhage)
		{
			return ("cerebrally " + CirculatoryLossTerm).Color(Color);
		}
		return ("nasally " + CirculatoryLossTerm).Color(Color);
	}

	public override bool CanApplyBleeding(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyBleeding", "Effect", this)))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Bleeding", this))
		{
			return false;
		}
		if (!Object.CanHaveNosebleed() && Object.Brain == null)
		{
			return false;
		}
		return base.CanApplyBleeding(Object);
	}

	public override void SyncVersion()
	{
		BaseLiquid primaryLiquidForLiquidSpecification = GetPrimaryLiquidForLiquidSpecification();
		if (primaryLiquidForLiquidSpecification != null)
		{
			CirculatoryLossTerm = primaryLiquidForLiquidSpecification.CirculatoryLossTerm;
			CirculatoryLossNoun = primaryLiquidForLiquidSpecification.CirculatoryLossNoun;
			Color = primaryLiquidForLiquidSpecification.GetColor();
			DisplayName = ("nose" + CirculatoryLossNoun).Color(Color);
		}
	}

	public override void ApplyingBleeding(GameObject Object)
	{
		if (!Object.CanHaveNosebleed())
		{
			DisplayName = "hemorrhaging".Color(Color);
			Hemorrhage = true;
		}
		base.ApplyingBleeding(Object);
	}

	public override void StartMessage(GameObject Object)
	{
		if (Hemorrhage)
		{
			base.StartMessage(Object);
		}
		else if (Object.GetBodyPartCount("Face") > 1)
		{
			if (Object.HasEffectOtherThan(typeof(Nosebleed), this))
			{
				EmitMessage(Object.Poss("noses") + " begin " + CirculatoryLossTerm + " more heavily.", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, null, Object);
			}
			else
			{
				EmitMessage(Object.Poss("noses") + " begin " + CirculatoryLossTerm + ".", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, null, Object);
			}
		}
		else if (Object.HasEffectOtherThan(typeof(Nosebleed), this))
		{
			EmitMessage(Object.Poss("nose") + " begins " + CirculatoryLossTerm + " more heavily.", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, null, Object);
		}
		else
		{
			EmitMessage(Object.Poss("nose") + " begins " + CirculatoryLossTerm + ".", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, null, Object);
		}
	}

	public override void StopMessage(GameObject Object)
	{
		if (Hemorrhage)
		{
			base.StartMessage(Object);
		}
		else if (Object.GetBodyPartCount("Face") > 1)
		{
			if (Object.HasEffectOtherThan(typeof(Nosebleed), this))
			{
				EmitMessage(Object.Poss("noses") + " stop " + CirculatoryLossTerm + " quite so heavily.", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, Object);
			}
			else
			{
				EmitMessage(Object.Poss("noses") + " stop " + CirculatoryLossTerm + ".", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, Object);
			}
		}
		else if (Object.HasEffectOtherThan(typeof(Nosebleed), this))
		{
			EmitMessage(Object.Poss("nose") + " stops " + CirculatoryLossTerm + " quite so heavily.", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, Object);
		}
		else
		{
			EmitMessage(Object.Poss("nose") + " stops " + CirculatoryLossTerm + ".", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, Object);
		}
	}

	public override string DamageMessage()
	{
		if (Hemorrhage)
		{
			return base.DamageMessage();
		}
		return "from " + Grammar.A(DisplayNameStripped) + ".";
	}
}
