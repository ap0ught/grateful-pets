using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
	public GameObject[] joystickDirectionIndicators;

	private Dictionary<string, int> joystickDirectionIndexes = new Dictionary<string, int>
	{
		{ "N", 0 },
		{ "NE", 1 },
		{ "E", 2 },
		{ "SE", 3 },
		{ "S", 4 },
		{ "SW", 5 },
		{ "W", 6 },
		{ "NW", 7 }
	};

	public GameObject activeJoystickDirection;

	public void setActiveDirection(string dir)
	{
		if (activeJoystickDirection != null)
		{
			activeJoystickDirection.SetActive(value: false);
			activeJoystickDirection = null;
		}
		if (dir != null && joystickDirectionIndexes.ContainsKey(dir))
		{
			activeJoystickDirection = joystickDirectionIndicators[joystickDirectionIndexes[dir]];
			if (!activeJoystickDirection.activeSelf)
			{
				activeJoystickDirection.SetActive(value: true);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
