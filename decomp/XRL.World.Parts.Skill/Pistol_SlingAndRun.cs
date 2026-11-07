using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Pistol_SlingAndRun : BaseSkill
{
	public override void Initialize()
	{
		base.Initialize();
		Run.SyncAbility(ParentObject);
	}
}
