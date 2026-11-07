using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainMedicinal_DiseaseResistUnit : ProceduralCookingEffectUnit
{
	public override void Init(GameObject target)
	{
	}

	public override string GetDescription()
	{
		return "+3 to saves vs. disease";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "ModifyDefendingSave" && E.GetStringParameter("Vs").Contains("Disease"))
		{
			E.SetParameter("Roll", E.GetIntParameter("Roll") + 3);
		}
	}
}
