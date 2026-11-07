using UnityEngine;
using XRL.UI;

public class ScaleResponsiveElement : MonoBehaviour
{
	public Transform target;

	public float SmallScale;

	public float MediumScale;

	public float LargeScale;

	public bool ControlX;

	public bool ControlY;

	public bool ControlZ;

	private Media.SizeClass lastClass = Media.SizeClass.Unset;

	private float GetTargetScale()
	{
		if (Media.sizeClass <= Media.SizeClass.Small)
		{
			return SmallScale;
		}
		if (Media.sizeClass >= Media.SizeClass.Large)
		{
			return LargeScale;
		}
		return MediumScale;
	}

	private void Check()
	{
		if (lastClass != Media.sizeClass)
		{
			lastClass = Media.sizeClass;
			float targetScale = GetTargetScale();
			target.transform.localScale = new Vector3(ControlX ? targetScale : target.transform.localScale.x, ControlY ? targetScale : target.transform.localScale.y, ControlZ ? targetScale : target.transform.localScale.z);
		}
	}

	private void Awake()
	{
		Check();
	}

	private void Update()
	{
		Check();
	}
}
