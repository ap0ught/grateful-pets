namespace XRL.World;

/// <summary>A pooled event which is dispatched to a <see cref="T:XRL.IEventHandler" /> that implements <see cref="T:XRL.World.IModEventHandler`1" />.</summary>
/// <example>
/// <code>
/// public class ExamplePooledEvent : ModPooledEvent&lt;ExamplePooledEvent&gt;
/// {
///
///     public static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_EXCEPT_THROWN_WEAPON;
///
///     public bool Test;
///
///     public static bool Check(GameObject Object)
///     {
///         var E = FromPool();
///         Object.HandleEvent(E);
///         var result = E.Test;
///         ResetTo(ref E);
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
public abstract class ModPooledEvent<T> : PooledEvent<T> where T : ModPooledEvent<T>, new()
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
