using System;
using XRL.Rules;
using XRL.World.ObjectBuilders;

namespace XRL.World.Parts;

[Serializable]
[Obsolete("Use XRL.World.ObjectBuilders.Roboticized")]
public class Roboticized : IPart
{
	public const string PREFIX_NAME = "{{c|mechanical}}";

	public const string POSTFIX_DESC = "There is a low, persistent hum emanating outward.";

	public int ChanceOneIn = 10000;

	public string NamePrefix = "{{c|mechanical}}";

	public string DescriptionPostfix = "There is a low, persistent hum emanating outward.";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

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
		if (Stat.Random(1, ChanceOneIn) == 1)
		{
			Roboticize(ParentObject, NamePrefix, DescriptionPostfix);
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public static void Roboticize(GameObject Object, string NamePrefix = "{{c|mechanical}}", string DescriptionPostfix = "There is a low, persistent hum emanating outward.")
	{
		XRL.World.ObjectBuilders.Roboticized.Roboticize(Object, NamePrefix, DescriptionPostfix);
	}
}
