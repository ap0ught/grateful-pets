using System;

namespace XRL.CharacterBuilds;

/// <summary>
///     AbstractQudEmbarkBuilderModule with a specific data type.
/// </summary>
/// <typeparam name="T">An EmbarkBuilderModuleData type.</typeparam>
public abstract class QudEmbarkBuilderModule<T> : AbstractQudEmbarkBuilderModule where T : AbstractEmbarkBuilderModuleData
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
