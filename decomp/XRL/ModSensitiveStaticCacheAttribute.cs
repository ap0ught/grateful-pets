using System;

namespace XRL;

/// <summary>
///     Flag a static field for "reset" when mods reload.  The class should have <see cref="T:XRL.HasModSensitiveStaticCacheAttribute" />.
///     <para>
///         The default mode is to set the value to <see cref="!:null" />, but can be set to an empty instance with the
///         <see cref="F:XRL.ModSensitiveStaticCacheAttribute.CreateEmptyInstance" /> field or constructor parameters.
///     </para>
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class ModSensitiveStaticCacheAttribute : Attribute
{
	public bool CreateEmptyInstance;

	public ModSensitiveStaticCacheAttribute(bool createEmptyInstance = false)
	{
		CreateEmptyInstance = createEmptyInstance;
	}
}
