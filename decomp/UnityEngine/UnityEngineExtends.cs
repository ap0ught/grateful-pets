namespace UnityEngine;

public static class UnityEngineExtends
{
	public static T Instantiate<T>(this T original) where T : Object
	{
		return Object.Instantiate(original);
	}

	public static T Instantiate<T>(this T original, Vector3 position, Quaternion rotation) where T : Object
	{
		return Object.Instantiate(original, position, rotation);
	}

	public static void Destroy(this GameObject obj)
	{
		obj.transform.SetParent(null);
		Object.Destroy(obj);
	}

	public static void Destroy(this Object obj)
	{
		Object.Destroy(obj);
	}

	public static void Destroy(this GameObject obj, float waitTime)
	{
		obj.transform.parent = null;
		((Object)obj).Destroy(waitTime);
	}

	public static void Destroy(this Object obj, float waitTime)
	{
		Object.Destroy(obj, waitTime);
	}

	public static void DestroyImmediate(this GameObject obj)
	{
		obj.transform.SetParent(null);
		Object.DestroyImmediate(obj);
	}

	public static void DestroyImmediate(this GameObject obj, bool allowDestroyingAssets)
	{
		obj.transform.parent = null;
		Object.DestroyImmediate(obj, allowDestroyingAssets);
	}

	public static void DestroyImmediate(this Object obj)
	{
		Object.DestroyImmediate(obj);
	}

	public static void DestroyImmediate(this Object obj, bool allowDestroyingAssets)
	{
		Object.DestroyImmediate(obj, allowDestroyingAssets);
	}

	public static void SetLossyScale(this Transform trans, Vector3 worldScale)
	{
		Vector3 lossyScale = trans.lossyScale;
		Vector3 localScale = trans.localScale;
		trans.localScale = new Vector3(worldScale.x / lossyScale.x * localScale.x, worldScale.y / lossyScale.y * localScale.y, worldScale.z / lossyScale.z * localScale.z);
	}

	public static T GetComponentUpwards<T>(this GameObject _go) where T : Component
	{
		Transform parent = _go.transform.parent;
		while (parent != null)
		{
			T val = parent.GetComponent(typeof(T)) as T;
			if (val != null)
			{
				return val;
			}
			parent = parent.parent;
		}
		return null;
	}

	public static T FindParentComponent<T>(this T component) where T : Component
	{
		return component.gameObject.GetComponentUpwards<T>();
	}

	public static T GetComponentInChildrenFast<T>(this GameObject go) where T : Component
	{
		if (go.activeInHierarchy)
		{
			Component component = go.GetComponent(typeof(T));
			if (component != null)
			{
				return component as T;
			}
		}
		Transform transform = go.transform;
		if (transform != null)
		{
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				T componentInChildrenFast = transform.GetChild(i).gameObject.GetComponentInChildrenFast<T>();
				if (componentInChildrenFast != null)
				{
					return componentInChildrenFast;
				}
			}
		}
		return null;
	}

	public static T GetComponentInChildrenFast<T>(this Component component) where T : Component
	{
		return component.gameObject.GetComponentInChildrenFast<T>();
	}
}
