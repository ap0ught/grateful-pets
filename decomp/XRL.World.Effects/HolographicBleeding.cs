using System;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class HolographicBleeding : Bleeding, ITierInitialized
{
	public HolographicBleeding()
	{
	}

	public HolographicBleeding(string Damage = "1", int SaveTarget = 20, GameObject Owner = null, bool Stack = true, bool Internal = false, bool StartMessageUsePopup = false, bool StopMessageUsePopup = false)
		: this()
	{
		base.Damage = Damage;
		base.SaveTarget = SaveTarget;
		base.Owner = Owner;
		base.Stack = Stack;
		base.Internal = Internal;
		base.StartMessageUsePopup = StartMessageUsePopup;
		base.StopMessageUsePopup = StopMessageUsePopup;
	}

	public override int GetEffectType()
	{
		return 117440576;
	}

	public override void StartMessage(GameObject Object)
	{
		if (Object.IsPlayer())
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
		else if (Object.HasEffectOtherThan((Effect fx) => fx is Bleeding, this))
		{
			DidX("begin", "acting like " + Object.itis + " " + DisplayNameStripped + " from another wound", null, null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
		}
		else
		{
			DidX("begin", "acting like " + Object.itis + " " + DisplayNameStripped, null, null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
		}
	}

	public override void StopMessage(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetIntProperty("Analgesia") > 0)
			{
				if (Object.HasEffectOtherThan((Effect fx) => fx is Bleeding, this))
				{
					DidX("realize", "one of your wounds is an illusion", null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StopMessageUsePopup);
				}
				else
				{
					DidX("realize", "your wound is an illusion", null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StopMessageUsePopup);
				}
			}
			else if (Object.HasEffectOtherThan((Effect fx) => fx is Bleeding, this))
			{
				DidX("realize", "one of your wounds is an illusion, and the pain from it suddenly stops", null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StopMessageUsePopup);
			}
			else
			{
				DidX("realize", "your wound is an illusion, and the pain suddenly stops", null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StopMessageUsePopup);
			}
		}
		else if (Object.HasEffectOtherThan((Effect fx) => fx is Bleeding, this))
		{
			DidX("stop", "acting like " + Object.itis + " " + DisplayNameStripped + " so much", null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
		}
		else
		{
			DidX("stop", "acting like " + Object.itis + " " + DisplayNameStripped, null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, StartMessageUsePopup);
		}
	}

	public override string DamageAttributes()
	{
		return "Bleeding Illusion Unavoidable";
	}

	public override string SaveAttribute()
	{
		return "Intelligence";
	}

	public override string SaveVs()
	{
		return "Hologram Illusion";
	}

	public override void SplashCreated(GameObject Object)
	{
		Object.AddPart(new XRL.World.Parts.Temporary(25));
		base.SplashCreated(Object);
	}
}
