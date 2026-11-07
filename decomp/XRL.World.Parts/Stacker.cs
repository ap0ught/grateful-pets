using System;
using Newtonsoft.Json;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Stacker : IPart
{
	[NonSerialized]
	public int _StackCount = 1;

	[JsonIgnore]
	public int Number
	{
		get
		{
			if (StackCount <= 0)
			{
				StackCount = 1;
			}
			return StackCount;
		}
	}

	public int StackCount
	{
		get
		{
			return _StackCount;
		}
		set
		{
			if (value != _StackCount)
			{
				int stackCount = _StackCount;
				_StackCount = value;
				StackCountChangedEvent.Send(ParentObject, stackCount, _StackCount);
			}
		}
	}

	public bool Check()
	{
		if (ParentObject == null)
		{
			return false;
		}
		GameObject inInventory = ParentObject.InInventory;
		if (inInventory != null && CheckInventoryForStacking(inInventory))
		{
			return true;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && CheckCellForStacking(cell))
		{
			return true;
		}
		return false;
	}

	public GameObject RemoveOne()
	{
		if (StackCount > 1)
		{
			StackCount--;
			if (HasTag("AlwaysStack"))
			{
				GameObject gameObject = GameObject.Create(ParentObject.Blueprint);
				Temporary.CarryOver(ParentObject, gameObject);
				Phase.carryOver(ParentObject, gameObject);
				return gameObject;
			}
			GameObject gameObject2 = ParentObject.DeepCopy(CopyEffects: true);
			gameObject2.Stacker.StackCount = 1;
			gameObject2.WasUnstackedFrom(ParentObject);
			return gameObject2;
		}
		return ParentObject;
	}

	public override void Attach()
	{
		ParentObject.Stacker = this;
	}

	public override void Remove()
	{
		if (ParentObject?.Stacker == this)
		{
			ParentObject.Stacker = null;
		}
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		return new Stacker
		{
			StackCount = StackCount,
			ParentObject = Parent
		};
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(StackCount);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		StackCount = Reader.ReadInt32();
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != BeforeDestroyObjectEvent.ID && ID != EnteredCellEvent.ID && ID != EquippedEvent.ID)
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!E.NoStack && CheckCellForStacking())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		if (!E.NoStack && CheckInventoryForStacking(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (Number > 1)
		{
			GameObject gameObject;
			if (ParentObject.HasTag("AlwaysStack"))
			{
				gameObject = GameObject.Create(ParentObject.Blueprint);
				gameObject.Stacker.StackCount = StackCount - 1;
				Temporary.CarryOver(ParentObject, gameObject);
				Phase.carryOver(ParentObject, gameObject);
				StackCount = 1;
			}
			else
			{
				gameObject = ParentObject.DeepCopy(CopyEffects: true);
				gameObject.Stacker.StackCount = StackCount - 1;
				StackCount = 1;
				ParentObject.WasUnstackedFrom(gameObject);
			}
			E.Actor.ReceiveObject(gameObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Single && !ParentObject.HasTag("SingleDisplay"))
		{
			if (ParentObject.HasTag("SelfRenderStackerDisplay"))
			{
				Event obj = Event.New("SelfRenderStackerDisplay");
				ParentObject.FireEvent(obj);
				string stringParameter = obj.GetStringParameter("Display");
				if (!string.IsNullOrEmpty(stringParameter))
				{
					E.AddTag(stringParameter, 60);
				}
			}
			else if (ParentObject.HasTag("HasCustomStackerDisplay"))
			{
				if (Number <= 1)
				{
					E.AddTag(ParentObject.GetTag("StackerSingularPrefix") + Number + ParentObject.GetTag("StackerSingularPostfix"), 60);
				}
				else
				{
					E.AddTag(ParentObject.GetTag("StackerPluralPrefix") + Number + ParentObject.GetTag("StackerPluralPostfix"), 60);
				}
			}
			else if (Number > 1)
			{
				E.AddClause("x" + Number, 60);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		if (!E.Obliterate && StackCount > 1)
		{
			StackCount--;
			ParentObject?.InInventory?.Inventory?.FlushWeightCache();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public GameObject SplitStack(int Count, GameObject OwningObject = null, bool NoRemove = false)
	{
		if (Number <= 1)
		{
			return null;
		}
		int num = Number - Count;
		if (num <= 0)
		{
			return null;
		}
		GameObject gameObject;
		if (ParentObject.HasTag("AlwaysStack"))
		{
			gameObject = GameObject.Create(ParentObject.Blueprint);
			Temporary.CarryOver(ParentObject, gameObject);
			Phase.carryOver(ParentObject, gameObject);
		}
		else
		{
			gameObject = ParentObject.DeepCopy(CopyEffects: true);
		}
		if (!GameObject.Validate(gameObject))
		{
			gameObject.Obliterate(null, Silent: true);
			return null;
		}
		gameObject.Stacker.StackCount = num;
		StackCount -= num;
		gameObject.WasUnstackedFrom(ParentObject);
		if (OwningObject == null)
		{
			OwningObject = ParentObject.InInventory ?? ParentObject.Equipped;
			if (ParentObject.HasTag("AlwaysStack") && OwningObject != null && !NoRemove)
			{
				OwningObject.FireEvent(Event.New("CommandRemoveObject", "Object", ParentObject).SetSilent(Silent: true));
			}
		}
		Inventory inventory = OwningObject?.Inventory;
		if (inventory != null)
		{
			gameObject.Physics.InInventory = inventory.ParentObject;
			inventory.Objects.Add(gameObject);
		}
		else
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				gameObject.CurrentCell = cell;
				cell.Objects.Add(gameObject);
			}
		}
		return gameObject;
	}

	private bool CheckInventoryForStacking(GameObject obj)
	{
		if (GameObject.Validate(ParentObject) && !ParentObject.HasPropertyOrTag("NeverStack"))
		{
			Inventory inventory = obj?.Inventory;
			if (inventory != null && AllowInventoryStackEvent.Check(obj, ParentObject))
			{
				for (int num = inventory.Objects.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = inventory.Objects[num];
					if (gameObject != ParentObject && gameObject.HasPart<Stacker>() && !gameObject.HasPropertyOrTag("NeverStack"))
					{
						bool flag = gameObject.HasTag("AlwaysStack") && ParentObject.Blueprint == gameObject.Blueprint;
						if (flag && gameObject.IsTemporary != ParentObject.IsTemporary)
						{
							flag = false;
						}
						if (flag || gameObject.SameAs(ParentObject))
						{
							bool flag2 = !flag;
							if (!flag2)
							{
								if (!gameObject.IsTemporary)
								{
									flag2 = true;
								}
								else
								{
									Temporary part = gameObject.GetPart<Temporary>();
									Temporary part2 = ParentObject.GetPart<Temporary>();
									ExistenceSupport part3 = gameObject.GetPart<ExistenceSupport>();
									ExistenceSupport part4 = ParentObject.GetPart<ExistenceSupport>();
									flag2 = ((part == null && part2 == null) || (part != null && part2 != null && part.SameAs(part2))) && ((part3 == null && part4 == null) || (part3 != null && part4 != null && part3.SameAs(part4)));
								}
							}
							if (flag2)
							{
								gameObject.Stacker.StackCount += StackCount;
								ParentObject.Obliterate(null, Silent: true);
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	private bool CheckCellForStacking(Cell C = null)
	{
		if (GameObject.Validate(ParentObject) && !ParentObject.HasPropertyOrTag("NeverStack"))
		{
			if (C == null)
			{
				C = ParentObject.CurrentCell;
			}
			if (C != null)
			{
				Cell.ObjectRack objects = ParentObject.CurrentCell.Objects;
				for (int num = objects.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = objects[num];
					Stacker stacker = gameObject.Stacker;
					if (stacker != null && gameObject != ParentObject && !gameObject.HasPropertyOrTag("NeverStack") && ((gameObject.HasTag("AlwaysStack") && ParentObject.Blueprint == gameObject.Blueprint) || gameObject.SameAs(ParentObject)) && gameObject.HasPart<Temporary>() == ParentObject.HasPart<Temporary>())
					{
						stacker.StackCount += StackCount;
						ParentObject.Obliterate(null, Silent: true);
						return true;
					}
				}
			}
		}
		return false;
	}
}
