using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using XRL.Core;
using XRL.UI;
using XRL.World;

public static class CombatJuice
{
	public interface ICombatJuiceConfigurable
	{
		void configureWithObject(object args)
		{
		}

		void configure(string configurationString)
		{
		}
	}

	public interface ICombatJuiceAnimator
	{
		void Play(bool loop = false, Action after = null, string name = null, string objectId = null);

		void Stop();
	}

	public static Dictionary<string, UnityEngine.GameObject> prefabAnimations = new Dictionary<string, UnityEngine.GameObject>();

	public static Dictionary<string, Queue<UnityEngine.GameObject>> prefabPool = new Dictionary<string, Queue<UnityEngine.GameObject>>();

	public static CC_Blend lowHPBlend = null;

	public static Dictionary<string, UnityEngine.GameObject> roots = new Dictionary<string, UnityEngine.GameObject>();

	public static long juiceTurn = 0L;

	public static GameManager gameManager => GameManager.Instance;

	public static bool soundsEnabled => Options.UseCombatSounds;

	public static bool enabled => Options.UseOverlayCombatEffects;

	public static CombatJuiceManager juiceManager => CombatJuiceManager.instance;

	public static void _setLowHPIndicator(float percent)
	{
		if (lowHPBlend == null)
		{
			lowHPBlend = GameManager.MainCamera.GetComponent<CC_Blend>();
		}
		if (!(lowHPBlend != null))
		{
			return;
		}
		if (percent <= 0f && lowHPBlend.enabled)
		{
			lowHPBlend.enabled = false;
		}
		else if (percent > 0f)
		{
			if (!lowHPBlend.enabled)
			{
				lowHPBlend.enabled = true;
			}
			lowHPBlend.amount = percent * 0.5f;
		}
	}

	public static UnityEngine.GameObject getInstance(string name, string root)
	{
		UnityEngine.GameObject gameObject = null;
		if (!roots.ContainsKey(root))
		{
			if (!roots.ContainsKey("_JuiceRoot"))
			{
				UnityEngine.GameObject gameObject2 = new UnityEngine.GameObject();
				gameObject2.name = "_JuiceRoot";
				gameObject2.transform.position = new Vector3(0f, 0f, 0f);
				roots.Add("_JuiceRoot", gameObject2);
			}
			UnityEngine.GameObject gameObject3 = new UnityEngine.GameObject();
			gameObject3.transform.parent = roots["_JuiceRoot"].transform;
			gameObject3.name = root;
			gameObject3.transform.position = new Vector3(0f, 0f, 0f);
			roots.Add(root, gameObject3);
		}
		if (!prefabPool.ContainsKey(name))
		{
			prefabPool.Add(name, new Queue<UnityEngine.GameObject>());
		}
		if (prefabPool.ContainsKey(name) && prefabPool[name].Count > 0)
		{
			gameObject = prefabPool[name].Dequeue();
		}
		if (!prefabAnimations.ContainsKey(name))
		{
			prefabAnimations.Add(name, Resources.Load("Prefabs/" + name) as UnityEngine.GameObject);
		}
		if (gameObject == null)
		{
			if (!prefabAnimations.ContainsKey(name))
			{
				MetricsManager.LogError("Can't find animation name " + name);
				return null;
			}
			gameObject = UnityEngine.Object.Instantiate(prefabAnimations[name]);
			gameObject.transform.SetParent(roots[root].transform, worldPositionStays: true);
		}
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public static void pool(string name, UnityEngine.GameObject prefab, bool disableFirst = true)
	{
		if (disableFirst)
		{
			prefab.SetActive(value: false);
		}
		prefabPool[name].Enqueue(prefab);
	}

	public static void _cameraShake(float duration)
	{
		CameraShake.shakeDuration += duration;
	}

	public static void cameraShake(float duration, bool Async = false)
	{
		CombatJuiceManager.enqueueEntry(new CombatJuiceEntryCameraShake(duration), Async);
	}

	public static void _text(Vector3 start, Vector3 end, string text, Color color, float floatTime, float scale = 1f)
	{
		UnityEngine.GameObject instance = getInstance("CombatJuice/CombatJuiceText", "CombatJuiceText");
		instance.transform.position = start;
		instance.transform.localScale = new Vector3(scale, scale, 1f);
		instance.GetComponent<SimpleTextMeshTweener>().init(start, end, color, new Color(color.r, color.g, color.b, 0f), floatTime, "CombatJuice/CombatJuiceText");
		TextMesh component = instance.GetComponent<TextMesh>();
		component.color = color;
		component.text = text;
	}

	public static void _playPrefabAnimation(Vector3 location, string name, string objectId = null, string configurationString = null, object configurationObject = null)
	{
		UnityEngine.GameObject animation = getInstance(name, "PrefabAnimation_" + name);
		animation.transform.position = location;
		ICombatJuiceConfigurable component = animation.GetComponent<ICombatJuiceConfigurable>();
		if (component != null)
		{
			component.configureWithObject(configurationObject);
			component.configure(configurationString);
		}
		animation.GetComponent<ICombatJuiceAnimator>().Play(loop: false, () =>
		{
			pool(name, animation);
		}, name, objectId);
	}

	public static void _playPrefabAnimation(int x, int y, string name)
	{
		playPrefabAnimation(GameManager.Instance.getTileCenter(x, y, 100), name);
	}

	public static void StopPrefabAnimation(string Name)
	{
		if (GameManager.IsOnUIContext())
		{
			if (!roots.TryGetValue("PrefabAnimation_" + Name, out var value))
			{
				return;
			}
			Transform transform = value.transform;
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				UnityEngine.GameObject gameObject = transform.GetChild(i).gameObject;
				if (gameObject.activeSelf)
				{
					gameObject.GetComponent<ICombatJuiceAnimator>()?.Stop();
				}
			}
		}
		else
		{
			GameManager.Instance.uiQueue.queueTask(() =>
			{
				StopPrefabAnimation(Name);
			});
		}
	}

	public static void playWorldSound(XRL.World.GameObject obj, string clip, float volume = 0.5f, float pitchVariance = 0f, float t = 0f, float delay = 0f)
	{
		if (soundsEnabled && obj != null && obj.IsValid() && obj.IsVisible())
		{
			CombatJuiceEntryWorldSound juiceWorldSound = obj.Physics.GetJuiceWorldSound(clip, volume, pitchVariance, delay);
			if (juiceWorldSound != null)
			{
				juiceWorldSound.t = t;
				CombatJuiceManager.enqueueEntry(juiceWorldSound);
			}
		}
	}

	public static void _playPrefabAnimation(XRL.World.GameObject gameObject, string animation)
	{
		try
		{
			if (gameObject != null && gameObject.IsVisible() && gameObject.IsValid())
			{
				playPrefabAnimation(GameManager.Instance.getTileCenter(gameObject.Physics.CurrentCell.Location.X, gameObject.Physics.CurrentCell.Location.Y, 100), animation);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("CombatJuice::playPrefabAnimation", x);
		}
	}

	public static void floatingText(Cell C, string text, Color color, float duration = 1.5f, float floatLength = 24f, float scale = 1f, bool ignoreVisibility = true, XRL.World.GameObject gameObject = null)
	{
		if (Options.UseCombatText && C != null && (ignoreVisibility || C.IsVisible()))
		{
			Vector3 vector = gameManager.getTileCenter(C.X, C.Y, 100) + new Vector3(0f, 12f, 0f);
			Vector3 end = vector + new Vector3(0f, floatLength, 0f);
			CombatJuiceManager.enqueueEntry(new CombatJuiceEntryText(vector, end, duration, text, color, gameObject, scale));
		}
	}

	public static void floatingText(XRL.World.GameObject gameObject, string text, Color color, float duration = 1.5f, float floatLength = 24f, float scale = 1f, bool ignoreVisibility = false)
	{
		if (Options.UseCombatText && XRL.World.GameObject.Validate(ref gameObject) && (ignoreVisibility || gameObject.IsVisible()))
		{
			floatingText(gameObject.CurrentCell, text, color, duration, floatLength, scale, ignoreVisibility: true, gameObject);
		}
	}

	public static CombatJuiceEntryMissileWeaponVFX missileWeaponVFX(MissileWeaponVFXConfiguration configuration, bool Async = false)
	{
		CombatJuiceEntryMissileWeaponVFX combatJuiceEntryMissileWeaponVFX = new CombatJuiceEntryMissileWeaponVFX();
		combatJuiceEntryMissileWeaponVFX.configure(configuration);
		CombatJuiceManager.enqueueEntry(combatJuiceEntryMissileWeaponVFX, Async);
		return combatJuiceEntryMissileWeaponVFX;
	}

	public static CombatJuiceEntryPunch punch(XRL.World.GameObject Attacker, XRL.World.GameObject Defender, float Time = 0.2f, Easing.Functions Ease = Easing.Functions.SineEaseInOut, float FromXOffset = 0f, float FromYOffset = 0f, float ToXOffset = 0f, float ToYOffset = 0f, float DistanceFactor = 1f, string SoundTag = "PunchSound", string DefaultSound = null, bool AllowPlayerSound = false, bool Async = false)
	{
		try
		{
			if (XRL.World.GameObject.Validate(ref Attacker) && XRL.World.GameObject.Validate(ref Defender) && Attacker.IsInActiveZone() && Defender.IsInActiveZone())
			{
				if ((AllowPlayerSound || !Attacker.IsPlayer()) && (!SoundTag.IsNullOrEmpty() || !DefaultSound.IsNullOrEmpty()))
				{
					Attacker.PlayWorldSound(Attacker.GetSoundTag(SoundTag).Coalesce(DefaultSound));
				}
				if (Attacker.IsVisible())
				{
					Zone currentZone = Attacker.CurrentZone;
					Zone currentZone2 = Defender.CurrentZone;
					Location2D location = Attacker.CurrentCell.Location;
					Location2D location2 = Defender.CurrentCell.Location;
					if (DistanceFactor != 1f)
					{
						ToXOffset += (float)(location.X - location2.X) * DistanceFactor;
						ToYOffset += (float)(location.Y - location2.Y) * DistanceFactor;
					}
					float toYOffset;
					float toXOffset;
					float fromYOffset;
					float fromXOffset;
					if (currentZone == currentZone2)
					{
						fromXOffset = FromXOffset;
						fromYOffset = FromYOffset;
						toXOffset = ToXOffset;
						toYOffset = ToYOffset;
						return punch(location, location2, Time, Ease, fromXOffset, fromYOffset, toXOffset, toYOffset);
					}
					int x = location.X;
					int y = location.Y;
					int num;
					int num2;
					if (currentZone.Z < currentZone2.Z)
					{
						num = Math.Min(Math.Max(x + 1, 0), currentZone.Width);
						num2 = Math.Min(Math.Max(y + 1, 0), currentZone.Height);
						if (num != x)
						{
							ToXOffset -= 0.5f;
						}
						if (num2 != y)
						{
							ToYOffset -= 0.5f;
						}
					}
					else if (currentZone.Z > currentZone2.Z)
					{
						num = Math.Min(Math.Max(x - 1, 0), currentZone.Width);
						num2 = Math.Min(Math.Max(y - 1, 0), currentZone.Height);
						if (num != x)
						{
							ToXOffset += 0.5f;
						}
						if (num2 != y)
						{
							ToYOffset += 0.5f;
						}
					}
					else
					{
						num = location2.X;
						num2 = location2.Y;
						if (num > x + 1 || num < x - 1)
						{
							num = x;
							if (num2 == y)
							{
								ToYOffset += ((num2 > 0) ? 0.5f : (-0.5f));
								num2 += ((num2 <= 0) ? 1 : (-1));
							}
						}
						if (num2 > y + 1 || num2 < y - 1)
						{
							num2 = y;
							if (num == x)
							{
								ToXOffset += ((num > 0) ? 0.5f : (-0.5f));
								num += ((num <= 0) ? 1 : (-1));
							}
						}
					}
					int toX = num;
					int toY = num2;
					toYOffset = FromXOffset;
					toXOffset = FromYOffset;
					fromYOffset = ToXOffset;
					fromXOffset = ToYOffset;
					return punch(x, y, toX, toY, Time, Ease, toYOffset, toXOffset, fromYOffset, fromXOffset, Async);
				}
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("CombatJuice::punch", x2);
		}
		return null;
	}

	public static CombatJuiceEntryPunch punch(Location2D AttackerCellLocation, Location2D DefenderCellLocation, float Time = 0.2f, Easing.Functions Ease = Easing.Functions.SineEaseInOut, float FromXOffset = 0f, float FromYOffset = 0f, float ToXOffset = 0f, float ToYOffset = 0f)
	{
		if (AttackerCellLocation == null || DefenderCellLocation == null || AttackerCellLocation == DefenderCellLocation)
		{
			return null;
		}
		return punch(AttackerCellLocation.X, AttackerCellLocation.Y, DefenderCellLocation.X, DefenderCellLocation.Y, Time, Ease, FromXOffset, FromYOffset, ToXOffset, ToYOffset);
	}

	public static CombatJuiceEntryPunch punch(int FromX, int FromY, int ToX, int ToY, float Time = 0.2f, Easing.Functions Ease = Easing.Functions.SineEaseInOut, float FromXOffset = 0f, float FromYOffset = 0f, float ToXOffset = 0f, float ToYOffset = 0f, bool Async = false)
	{
		ex3DSprite2 tile = gameManager.getTile(FromX, FromY);
		Vector3 start = gameManager.getTileCenter(FromX, FromY) + new Vector3(FromXOffset, FromYOffset, 0f);
		Vector3 end = gameManager.getTileCenter(ToX, ToY) + new Vector3(ToXOffset, ToYOffset, 0f);
		CombatJuiceEntryPunch combatJuiceEntryPunch = new CombatJuiceEntryPunch(tile, start, end, Time, Ease);
		CombatJuiceManager.enqueueEntry(combatJuiceEntryPunch, Async);
		return combatJuiceEntryPunch;
	}

	public static void Hover(Location2D Location, float Duration = 10f, float Rise = 1f)
	{
		ex3DSprite2 tile = gameManager.getTile(Location.X, Location.Y);
		Vector3 tileCenter = gameManager.getTileCenter(Location.X, Location.Y);
		CombatJuiceManager.enqueueEntry(new CombatJuiceEntryHover(tile, tileCenter, 12.5f, Duration, Rise), async: true);
	}

	public static CombatJuiceEntryJump Jump(XRL.World.GameObject Actor, Location2D Location, Location2D Target, float Duration = 10f, float Arc = 0.5f, float Scale = 1f, bool Focus = false, bool Enqueue = true)
	{
		Vector3 tileCenter = gameManager.getTileCenter(Location.X, Location.Y);
		Vector3 tileCenter2 = gameManager.getTileCenter(Target.X, Target.Y);
		CombatJuiceEntryJump combatJuiceEntryJump = new CombatJuiceEntryJump(Actor.RenderForUI(), tileCenter, tileCenter2, Duration, Arc, Scale, Focus);
		if (Enqueue)
		{
			CombatJuiceManager.enqueueEntry(combatJuiceEntryJump, async: true);
		}
		return combatJuiceEntryJump;
	}

	public static CombatJuiceEntryMissileWeaponVFX Throw(XRL.World.GameObject Actor, XRL.World.GameObject Weapon, Location2D Location, Location2D Target, bool Async = false)
	{
		MissileWeaponVFXConfiguration missileWeaponVFXConfiguration = MissileWeaponVFXConfiguration.next();
		MissileWeaponVFXConfiguration.MissileVFXPathDefinition path = missileWeaponVFXConfiguration.GetPath(0);
		path.addStep(Location);
		path.addStep(Target);
		path.SetParameter("Tile", "Inherit");
		path.SetParameter("Foreground", "Inherit");
		path.SetParameter("Detail", "Inherit");
		path.SetParameter("Arc", "16");
		path.SetParameter("Speed", "350");
		path.SetParameter("Orientation", "0,360");
		path.SetParameter("Rotation", "240,480");
		path.projectileVFX = Weapon.GetPropertyOrTag("ThrowVFX", "MissileWeaponsEffects/3cprojectile");
		Dictionary<string, Dictionary<string, string>> xTags = Weapon.GetBlueprint().xTags;
		if (xTags != null && xTags.TryGetValue("ThrowVFX", out var value))
		{
			path.SetProjectileVFX(value);
		}
		path.SetProjectileRender(Weapon);
		ConfigureMissileVisualEffectEvent.Send(missileWeaponVFXConfiguration, path, Actor, null, Weapon);
		return missileWeaponVFX(missileWeaponVFXConfiguration, Async);
	}

	public static void BlockUntilAllFinished(int MaxMilliseconds = 10000, bool Interruptible = false)
	{
		XRLCore core = XRLCore.Core;
		Stopwatch frameTimer = XRLCore.FrameTimer;
		long num = frameTimer.ElapsedMilliseconds + MaxMilliseconds;
		while (CombatJuiceManager.AnyActive() && frameTimer.ElapsedMilliseconds < num)
		{
			if (Interruptible && Keyboard.kbhit())
			{
				finishAll();
				break;
			}
			core.RenderBase(UpdateSidebar: false);
		}
	}

	public static void BlockUntilFinished(CombatJuiceEntry Entry, IList<XRL.World.GameObject> Hide = null, int MaxMilliseconds = 10000, bool Interruptible = true)
	{
		if (Entry == null)
		{
			return;
		}
		if (!Hide.IsNullOrEmpty())
		{
			for (int num = Hide.Count - 1; num >= 0; num--)
			{
				XRL.World.GameObject gameObject = Hide[num];
				if (gameObject.Render == null || !gameObject.Render.Visible)
				{
					Hide[num] = null;
				}
				else
				{
					gameObject.Render.Visible = false;
				}
			}
		}
		XRLCore core = XRLCore.Core;
		Stopwatch frameTimer = XRLCore.FrameTimer;
		long num2 = frameTimer.ElapsedMilliseconds + MaxMilliseconds;
		while (!Entry.finished && frameTimer.ElapsedMilliseconds < num2)
		{
			if (Interruptible && Keyboard.kbhit())
			{
				finishAll();
				break;
			}
			XRLCore.RenderHiddenPlayer = Hide == null;
			core.RenderBase(UpdateSidebar: false);
		}
		if (Hide.IsNullOrEmpty())
		{
			return;
		}
		for (int num3 = Hide.Count - 1; num3 >= 0; num3--)
		{
			if (Hide[num3] != null)
			{
				Hide[num3].Render.Visible = true;
			}
		}
	}

	public static void BlockUntilFinished(CombatJuiceEntry Entry, Action Render, int MaxMilliseconds = 10000, bool Interruptible = true)
	{
		Stopwatch frameTimer = XRLCore.FrameTimer;
		long num = frameTimer.ElapsedMilliseconds + MaxMilliseconds;
		while (!Entry.finished && frameTimer.ElapsedMilliseconds < num)
		{
			if (Interruptible && Keyboard.kbhit())
			{
				finishAll();
				break;
			}
			Render();
		}
	}

	public static void clearUpToTurn(long n)
	{
		CombatJuiceManager.clearUpToTurn(n);
	}

	public static void finishAll()
	{
		GameManager.Instance.uiQueue.queueTask(CombatJuiceManager.finishAll);
	}

	public static void startTurn()
	{
		juiceTurn++;
		GameManager.Instance.uiQueue.queueSingletonTask("combatJuiceClearTurn", () =>
		{
			try
			{
				clearUpToTurn(juiceTurn - 2);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("CombatJuice::startTurn", x);
			}
		});
	}

	public static CombatJuiceEntryPrefabAnimation playPrefabAnimation(Vector3 location, string animation, string objectID = null, string configurationString = null, object configurationObject = null, bool async = false)
	{
		CombatJuiceEntryPrefabAnimation combatJuiceEntryPrefabAnimation = new CombatJuiceEntryPrefabAnimation(location, animation, objectID, configurationString, configurationObject)
		{
			async = async
		};
		CombatJuiceManager.enqueueEntry(combatJuiceEntryPrefabAnimation, async);
		return combatJuiceEntryPrefabAnimation;
	}

	public static CombatJuiceEntryPrefabAnimation playPrefabAnimation(XRL.World.GameObject gameObject, string animation, string objectId = null, string configurationString = null, object configurationObject = null, bool async = false)
	{
		if (gameObject != null && gameObject.IsValid() && gameObject.IsVisible())
		{
			return playPrefabAnimation(gameObject.Physics.CurrentCell.Location, animation, objectId, configurationString, null, async);
		}
		return null;
	}

	public static CombatJuiceEntryPrefabAnimation playPrefabAnimation(Location2D cellLocation, string animation, string objectId = null, string configurationString = null, object configurationObject = null, bool async = false)
	{
		return playPrefabAnimation(gameManager.getTileCenter(cellLocation.X, cellLocation.Y, 100), animation, objectId, configurationString, configurationObject, async);
	}
}
