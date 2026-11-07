using System;
using XRL.Collections;

namespace XRL.World.AI;

public class OpinionList : Container<IOpinion>, IComposite
{
	public int Total;

	public IOpinion this[int Index]
	{
		get
		{
			if ((uint)Index >= (uint)Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Items[Index];
		}
		set
		{
			if ((uint)Index >= (uint)Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			Items[Index] = value;
			Variant++;
		}
	}

	public void RefreshTotal()
	{
		Total = 0;
		for (int i = 0; i < Length; i++)
		{
			Total += Items[i].Value;
		}
	}

	public virtual void Add(IOpinion Item)
	{
		if (Length == Size)
		{
			Resize(Length * 2);
		}
		Items[Length++] = Item;
		Total += Item.Value;
		Variant++;
	}

	public bool Remove(IOpinion Item)
	{
		int num = Array.IndexOf(Items, Item, 0, Length);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public void RemoveAt(int Index)
	{
		if (Index >= Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		Length--;
		Total -= Items[Index].Value;
		if (Index < Length)
		{
			Array.Copy(Items, Index + 1, Items, Index, Length - Index);
		}
		Items[Length] = null;
		Variant++;
	}

	public IOpinion Find(Predicate<IOpinion> Predicate)
	{
		for (int i = 0; i < Length; i++)
		{
			if (Predicate(Items[i]))
			{
				return Items[i];
			}
		}
		return null;
	}

	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Length);
		for (int i = 0; i < Length; i++)
		{
			Writer.Write(Items[i]);
		}
	}

	public override void Read(SerializationReader Reader)
	{
		Size = (Length = Reader.ReadOptimizedInt32());
		Items = new IOpinion[Size];
		Total = 0;
		for (int i = 0; i < Length; i++)
		{
			Items[i] = (IOpinion)Reader.ReadComposite();
			Total += Items[i].Value;
		}
	}
}
