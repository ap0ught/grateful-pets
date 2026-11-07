using System;
using System.Text;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RestoreOnDeath : IPart
{
	public int Cooldown;

	public int Health = 100;

	public int Amount = -1;

	public int CurrentCooldown;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDieEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDieEvent E)
	{
		if (CurrentCooldown <= 0)
		{
			CurrentCooldown = Cooldown;
			if (Visible())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("Just before your demise, your health is restored!");
				}
				else
				{
					DidX("swim", "before your eyes", "!", null, null, ParentObject);
				}
			}
			float num = (float)ParentObject.GetStat("Hitpoints").BaseValue * ((float)Health / 100f);
			ParentObject.Heal((int)num, Message: true, FloatText: true);
			if (Amount != -1 && --Amount <= 0)
			{
				ParentObject.RemovePart(this);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		SB.Append("Respawns ");
		if (Amount > 0)
		{
			SB.Append(Grammar.Multiplicative(Amount)).Append(' ');
		}
		SB.Append("at ").Append(Health).Append("% health");
	}

	public override bool WantTurnTick()
	{
		return Cooldown > 0;
	}

	public override void TurnTick(long TimeTick, int Amount1)
	{
		if (CurrentCooldown > 0)
		{
			CurrentCooldown--;
		}
	}
}
