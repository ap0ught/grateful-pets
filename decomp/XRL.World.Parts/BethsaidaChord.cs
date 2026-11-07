using System.Text;
using UnityEngine;

namespace XRL.World.Parts;

public class BethsaidaChord : INephalChord
{
	public override string Source => "Bethsaida";

	public virtual float HPMult => 0.5f;

	public virtual int HPBonus => Mathf.RoundToInt((float)ParentObject.GetStat("Hitpoints").BaseValue * HPMult);

	public override void Initialize()
	{
		StatShifter.SetStatShift("Hitpoints", HPBonus);
	}

	public override void Remove()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void AppendRules(StringBuilder Postfix)
	{
		Postfix.Append("\n• ");
		if (HPMult > 0f)
		{
			Postfix.Append('+');
		}
		Postfix.Append(Mathf.RoundToInt(HPMult * 100f)).Append("% ").Append(Statistic.GetStatDisplayName("Hitpoints"));
	}
}
