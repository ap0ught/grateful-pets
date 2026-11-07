using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Invisibility : BaseMutation
{
	public Invisibility()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("glass", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You cannot be seen.";
	}

	public override string GetLevelText(int Level)
	{
		return GetDescription();
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CustomRender");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CustomRender")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				LightLevel light = cell.GetLight();
				if (light == LightLevel.Darkvision || light == LightLevel.LitRadar || light == LightLevel.Radar || light == LightLevel.Dimvision || light == LightLevel.Interpolight || light == LightLevel.Omniscient)
				{
					ParentObject.Render.Visible = true;
				}
				else
				{
					ParentObject.Render.Visible = false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
