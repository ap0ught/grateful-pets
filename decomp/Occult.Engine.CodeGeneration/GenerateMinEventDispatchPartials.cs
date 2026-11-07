using System;

namespace Occult.Engine.CodeGeneration;

[AttributeUsage(AttributeTargets.Class)]
[Obsolete("Use XRL.World.GameEventAttribute.")]
public class GenerateMinEventDispatchPartials : Attribute
{
}
