using System;

namespace XRL.World.Parts;

[Serializable]
public class StringPropertyChangeDuringEffect : IPart
{
	public string PropertyName;

	public string EffectName;

	public string Value;

	public bool TriggerRepaint;

	public bool Active;

	public bool HadProperty;

	public string OriginalValue;

	public string RemoveStringPropertyOnTransition;

	public override bool SameAs(IPart p)
	{
		StringPropertyChangeDuringEffect stringPropertyChangeDuringEffect = p as StringPropertyChangeDuringEffect;
		if (stringPropertyChangeDuringEffect.PropertyName != PropertyName)
		{
			return false;
		}
		if (stringPropertyChangeDuringEffect.EffectName != EffectName)
		{
			return false;
		}
		if (stringPropertyChangeDuringEffect.Value != Value)
		{
			return false;
		}
		if (stringPropertyChangeDuringEffect.TriggerRepaint != TriggerRepaint)
		{
			return false;
		}
		if (stringPropertyChangeDuringEffect.Active != Active)
		{
			return false;
		}
		if (stringPropertyChangeDuringEffect.HadProperty != HadProperty)
		{
			return false;
		}
		if (stringPropertyChangeDuringEffect.OriginalValue != OriginalValue)
		{
			return false;
		}
		if (stringPropertyChangeDuringEffect.RemoveStringPropertyOnTransition != RemoveStringPropertyOnTransition)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID)
		{
			return ID == EffectRemovedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		Sync();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		Sync();
		return base.HandleEvent(E);
	}

	public void Sync()
	{
		bool flag = false;
		if (Active)
		{
			if (!ParentObject.HasEffect(EffectName))
			{
				if (HadProperty)
				{
					ParentObject.SetStringProperty(PropertyName, OriginalValue);
				}
				else
				{
					ParentObject.RemoveStringProperty(PropertyName);
				}
				HadProperty = false;
				OriginalValue = null;
				Active = false;
				flag = true;
			}
		}
		else if (ParentObject.HasEffect(EffectName))
		{
			HadProperty = ParentObject.HasStringProperty(PropertyName);
			OriginalValue = (HadProperty ? ParentObject.GetStringProperty(PropertyName) : null);
			ParentObject.SetStringProperty(PropertyName, Value);
			Active = true;
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (!RemoveStringPropertyOnTransition.IsNullOrEmpty())
		{
			ParentObject.RemoveStringProperty(RemoveStringPropertyOnTransition);
		}
		if (TriggerRepaint)
		{
			Zone currentZone = ParentObject.CurrentZone;
			if (currentZone != null && currentZone.Built)
			{
				ParentObject.CurrentCell.PaintWallsAround();
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
