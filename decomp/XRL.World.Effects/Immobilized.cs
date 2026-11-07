using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Immobilized : Effect, ITierInitialized
{
	public string Text = "immobilized";

	public int SaveTarget = 20;

	public string SaveStat = "Agility";

	public string SaveVs = "Immobilization";

	public int SaveTargetTurnDivisor = 5;

	public int Turns;

	public bool LinkedToProne = true;

	public Immobilized()
	{
		DisplayName = "immobilized";
		Duration = 1;
	}

	public Immobilized(int SaveTarget)
		: this()
	{
		this.SaveTarget = SaveTarget;
	}

	public Immobilized(int SaveTarget, string SaveStat)
		: this(SaveTarget)
	{
		this.SaveStat = SaveStat;
	}

	public Immobilized(int SaveTarget, string SaveStat, string SaveVs)
		: this(SaveTarget, SaveStat)
	{
		this.SaveVs = SaveVs;
	}

	public Immobilized(int SaveTarget, string SaveStat, string SaveVs, string Text)
		: this(SaveTarget, SaveStat, SaveVs)
	{
		this.Text = Text;
	}

	public Immobilized(int SaveTarget, string SaveStat, string SaveVs, string Text, bool LinkedToProne)
		: this(SaveTarget, SaveStat, SaveVs, Text)
	{
		this.LinkedToProne = LinkedToProne;
	}

	public void Initialize(int Tier)
	{
		Duration = Tier + 1;
	}

	public override int GetEffectType()
	{
		return 100663424;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Can't move or stand up.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.Statistics.ContainsKey("Energy"))
		{
			return false;
		}
		if (!Object.CanChangeBodyPosition("Immobilized", ShowMessage: false, Involuntary: true))
		{
			return false;
		}
		if (Object.FireEvent("ApplyImmobilized"))
		{
			DidX("are", Text, "!", null, null, null, Object);
			if (!Object.IsPlayer() && Visible())
			{
				Object.ParticleText("*" + Text + "*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
			}
			Object.Statistics["Energy"].BaseValue = 0;
			Object.BodyPositionChanged(null, Involuntary: true);
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanChangeMovementModeEvent>.ID)
		{
			return ID == PooledEvent<GetCompanionStatusEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (Duration > 0 && !E.Involuntary && E.Object == Object)
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You are " + Text + "!");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("immobilized", 70);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("CanChangeBodyPosition");
		Registrar.Register("CanStandUp");
		Registrar.Register("EndTurn");
		Registrar.Register("IsMobile");
		Registrar.Register("LeaveCell");
		Registrar.Register("MovementModeChanged");
		base.Register(Object, Registrar);
	}

	public int GetEffectiveSaveTarget()
	{
		int num = SaveTarget;
		if (SaveTargetTurnDivisor != 0)
		{
			num -= Turns / SaveTargetTurnDivisor;
		}
		return num;
	}

	public void EndImmobilization()
	{
		DidX("are", "no longer " + Text, null, null, null, Object);
		Object.RemoveEffect(this);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile" || E.ID == "CanStandUp")
		{
			if (Duration > 0)
			{
				return false;
			}
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (Turns > 0 && Object.MakeSave(SaveStat, GetEffectiveSaveTarget(), null, null, SaveVs))
			{
				EndImmobilization();
			}
			else
			{
				Turns++;
			}
		}
		else if (E.ID == "LeaveCell")
		{
			if (Duration > 0 && !E.HasParameter("Teleporting") && !E.HasParameter("Dragging") && !E.HasParameter("Forced"))
			{
				if (Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You are " + Text + "&y!");
				}
				Object.UseEnergy(1000);
				return false;
			}
		}
		else if (E.ID == "CanChangeBodyPosition")
		{
			if (Duration > 0 && !E.HasFlag("Involuntary"))
			{
				if (E.HasFlag("ShowMessage") && Object.IsPlayer())
				{
					Popup.ShowFail("You are {{|" + Text + "}}!");
				}
				return false;
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			Object.RemoveEffect(this);
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 40 && num < 60)
			{
				E.RenderString = "\u001c";
				E.ColorString = "&K";
			}
		}
		return true;
	}
}
