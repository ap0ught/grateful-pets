using System;
using UnityEngine;

namespace XRL.UI.Framework;

public class FrameworkContext : MonoBehaviour
{
	public virtual NavigationContext context { get; set; }

	public T RequireContext<T>() where T : NavigationContext
	{
		if (context != null)
		{
			T val = context as T;
			_ = val;
			return val;
		}
		context = Activator.CreateInstance<T>();
		return context as T;
	}
}
