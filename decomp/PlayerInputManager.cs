using UnityEngine;
using XRL.UI;

public class PlayerInputManager : MonoBehaviour
{
	public static bool ready;

	public static PlayerInputManager instance => GameManager.Instance?.player;

	public bool GetButton(string id, bool forceEnable = false)
	{
		id = ControlManager.mapRewiredIDToLegacyID(id);
		if (!ready)
		{
			return false;
		}
		return CommandBindingManager.CommandBindings[id].IsPressed(forceEnable);
	}

	public bool GetButtonPerformedThisFrame(string id, bool ignoreSkipframes = false)
	{
		if (!ready)
		{
			return false;
		}
		return ControlManager.isCommandPerformedThisFrame(id);
	}

	public bool GetButtonReleasedThisFrame(string id, bool ignoreSkipframes = false)
	{
		if (!ready)
		{
			return false;
		}
		return ControlManager.isCommandReleasedThisFrame(id);
	}

	public bool GetButtonPressedThisFrame(string id, bool ignoreSkipframes = false)
	{
		if (!ready)
		{
			return false;
		}
		return ControlManager.isCommandPressedThisFrame(id);
	}

	public bool GetButtonDown(string id, bool ignoreSkipframes = false, bool skipTutorialCheck = false)
	{
		if (!ready)
		{
			return false;
		}
		return ControlManager.isCommandDown(id, repeat: false, ignoreSkipframes, skipTutorialCheck);
	}

	public bool GetButtonDownRepeating(string id, bool ignoreSkipframes = false)
	{
		if (!ready)
		{
			return false;
		}
		return ControlManager.isCommandDown(id, repeat: true, ignoreSkipframes);
	}

	public bool GetNegativeButtonDownRepeating(string id, bool ignoreSkipframes = false)
	{
		if (!ready)
		{
			return false;
		}
		return ControlManager.isCommandDown(id, repeat: true, ignoreSkipframes);
	}

	public float GetAxis(string id)
	{
		id = ControlManager.mapRewiredIDToLegacyID(id);
		if (!ready)
		{
			return 0f;
		}
		return CommandBindingManager.CommandBindings[id].ReadValue<float>();
	}

	public Vector2 GetAxis2(string id)
	{
		id = ControlManager.mapRewiredIDToLegacyID(id);
		if (!ready)
		{
			return Vector2.zero;
		}
		return CommandBindingManager.CommandBindings[id].ReadValue<Vector2>();
	}
}
