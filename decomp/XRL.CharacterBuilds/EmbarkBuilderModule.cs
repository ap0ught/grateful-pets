using System;

namespace XRL.CharacterBuilds;

/// <summary>
///     AbstractEmbarkBuilderModule with a specific data type.
/// </summary>
/// <typeparam name="T">An EmbarkBuilderModuleData type.</typeparam>
public abstract class EmbarkBuilderModule<T> : AbstractEmbarkBuilderModule where T : AbstractEmbarkBuilderModuleData
{
	public T data
	{
		get
		{
			return getData() as T;
		}
		set
		{
			setData(value);
		}
	}

	public override Type getDataType()
	{
		return typeof(T);
	}
}
