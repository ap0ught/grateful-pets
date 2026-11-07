using System;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Units;

[Serializable]
public class GameObjectMutationUnit : GameObjectUnit
{
	public string Name;

	public string Class;

	public int Level;

	public bool Enhance = true;

	public bool ShouldShowLevel = true;

	[NonSerialized]
	private MutationEntry _Entry;

	public MutationEntry Entry => _Entry ?? (_Entry = (MutationFactory.TryGetMutationEntry(Name, Class, out _Entry) ? _Entry : null));

	public override void Apply(GameObject Object)
	{
		Mutations mutations = Object.RequirePart<Mutations>();
		BaseMutation baseMutation = mutations.GetMutationByName(Entry?.Name ?? Name) ?? mutations.GetMutation(Entry?.Class ?? Class);
		if (baseMutation != null)
		{
			if (Enhance)
			{
				mutations.LevelMutation(baseMutation, baseMutation.BaseLevel + Level);
			}
			return;
		}
		string text = Entry?.Class ?? Class;
		if (text != null)
		{
			mutations.AddMutation(text, Level);
		}
	}

	public override void Remove(GameObject Object)
	{
		if (Object.GetPart(Entry?.Class ?? Class) is BaseMutation mutation)
		{
			Object.GetPart<Mutations>()?.RemoveMutation(mutation);
		}
	}

	public override void Reset()
	{
		base.Reset();
		Name = null;
		Class = null;
		_Entry = null;
		Level = 0;
		Enhance = true;
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (Name == null)
		{
			Name = Entry?.GetDisplayName();
		}
		if (Name.IsNullOrEmpty() && !Class.IsNullOrEmpty())
		{
			try
			{
				BaseMutation baseMutation = Entry?.Mutation ?? Mutations.GetGenericMutation(Class);
				Name = baseMutation.GetDisplayName();
				ShouldShowLevel = baseMutation.ShouldShowLevel();
			}
			catch
			{
				Name = "ERR:" + Class;
			}
		}
		if (!ShouldShowLevel)
		{
			return Name;
		}
		return Name + " at level " + Level;
	}
}
