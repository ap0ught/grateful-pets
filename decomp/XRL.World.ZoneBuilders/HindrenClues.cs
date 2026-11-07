namespace XRL.World.ZoneBuilders;

public class HindrenClues : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		foreach (string clueItem in KithAndKinGameState.Instance.clueItems)
		{
			ZoneBuilderSandbox.PlaceObject(GameObjectFactory.Factory.CreateObject(clueItem), Z);
		}
		foreach (HindrenClueLook lookClue in KithAndKinGameState.Instance.lookClues)
		{
			foreach (GameObject @object in Z.GetObjects((GameObject go) => go.Blueprint == lookClue.target))
			{
				lookClue.apply(@object);
			}
		}
		return true;
	}
}
