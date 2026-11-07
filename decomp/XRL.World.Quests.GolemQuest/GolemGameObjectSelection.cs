using System;
using System.Collections.Generic;
using ConsoleLib.Console;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
public abstract class GolemGameObjectSelection : GolemMaterialSelection<GameObject, string>
{
	public string ObjectID;

	public string ObjectDisplayName;

	public Renderable ObjectIcon;

	[NonSerialized]
	private bool Failed;

	[NonSerialized]
	private GameObject _Material;

	[NonSerialized]
	private List<GameObject> _ValidMaterials = new List<GameObject>(8);

	public virtual bool IsCarried => true;

	public virtual int Consumed => 0;

	public override GameObject Material
	{
		get
		{
			if (_Material == null && !Failed && !ObjectID.IsNullOrEmpty())
			{
				_Material = The.ZoneManager.FindObjectByID(ObjectID);
				Failed = _Material == null;
			}
			return _Material;
		}
		set
		{
			_Material = value;
			ObjectID = value?.ID;
			ObjectDisplayName = ((value != null) ? GetNameFor(value) : null);
			ObjectIcon = ((value != null) ? new Renderable(value.Render) : null);
			Failed = false;
		}
	}

	public override List<GameObject> GetValidMaterials()
	{
		_ValidMaterials.Clear();
		if (IsCarried)
		{
			Predicate<GameObject> filter = IsValid;
			foreach (GameObject validHolder in GetValidHolders())
			{
				validHolder.Inventory.GetObjects(_ValidMaterials, filter);
			}
		}
		else
		{
			Zone.ObjectEnumerator enumerator2 = The.ActiveZone.IterateObjects().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				GameObject current = enumerator2.Current;
				if (IsValid(current))
				{
					_ValidMaterials.Add(current);
				}
			}
		}
		return _ValidMaterials;
	}

	public override string GetNameFor(GameObject Material)
	{
		return Material.DisplayName;
	}

	public override IRenderable GetIconFor(GameObject Material)
	{
		return Material.RenderForUI();
	}

	public override void Apply(GameObject Object)
	{
		base.Apply(Object);
		if (GameObject.Validate(Material))
		{
			Object.SetStringProperty(ID + "Blueprint", Material.Blueprint);
			Object.SetStringProperty(ID + "DisplayName", Material.Render.DisplayName);
			if (!Material.HasProperName)
			{
				Object.SetStringProperty(ID + "IndefiniteArticle", Material.a);
			}
		}
		if (Consumed > 0 && GameObject.Validate(Material))
		{
			Material.SplitStack(Consumed);
			Material.Obliterate();
		}
	}

	public GolemGameObjectSelection GetFirstConflict()
	{
		if (Material == null)
		{
			return null;
		}
		if (Consumed <= 0)
		{
			return null;
		}
		Dictionary<string, GolemQuestSelection> dictionary = System?.Selections;
		if (dictionary.IsNullOrEmpty())
		{
			return null;
		}
		foreach (KeyValuePair<string, GolemQuestSelection> item in dictionary)
		{
			if (item.Value is GolemGameObjectSelection golemGameObjectSelection && golemGameObjectSelection != this && golemGameObjectSelection.Consumed > 0 && golemGameObjectSelection.Material == Material)
			{
				return golemGameObjectSelection;
			}
		}
		return null;
	}

	public override bool IsValid()
	{
		if (base.IsValid())
		{
			return GetFirstConflict() == null;
		}
		return false;
	}

	public override bool IsValid(GameObject Object)
	{
		if (!base.IsValid(Object))
		{
			return false;
		}
		GameObject gameObject = Object;
		if (IsCarried)
		{
			gameObject = Object.Holder;
			if (!GolemQuestSelection.IsValidHolder(gameObject))
			{
				return false;
			}
		}
		if (System.Mound != null)
		{
			return System.Mound.ParentObject.InSameZone(gameObject);
		}
		return true;
	}

	public override string GetOptionChoice()
	{
		return ObjectDisplayName;
	}

	private bool IsObjectMatch(GameObject Object)
	{
		return Object.IDMatch(ObjectID);
	}
}
