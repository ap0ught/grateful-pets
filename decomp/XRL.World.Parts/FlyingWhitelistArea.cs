using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FlyingWhitelistArea : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ClimbUp");
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Calendar.IsDay())
		{
			ParentObject?.CurrentCell?.ParentZone?.AddLight(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, 1, LightLevel.Light);
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ClimbUp")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("GO");
			if (gameObjectParameter != null && gameObjectParameter.HasEffect<Flying>() && gameObjectParameter.IsPlayer())
			{
				Cell cell = gameObjectParameter.GetCurrentCell()?.GetCellFromDirection("U", BuiltOnly: false);
				if (cell == null)
				{
					return true;
				}
				if (!cell.HasObjectWithPart("StairsDown") && !cell.HasObjectWithTag("FlyingWhitelistArea"))
				{
					return true;
				}
				gameObjectParameter.Move("U", Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: false);
				return false;
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
