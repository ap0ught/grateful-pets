namespace XRL.World.Parts;

public class LifeGate : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttemptDoorUnlock");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttemptDoorUnlock")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			if (!The.Game.HasQuest("Reclamation"))
			{
				return gameObjectParameter.ShowFailure("The gates are sealed for eternity.");
			}
			if (!The.Game.HasFinishedQuest("Reclamation"))
			{
				return gameObjectParameter.ShowFailure("The gates are secured shut until the threat to Omonporch is eliminated.");
			}
			if (ParentObject.TryGetPart<Door>(out var Part) && Part.Locked)
			{
				Part.Unlock();
				Part.AttemptOpen();
			}
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}
}
