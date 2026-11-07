namespace XRL.World.Parts;

public abstract class IObjectGasBehavior : IGasBehavior
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DensityChange");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DensityChange" && StepValue(E.GetIntParameter("OldValue")) != StepValue(E.GetIntParameter("NewValue")))
		{
			FlushNavigationCaches();
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Type != "Thrown")
		{
			ApplyGas(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ApplyGas(ParentObject.CurrentCell);
	}

	public virtual bool ApplyGas(Cell Cell)
	{
		bool result = false;
		if (Cell != null)
		{
			Cell.ObjectRack objects = Cell.Objects;
			int i = 0;
			for (int count = objects.Count; i < count; i++)
			{
				GameObject gameObject = objects[i];
				if (!ApplyGas(gameObject))
				{
					continue;
				}
				result = true;
				if (count != objects.Count)
				{
					count = objects.Count;
					if (i < count && objects[i] != gameObject)
					{
						i--;
					}
				}
			}
		}
		return result;
	}

	public virtual bool ApplyGas(GameObject Object)
	{
		return false;
	}
}
