public class exUIPointEvent : exUIEvent
{
	public bool altKey;

	public bool ctrlKey;

	public bool metaKey;

	public bool isMouse;

	public exUIPointInfo[] pointInfos;

	public bool isTouch => !isMouse;

	public exUIPointInfo mainPoint => pointInfos[0];

	public bool GetMouseButton(int _id)
	{
		if (isMouse)
		{
			for (int i = 0; i < pointInfos.Length; i++)
			{
				if (pointInfos[i].id == _id)
				{
					return true;
				}
			}
		}
		return false;
	}
}
