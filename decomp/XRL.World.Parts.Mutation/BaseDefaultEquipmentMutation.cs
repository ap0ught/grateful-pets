using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

public class BaseDefaultEquipmentMutation : BaseMutation
{
	public Dictionary<string, int> RegisteredSlots;

	public bool HasRegisteredSlot(string id)
	{
		if (RegisteredSlots == null)
		{
			return false;
		}
		return RegisteredSlots.ContainsKey(id);
	}

	public int RegisterSlot(string id, BodyPart part)
	{
		if (RegisteredSlots == null)
		{
			RegisteredSlots = new Dictionary<string, int>();
		}
		RegisteredSlots[id] = part.ID;
		return part.ID;
	}

	public BodyPart GetRegisteredSlot(string id, bool evenIfDismembered)
	{
		if (id == null)
		{
			return null;
		}
		if (ParentObject == null)
		{
			return null;
		}
		if (RegisteredSlots == null)
		{
			return null;
		}
		int value = 0;
		RegisteredSlots.TryGetValue(id, out value);
		return ParentObject.Body?.GetPartByID(value, evenIfDismembered);
	}

	public BodyPart GetRegisteredSlot(Body Body, string Type)
	{
		if (RegisteredSlots != null && RegisteredSlots.TryGetValue(Type, out var value))
		{
			return Body._Body.GetPartByID(value);
		}
		return null;
	}

	public BodyPart RequireRegisteredSlot(Body Body, string Type)
	{
		if (TryGetRegisteredSlot(Body, Type, out var Part))
		{
			return Part;
		}
		Part = Body.GetFirstPart(Type);
		if (Part == null)
		{
			return null;
		}
		if (RegisteredSlots == null)
		{
			RegisteredSlots = new Dictionary<string, int>();
		}
		RegisteredSlots[Type] = Part.ID;
		return Part;
	}

	/// <returns>
	/// True if registration exists and is valid, false if new registration should be made.
	/// Can still output null for a true case if dismembered.
	/// </returns>
	public bool TryGetRegisteredSlot(Body Body, string Type, out BodyPart Part, bool EvenIfDismembered = false)
	{
		if (RegisteredSlots != null && RegisteredSlots.TryGetValue(Type, out var value))
		{
			Part = Body._Body.GetPartByID(value);
			if (Part != null)
			{
				return true;
			}
			Part = Body.GetDismemberedPartByID(value);
			if (Part == null)
			{
				RegisteredSlots.Remove(Type);
				return false;
			}
			if (!EvenIfDismembered)
			{
				Part = null;
			}
			return true;
		}
		Part = null;
		return false;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		ParentObject?.Body?.UpdateBodyParts();
		return base.ChangeLevel(NewLevel);
	}

	public override void AfterMutate()
	{
		ParentObject?.Body?.UpdateBodyParts();
		base.AfterMutate();
	}

	public override void AfterUnmutate(GameObject GO)
	{
		GO?.Body?.UpdateBodyParts();
		base.AfterUnmutate(GO);
	}

	public virtual void OnRegenerateDefaultEquipment(Body body)
	{
	}

	public virtual void OnDecorateDefaultEquipment(Body body)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<DecorateDefaultEquipmentEvent>.ID)
		{
			return ID == PooledEvent<RegenerateDefaultEquipmentEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(RegenerateDefaultEquipmentEvent E)
	{
		OnRegenerateDefaultEquipment(E.Body);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DecorateDefaultEquipmentEvent E)
	{
		OnDecorateDefaultEquipment(E.Body);
		return base.HandleEvent(E);
	}
}
