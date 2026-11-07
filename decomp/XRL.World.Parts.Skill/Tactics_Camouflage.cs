using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Camouflage : BaseSkill
{
	public int Level = 4;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	private void CheckCamouflage(bool ForceRemove = false)
	{
		FoliageCamouflaged foliageCamouflaged = ParentObject.GetEffect<FoliageCamouflaged>();
		bool flag = foliageCamouflaged != null;
		if (!flag)
		{
			foliageCamouflaged = new FoliageCamouflaged();
		}
		if (!ForceRemove && ParentObject.CurrentCell != null && ParentObject.CurrentCell.HasObjectOtherThan(foliageCamouflaged.EnablesCamouflage, ParentObject))
		{
			if (!flag)
			{
				ParentObject.ApplyEffect(foliageCamouflaged);
			}
			foliageCamouflaged.SetContribution(ParentObject, Level);
		}
		else if (flag)
		{
			foliageCamouflaged.RemoveContribution(ParentObject);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" || E.ID == "EnteredCell")
		{
			CheckCamouflage();
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		CheckCamouflage();
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		CheckCamouflage(ForceRemove: true);
		return base.AddSkill(GO);
	}
}
