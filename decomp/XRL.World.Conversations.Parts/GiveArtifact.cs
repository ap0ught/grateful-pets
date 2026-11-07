using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class GiveArtifact : IConversationPart
{
	public static bool IsArtifact(GameObject Object)
	{
		if (!Object.HasPart<TinkerItem>())
		{
			return false;
		}
		if (Object.TryGetPart<Examiner>(out var Part))
		{
			return Part.Complexity > 0;
		}
		return false;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnterElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{g|[give artifact]}}";
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		List<GameObject> objects = The.Player.Inventory.GetObjects(IsArtifact);
		if (objects.Count == 0)
		{
			Popup.Show("You have no artifacts to give.");
			return false;
		}
		GameObject gameObject = Popup.PickGameObject("Choose an artifact to give.", objects, AllowEscape: true);
		if (gameObject == null)
		{
			return false;
		}
		gameObject.SplitStack(1, The.Player);
		if (!The.Player.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject)))
		{
			Popup.Show("You can't give that object.");
			return false;
		}
		return base.HandleEvent(E);
	}
}
