using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class InteriorBlockEntrance : IPart
{
	public string Message;

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == SingletonEvent<CanEnterInteriorEvent>.ID;
	}

	public override bool HandleEvent(CanEnterInteriorEvent E)
	{
		if (E.Object == ParentObject)
		{
			if (E.Actor.IsPlayer() && !Message.IsNullOrEmpty())
			{
				Popup.Show(Message.StartReplace().AddObject(E.Actor).AddObject(E.Object)
					.ToString());
			}
			E.Status = 30;
			E.ShowMessage = false;
			return false;
		}
		return base.HandleEvent(E);
	}
}
