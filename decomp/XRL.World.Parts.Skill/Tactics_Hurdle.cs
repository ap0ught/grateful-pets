using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Hurdle : BaseSkill
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (!Object.HasSkill("Tactics_Run"))
		{
			Object.AddSkill("Tactics_Run");
		}
		base.Register(Object, Registrar);
	}

	public override void Initialize()
	{
		base.Initialize();
		Run.SyncAbility(ParentObject);
	}
}
