using System;

namespace XRL;

/// <summary>
///     Flag a static field for "reset" when game starts.  The class should have <see cref="T:XRL.HasGameBasedStaticCacheAttribute" />.
///     <para>
///         The default mode is to set the value to an empty instance, but can be toggled to set to null instead with the
///         <see cref="F:XRL.GameBasedStaticCacheAttribute.CreateInstance" /> field or constructor parameters.
///     </para>
///     <para>
///         <see cref="F:XRL.GameBasedStaticCacheAttribute.ClearInstance" /> will not reset the value, instead calling Clear() method on it.
///     </para>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class GameBasedStaticCacheAttribute : Attribute
{
	public bool CreateInstance = true;

	/// <summary>
	/// If cache implements <see cref="T:System.Collections.IDictionary" />, <see cref="T:System.Collections.IList" />
	/// or otherwise defines a parameterless Clear method, invoke it.
	/// </summary>
	public bool ClearInstance;

	public GameBasedStaticCacheAttribute(bool CreateInstance = true, bool ClearInstance = false)
	{
		this.CreateInstance = CreateInstance;
		this.ClearInstance = ClearInstance;
	}
}
