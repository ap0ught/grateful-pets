namespace XRL.World;

[GameEvent(Base = true, Cascade = 17)]
public abstract class ILimbRegenerationEvent : MinEvent
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public GameObject Actor;

	public GameObject Source;

	public bool Whole;

	public bool All;

	public bool IncludeMinor;

	public bool Voluntary;

	public int? ParentID;

	public int? Category;

	public int[] Categories;

	public int? ExceptCategory;

	public int[] ExceptCategories;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
		Source = null;
		Whole = false;
		All = false;
		IncludeMinor = false;
		Voluntary = false;
		ParentID = null;
		Category = null;
		Categories = null;
		ExceptCategory = null;
		ExceptCategories = null;
	}
}
