using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XRL.World.Parts;

[Serializable]
public class Mill : IPoweredPart
{
	public string _Transformations;

	public string _TagTransformations;

	[NonSerialized]
	private Dictionary<string, string> Transforms;

	[NonSerialized]
	private Dictionary<string, string> TagTransforms;

	public string Transformations
	{
		get
		{
			return _Transformations;
		}
		set
		{
			_Transformations = value;
			Transforms = null;
		}
	}

	public string TagTransformations
	{
		get
		{
			return _TagTransformations;
		}
		set
		{
			_TagTransformations = value;
			TagTransforms = null;
		}
	}

	public Mill()
	{
		ChargeUse = 1;
		WorksOnInventory = true;
	}

	public override bool SameAs(IPart p)
	{
		Mill mill = p as Mill;
		if (mill._Transformations != _Transformations)
		{
			return false;
		}
		if (mill._TagTransformations != _TagTransformations)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	private bool ProcessItem(GameObject obj)
	{
		string transform = GetTransform(obj);
		if (transform == null)
		{
			return true;
		}
		if (transform == "")
		{
			Butcherable part = obj.GetPart<Butcherable>();
			if (part != null && part.AttemptButcher(ParentObject, Automatic: false, SkipSkill: true, IntoInventory: true))
			{
				return false;
			}
			if (obj.GetPart<PreservableItem>() != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(ParentObject.The).Append(ParentObject.DisplayNameOnly).Append(ParentObject.GetVerb("mill"))
					.Append(' ');
				if (Campfire.PerformPreserve(obj, stringBuilder, ParentObject, Capitalize: false, Single: true))
				{
					if (Visible())
					{
						stringBuilder.Append('.');
						IComponent<GameObject>.AddPlayerMessage(stringBuilder.ToString());
					}
					return false;
				}
			}
		}
		else
		{
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			bool flag = Visible();
			if (flag)
			{
				stringBuilder2.Append(ParentObject.The).Append(ParentObject.DisplayNameOnly).Append(ParentObject.GetVerb("mill"))
					.Append(' ')
					.Append(obj.a)
					.Append(obj.DisplayNameOnly);
			}
			GameObject gameObject = obj.ReplaceWith(transform);
			if ((gameObject != null) & flag)
			{
				stringBuilder2.Append(" into").Append(gameObject.a).Append(gameObject.DisplayNameOnly)
					.Append('.');
				IComponent<GameObject>.AddPlayerMessage(stringBuilder2.ToString());
			}
		}
		return true;
	}

	public bool ProcessItems()
	{
		bool flag = ForeachActivePartSubjectWhile(ProcessItem, MayMoveAddOrDestroy: true);
		if (!flag && ChargeUse > 0)
		{
			ParentObject.UseCharge(ChargeUse, LiveOnly: false, 0L);
		}
		return flag;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ProcessItems();
		}
		return base.FireEvent(E);
	}

	public string GetTransform(GameObject obj)
	{
		if (Transforms == null)
		{
			DetermineTransforms();
		}
		if (Transforms.TryGetValue(obj.Blueprint, out var value))
		{
			return value;
		}
		if (TagTransforms == null)
		{
			DetermineTagTransforms();
		}
		foreach (KeyValuePair<string, string> tagTransform in TagTransforms)
		{
			if (obj.HasTag(tagTransform.Key))
			{
				return tagTransform.Value;
			}
		}
		return null;
	}

	private void DetermineTransforms()
	{
		if (string.IsNullOrEmpty(_Transformations))
		{
			Transforms = new Dictionary<string, string>(0);
			return;
		}
		List<string> list = _Transformations.CachedCommaExpansion();
		Transforms = new Dictionary<string, string>(list.Count);
		foreach (string item in list)
		{
			string[] array = item.Split(':');
			if (Transforms.ContainsKey(array[0]))
			{
				Debug.LogError("duplicate transformation on " + array[0]);
			}
			else if (array.Length > 1)
			{
				Transforms.Add(array[0], array[1]);
			}
			else
			{
				Transforms.Add(array[0], "");
			}
		}
	}

	private void DetermineTagTransforms()
	{
		if (string.IsNullOrEmpty(_TagTransformations))
		{
			TagTransforms = new Dictionary<string, string>(0);
			return;
		}
		List<string> list = _TagTransformations.CachedCommaExpansion();
		TagTransforms = new Dictionary<string, string>(list.Count);
		foreach (string item in list)
		{
			string[] array = item.Split(':');
			if (TagTransforms.ContainsKey(array[0]))
			{
				Debug.LogError("duplicate tag transformation on " + array[0]);
			}
			else if (array.Length > 1)
			{
				TagTransforms.Add(array[0], array[1]);
			}
			else
			{
				TagTransforms.Add(array[0], "");
			}
		}
	}
}
