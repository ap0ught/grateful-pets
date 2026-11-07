using Newtonsoft.Json;
using Steamworks;
using UnityEngine;
using XRL.Serialization;

namespace XRL;

public class SteamWorkshopInfo
{
	public ulong WorkshopId;

	public string Title;

	public string Description;

	[JsonConverter(typeof(CommaDelimitedArrayConverter))]
	public string[] Tags;

	public string Visibility;

	public string ImagePath;

	/// <summary>
	/// Open the mod's workshop page in the steam overlay, client, or browser.
	/// </summary>
	public void OpenWorkshopPage()
	{
		if (WorkshopId == 0L)
		{
			return;
		}
		if (PlatformManager.IsInitialized())
		{
			string text = "steam://url/CommunityFilePage/" + WorkshopId;
			if (SteamUtils.IsOverlayEnabled())
			{
				SteamFriends.ActivateGameOverlayToWebPage(text);
			}
			else
			{
				Application.OpenURL(text);
			}
		}
		else
		{
			Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + WorkshopId);
		}
	}
}
