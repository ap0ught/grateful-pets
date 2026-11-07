namespace XRL.World;

/// <summary>A static singleton event which is dispatched to a <see cref="T:XRL.IEventHandler" /> that implements <see cref="T:XRL.World.IModEventHandler`1" />.</summary>
/// <example>
/// <code>
/// public class ExampleSingletonEvent : ModSingletonEvent&lt;ExampleSingletonEvent&gt;
/// {
///
///     public static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_EXCEPT_THROWN_WEAPON;
///
///     public bool Test;
///
///     public static bool Check(GameObject Object)
///     {
///         Object.HandleEvent(Instance);
///         var result = Instance.Test;
///         Instance.Reset();
///
///         return result;
///     }
///
///     public override void Reset()
///     {
///         base.Reset();
///         Test = false;
///     }
///
///     public override int GetCascadeLevel()
///     {
///         return CascadeLevel;
///     }
///
/// }
/// </code>
/// </example>
public abstract class ModSingletonEvent<T> : SingletonEvent<T> where T : ModSingletonEvent<T>, new()
{
	public override bool Dispatch(IEventHandler Handler)
	{
		if (Handler is IModEventHandler<T> modEventHandler)
		{
			return modEventHandler.HandleEvent((T)this);
		}
		if (Handler is IEventSource)
		{
			return Handler.HandleEvent(this);
		}
		return true;
	}
}
