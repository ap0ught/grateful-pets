using System;

namespace XRL.World.QuestManagers;

[Serializable]
public class PaxQuestStep : IComposite
{
	public string Name;

	public string Text;

	public string Target;

	public bool Finished;
}
