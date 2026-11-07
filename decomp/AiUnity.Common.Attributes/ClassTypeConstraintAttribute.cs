using System;
using AiUnity.Common.Serialization.ClassTypeReference;
using UnityEngine;

namespace AiUnity.Common.Attributes;

public abstract class ClassTypeConstraintAttribute : PropertyAttribute
{
	public GUIContent LabelContent;

	private bool _allowAbstract;

	private ClassGrouping _grouping = ClassGrouping.ByNamespaceFlat;

	public bool AllowAbstract
	{
		get
		{
			return _allowAbstract;
		}
		set
		{
			_allowAbstract = value;
		}
	}

	public ClassGrouping Grouping
	{
		get
		{
			return _grouping;
		}
		set
		{
			_grouping = value;
		}
	}

	public ClassTypeConstraintAttribute(GUIContent labelContent)
	{
		LabelContent = labelContent;
	}

	public ClassTypeConstraintAttribute(string labelName, string tooltip = "")
	{
		LabelContent = new GUIContent(labelName, tooltip);
	}

	public virtual bool IsConstraintSatisfied(Type type)
	{
		if (!AllowAbstract)
		{
			return !type.IsAbstract;
		}
		return true;
	}
}
