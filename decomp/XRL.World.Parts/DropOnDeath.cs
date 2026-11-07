using System;

namespace XRL.World.Parts;

[Serializable]
public class DropOnDeath : IPart
{
	public string Blueprints;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (!Blueprints.IsNullOrEmpty())
		{
			DroppedEvent droppedEvent = null;
			DropOnDeathEvent dropOnDeathEvent = null;
			DelimitedEnumeratorChar enumerator = Blueprints.DelimitedBy(',').GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Split(':', out var First, out var Second);
				if (First.Length == 0)
				{
					continue;
				}
				int num = 1;
				if (Second.Length > 0)
				{
					num = ((!int.TryParse(Second, out var result)) ? new string(Second).RollCached() : result);
				}
				if (num <= 0)
				{
					continue;
				}
				IInventory dropInventory = ParentObject.GetDropInventory();
				string blueprint = new string(First);
				for (int i = 0; i < num; i++)
				{
					GameObject gameObject = GameObject.Create(blueprint);
					if (dropOnDeathEvent == null)
					{
						dropOnDeathEvent = DropOnDeathEvent.FromPool();
					}
					dropOnDeathEvent.Reset();
					dropOnDeathEvent.Actor = ParentObject;
					dropOnDeathEvent.Inventory = dropInventory;
					dropOnDeathEvent.Object = gameObject;
					dropOnDeathEvent.Type = "DropOnDeath";
					dropOnDeathEvent.Direction = ((dropInventory is Cell target) ? ParentObject.CurrentCell.GetDirectionFromCell(target) : null);
					if (gameObject.HandleEvent(dropOnDeathEvent, E))
					{
						dropInventory.AddObjectToInventory(gameObject, ParentObject, Silent: false, NoStack: false, FlushTransient: true, null, E);
						if (droppedEvent == null)
						{
							droppedEvent = DroppedEvent.FromPool();
						}
						droppedEvent.Reset();
						droppedEvent.Actor = ParentObject;
						droppedEvent.Item = gameObject;
						gameObject.HandleEvent(droppedEvent, E);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}
