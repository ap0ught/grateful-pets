using System;
using System.Reflection;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnit : IComposite
{
	[NonSerialized]
	public ProceduralCookingEffect parent;

	public virtual bool WantFieldReflection => true;

	public virtual string GetDescription()
	{
		return "[effect]";
	}

	public virtual string GetTemplatedDescription()
	{
		return GetDescription();
	}

	public virtual void Apply(GameObject go, Effect parent)
	{
	}

	public virtual void Remove(GameObject go, Effect parent)
	{
	}

	public virtual void FireEvent(Event E)
	{
	}

	public virtual void Init(GameObject target)
	{
	}

	public virtual void Write(SerializationWriter Writer)
	{
	}

	public virtual void Read(SerializationReader Reader)
	{
	}

	public virtual ProceduralCookingEffectUnit DeepCopy(ProceduralCookingEffect Parent)
	{
		Type type = GetType();
		ProceduralCookingEffectUnit proceduralCookingEffectUnit = (ProceduralCookingEffectUnit)Activator.CreateInstance(type);
		proceduralCookingEffectUnit.parent = Parent;
		FieldInfo[] cachedFields = type.GetCachedFields();
		foreach (FieldInfo fieldInfo in cachedFields)
		{
			if ((fieldInfo.Attributes & (FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)) == 0)
			{
				fieldInfo.SetValue(proceduralCookingEffectUnit, fieldInfo.GetValue(this));
			}
		}
		return proceduralCookingEffectUnit;
	}
}
