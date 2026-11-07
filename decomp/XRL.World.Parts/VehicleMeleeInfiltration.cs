using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class VehicleMeleeInfiltration : IPart
{
	public int ChancePerPenetration = 10;

	public bool AllowAllied;

	public bool AllowNeutral;

	[NonSerialized]
	private static uint LastAttempt;

	[NonSerialized]
	private int InfiltrationChance;

	[NonSerialized]
	private Vehicle _Vehicle;

	public Vehicle Vehicle => _Vehicle ?? (_Vehicle = ParentObject.GetPart<Vehicle>());

	public bool TryInfiltrate(GameObject Actor, Interior Interior)
	{
		uint Hash = ParentObject.CurrentZone.ZoneID.GetStableHashCode32();
		Interior.ZoneID.GetStableHashCode32(ref Hash);
		if (LastAttempt != Hash)
		{
			if (Popup.ShowYesNo(Event.FinalizeString(Event.NewStringBuilder(ParentObject.Does("are")).Append(" occupied.\n\n").Append("Attempting to enter is a hostile action and will function as an attack with ")
				.Append(ChancePerPenetration)
				.Append("% infiltration chance per penetration.\n\n")
				.Append("Are you sure you want to proceed?")), "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) != DialogResult.Yes)
			{
				return false;
			}
			LastAttempt = Hash;
		}
		if (The.Core.IDKFA)
		{
			return true;
		}
		try
		{
			InfiltrationChance = -1;
			ParentObject.RegisterPartEvent(this, "DefenderAfterAttack");
			Combat.PerformMeleeAttack(Actor, ParentObject);
		}
		finally
		{
			ParentObject.UnregisterPartEvent(this, "DefenderAfterAttack");
		}
		if (ParentObject.IsValid())
		{
			return InfiltrationChance.in100();
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<CanEnterInteriorEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanEnterInteriorEvent E)
	{
		if (E.Object == ParentObject && !E.Status.HasBit(30) && !Vehicle.PilotID.IsNullOrEmpty())
		{
			if (AllowAllied && E.Object.IsAlliedTowards(E.Actor))
			{
				return base.HandleEvent(E);
			}
			if (AllowNeutral && !E.Object.IsHostileTowards(E.Actor))
			{
				return base.HandleEvent(E);
			}
			if (!E.Actor.IsPlayer())
			{
				E.Status = 2;
				return false;
			}
			if (!E.Action)
			{
				E.Status.SetBit(1, value: false);
			}
			else
			{
				if (TryInfiltrate(E.Actor, E.Interior))
				{
					E.Status = 0;
					if (!Achievement.INFILTRATE_MECHA.Achieved && E.Actor.IsPlayer() && ParentObject.GetBlueprint().DescendsFrom("VehicleTemplarMech"))
					{
						Achievement.INFILTRATE_MECHA.Unlock();
					}
					if (E.ShowMessage)
					{
						IComponent<GameObject>.EmitMessage(E.Actor, E.Actor.Does("infiltrate") + " " + E.Object.t() + "!", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: false, E.Actor);
					}
				}
				else
				{
					E.Status = 1;
				}
				E.ShowMessage = false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefenderAfterAttack" && GameObject.Validate(ParentObject) && InfiltrationChance == -1)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			if (GameObject.Validate(gameObjectParameter) && gameObjectParameter.IsPlayer())
			{
				InfiltrationChance = E.GetIntParameter("Penetrations") * ChancePerPenetration;
			}
		}
		return base.FireEvent(E);
	}
}
