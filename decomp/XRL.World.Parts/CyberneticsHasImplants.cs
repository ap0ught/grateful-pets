using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsHasImplants : IPart
{
	public string Implants = "";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		Body body = ParentObject.Body;
		if (body != null)
		{
			string[] array = Implants.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('@');
				GameObject gameObject = GameObject.Create(array2[0]);
				BodyPart partByNameWithoutCybernetics = body.GetPartByNameWithoutCybernetics(array2[1]);
				if (partByNameWithoutCybernetics != null)
				{
					partByNameWithoutCybernetics.Implant(gameObject);
				}
				else
				{
					gameObject.Obliterate();
				}
			}
		}
		return base.HandleEvent(E);
	}
}
