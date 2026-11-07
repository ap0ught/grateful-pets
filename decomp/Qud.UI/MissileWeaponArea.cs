using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace Qud.UI;

public class MissileWeaponArea : MonoBehaviour
{
	public class MissileWeaponAreaWeaponStatus
	{
		public bool updated;

		public string text;

		public string display;

		public IRenderable renderable;

		public int ammoTotal;

		public int ammoRemaining;

		public MissileWeaponArea area;

		public MissileWeaponAreaInfo infoObject;

		public int displayAmmoTotalBars
		{
			get
			{
				if (ammoTotal > 30)
				{
					return (int)Math.Ceiling((float)ammoTotal / 5f);
				}
				return ammoTotal;
			}
		}

		public int displayAmmoRemainingBars
		{
			get
			{
				if (ammoTotal > 30)
				{
					return (int)Math.Ceiling((float)ammoRemaining / 5f);
				}
				return ammoRemaining;
			}
		}

		public void pool()
		{
			text = null;
			display = null;
			renderable = null;
			ammoTotal = 0;
			ammoRemaining = 0;
			if (infoObject != null)
			{
				UnityEngine.Object.Destroy(infoObject.gameObject);
			}
			infoObject = null;
			area = null;
		}

		public static MissileWeaponAreaWeaponStatus next()
		{
			if (statusPool.Count > 0)
			{
				return statusPool.Pop();
			}
			return new MissileWeaponAreaWeaponStatus();
		}

		public void update()
		{
			if (infoObject == null)
			{
				infoObject = UnityEngine.Object.Instantiate(area.missileWeaponInfoPrefab).GetComponent<MissileWeaponAreaInfo>();
				infoObject.gameObject.SetActive(value: true);
				infoObject.transform.parent = area.missileWeaponList.transform;
			}
			infoObject.UpdateFrom(this);
		}
	}

	public UITextSkin fireHotkeyText;

	public UITextSkin reloadHotkeyText;

	public UnityEngine.GameObject missileWeaponInfoPrefab;

	public UnityEngine.GameObject missileWeaponList;

	private static Stack<MissileWeaponAreaWeaponStatus> statusPool = new Stack<MissileWeaponAreaWeaponStatus>();

	private static Stack<UnityEngine.GameObject> missileWeaponInfoPrefabPool = new Stack<UnityEngine.GameObject>();

	private Dictionary<XRL.World.GameObject, MissileWeaponAreaWeaponStatus> weaponStatus = new Dictionary<XRL.World.GameObject, MissileWeaponAreaWeaponStatus>();

	private ReaderWriterLockSlim StatusLock = new ReaderWriterLockSlim();

	public bool needsLayoutRefresh;

	public void OnReloadClicked()
	{
		Keyboard.PushMouseEvent("Command:CmdReload");
	}

	public void OnFireClicked()
	{
		Keyboard.PushMouseEvent("Command:CmdFire");
	}

	public void Init()
	{
		XRLCore.RegisterAfterRenderCallback(AfterRender);
	}

	public void Update()
	{
		fireHotkeyText.Apply();
		reloadHotkeyText.Apply();
		if (!StatusLock.TryEnterWriteLock(0))
		{
			return;
		}
		try
		{
			List<XRL.World.GameObject> list = null;
			foreach (KeyValuePair<XRL.World.GameObject, MissileWeaponAreaWeaponStatus> item in weaponStatus)
			{
				if (!item.Value.updated)
				{
					if (list == null)
					{
						list = new List<XRL.World.GameObject>();
					}
					list.Add(item.Key);
					needsLayoutRefresh = true;
				}
			}
			list?.ForEach((XRL.World.GameObject k) =>
			{
				weaponStatus[k].pool();
				weaponStatus.Remove(k);
			});
			foreach (KeyValuePair<XRL.World.GameObject, MissileWeaponAreaWeaponStatus> item2 in weaponStatus)
			{
				item2.Value.update();
			}
		}
		finally
		{
			StatusLock.ExitWriteLock();
		}
		if (needsLayoutRefresh)
		{
			HorizontalLayoutGroup component = GetComponent<HorizontalLayoutGroup>();
			component.enabled = false;
			component.enabled = true;
			needsLayoutRefresh = false;
		}
	}

	private void AfterRender(XRLCore core, ScreenBuffer sb)
	{
		if (!StatusLock.TryEnterWriteLock(100))
		{
			return;
		}
		try
		{
			List<XRL.World.GameObject> missileWeapons = The.Player.GetMissileWeapons();
			foreach (KeyValuePair<XRL.World.GameObject, MissileWeaponAreaWeaponStatus> item in weaponStatus)
			{
				item.Value.updated = false;
			}
			if (missileWeapons != null && missileWeapons.Count > 0)
			{
				for (int i = 0; i < missileWeapons.Count; i++)
				{
					try
					{
						if (missileWeapons[i] == null)
						{
							continue;
						}
						MissileWeapon part = missileWeapons[i].GetPart<MissileWeapon>();
						if (part != null)
						{
							if (!weaponStatus.ContainsKey(missileWeapons[i]))
							{
								weaponStatus.Add(missileWeapons[i], MissileWeaponAreaWeaponStatus.next());
								needsLayoutRefresh = true;
							}
							weaponStatus[missileWeapons[i]].area = this;
							part.Status(weaponStatus[missileWeapons[i]]);
							weaponStatus[missileWeapons[i]].updated = true;
						}
					}
					catch (Exception x)
					{
						MetricsManager.LogException("MissileWeaponArea::AfterRender", x);
					}
				}
				fireHotkeyText.text = "{{W|[" + ControlManager.getCommandInputDescription("CmdFire") + "]}} fire";
				reloadHotkeyText.text = "{{W|[" + ControlManager.getCommandInputDescription("CmdReload") + "]}} reload";
			}
			else
			{
				fireHotkeyText.text = "{{K|You have no missile weapons equipped.}}";
				reloadHotkeyText.text = "";
			}
		}
		finally
		{
			StatusLock.ExitWriteLock();
		}
	}
}
