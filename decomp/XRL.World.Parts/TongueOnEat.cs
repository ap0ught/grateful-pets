using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class TongueOnEat : IPart
{
	public string Tongues = "1";

	public string Range = "10";

	public string Color = "&M";

	public string Message;

	public override bool SameAs(IPart p)
	{
		TongueOnEat tongueOnEat = p as TongueOnEat;
		if (tongueOnEat.Tongues != Tongues)
		{
			return false;
		}
		if (tongueOnEat.Range != Range)
		{
			return false;
		}
		if (tongueOnEat.Color != Color)
		{
			return false;
		}
		if (tongueOnEat.Message != Message)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			Message = "A trio of tongues vegetate from =subject.t's= =bodypart:Face=!";
			gameObjectParameter.EmitMessage(Message);
			StickyTongue.HarpoonNearest(gameObjectParameter, Range.RollCached(), Color, Tongues.RollCached());
			if (gameObjectParameter.IsPlayer())
			{
				E.RequestInterfaceExit();
			}
		}
		return base.FireEvent(E);
	}
}
