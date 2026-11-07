namespace XRL.World.Effects;

public abstract class SoundRendererEffect : Effect, ISoundRenderer
{
	public abstract void Render(RenderSoundEvent E);

	void ISoundRenderer.Render(RenderSoundEvent E)
	{
		Cell cell = Object?.CurrentCell;
		if (cell == null || !cell.ParentZone.IsActive())
		{
			E.Unregister = true;
			return;
		}
		E.Char = E.Buffer[cell.X, cell.Y];
		Render(E);
		E.Char.soundExtra.SetDistance(Zone.SoundMap.GetCostAtPoint(cell.X, cell.Y));
		E.Char.soundExtra.SetOccluded(!cell.IsVisible());
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		E.Zone.RegisterSoundRenderer(this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Cell.ParentZone.IsActive())
		{
			E.Cell.ParentZone.RegisterSoundRenderer(this);
		}
		return base.HandleEvent(E);
	}
}
