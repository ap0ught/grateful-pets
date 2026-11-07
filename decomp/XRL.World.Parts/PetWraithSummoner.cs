using System;

namespace XRL.World.Parts;

[Serializable]
public class PetWraithSummoner : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && IComponent<GameObject>.ThePlayer != null)
		{
			GameObject gameObject = GameObject.Create("BrokenPhylactery");
			IComponent<GameObject>.ThePlayer.ReceiveObject(gameObject);
			InventoryActionEvent.Check(gameObject, IComponent<GameObject>.ThePlayer, gameObject, "ActivateTemplarPhylactery");
			ParentObject.Obliterate();
		}
		return base.FireEvent(E);
	}
}
