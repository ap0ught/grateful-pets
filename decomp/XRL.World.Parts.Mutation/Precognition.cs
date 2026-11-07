using System;
using System.Collections;
using System.Collections.Generic;
using Qud.UI;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using XRL.Core;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Precognition : BaseMutation
{
	public class LoadCommand : IActionCommand, IComposite
	{
		private static LoadCommand Instance = new LoadCommand();

		public static void Issue()
		{
			XRLGame game = The.Game;
			if (game == null || !game.Running)
			{
				return;
			}
			ActionManager actionManager = game.ActionManager;
			if (!actionManager.HasAction(typeof(LoadCommand)))
			{
				if (actionManager.IsPlayerTurn())
				{
					actionManager.SkipPlayerTurn = true;
				}
				actionManager.SkipSegment = true;
				actionManager.DequeueActionsDescendedFrom<ISystemSaveCommand>();
				actionManager.EnqueueAction(Instance);
			}
		}

		public void Execute(XRLGame Game, ActionManager Manager)
		{
			Dictionary<string, object> gameState = GetPrecognitionRestoreGameStateEvent.GetFor(Game.Player.Body);
			SingletonWindowBase<LoadingStatusWindow>.instance.StayHidden = true;
			XRLGame.LoadCurrentGame("Precognition", ShowPopup: false, gameState);
			The.Core.RenderBase();
			Reverting = false;
			SingletonWindowBase<LoadingStatusWindow>.instance.StayHidden = false;
		}
	}

	public int TurnsLeft;

	public bool RealityDistortionBased = true;

	public Guid RevertActivatedAbilityID;

	public bool WasPlayer;

	public int HitpointsAtSave;

	public int TemperatureAtSave;

	[NonSerialized]
	private Guid GlimpseID;

	[NonSerialized]
	private long ActivatedSegment;

	[NonSerialized]
	private static Guid CurrentID;

	[NonSerialized]
	private static Texture2D Texture;

	[NonSerialized]
	private static bool TextureCaptured;

	[NonSerialized]
	public static bool Reverting;

	public Precognition()
	{
		Type = "Mental";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		int duration = GetDuration(Level);
		int num = duration;
		GameObject parentObject = activatedAbilityEntry.ParentObject;
		if (parentObject != null && parentObject.HasPart<Nonlinearity_Tomorrowful>())
		{
			stats.postfix += $"Duration increased by {duration} due to the Tomorrowful skill.";
			num = duration * 2;
		}
		stats.Set("Duration", num, !flag || duration != num, num - duration);
		stats.CollectCooldownTurns(activatedAbilityEntry, GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetDefensiveAbilityListEvent E)
	{
		if (TurnsLeft <= 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (E.Distance <= 1 || E.Actor.hitpoints < E.Actor.baseHitpoints || E.Actor.Con(null, IgnoreHideCon: true) >= 5) && CheckMyRealityDistortionAdvisability() && 70.in100())
		{
			E.Add("CommandPrecognition");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("time", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static void OnPostRenderCallback(Camera Camera)
	{
		try
		{
			if (Camera == GameManager.cameraMainCamera && !TextureCaptured)
			{
				Texture.ReadPixels(new Rect(0f, 0f, Texture.width, Texture.height), 0, 0, recalculateMipMaps: false);
				NativeArray<Color32> pixelData = Texture.GetPixelData<Color32>(0);
				int i = 0;
				for (int length = pixelData.Length; i < length; i++)
				{
					Color color = pixelData[i];
					float num = color.grayscale * color.grayscale;
					pixelData[i] = new Color(num, num, num);
				}
				Texture.Apply(updateMipmaps: false);
				TextureCaptured = true;
				Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Precognition:TextureCapture", x);
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
		}
	}

	public static void CaptureGlimpse(Guid GlimpseID)
	{
		if (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32))
		{
			return;
		}
		try
		{
			int width = Screen.width;
			int height = Screen.height;
			if ((bool)Texture)
			{
				if (Texture.width != width || Texture.height != height)
				{
					Texture.Reinitialize(width, height, TextureFormat.RGBA32, hasMipMap: false);
				}
			}
			else
			{
				Texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
			}
			CurrentID = GlimpseID;
			TextureCaptured = false;
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Precognition:TextureInit", x);
			CurrentID = Guid.Empty;
		}
	}

	public static IEnumerator GlimpseRoutine(Guid GlimpseID)
	{
		Color color = Color.white;
		Sprite sprite = null;
		if (GlimpseID == CurrentID && TextureCaptured)
		{
			try
			{
				sprite = Sprite.Create(Texture, new Rect(0f, 0f, Texture.width, Texture.height), new Vector2(0.5f, 0.5f));
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Precognition:CreateSprite", x);
				sprite = null;
			}
		}
		if (sprite == null)
		{
			sprite = UIManager.instance.DefaultSquare;
			color = Color.black;
		}
		UnityEngine.GameObject obj = new UnityEngine.GameObject("Glimpse");
		RectTransform rectTransform = obj.AddComponent<RectTransform>();
		Image image = obj.AddComponent<Image>();
		image.sprite = sprite;
		int threshold = Shader.PropertyToID("_Threshold");
		Material material = (image.material = Resources.Load<Material>("Materials/FX_ImageTransition"));
		Material material3 = material;
		rectTransform.SetParent(UIManager.instance.gameObject.transform);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.anchorMin = new Vector2(0f, 0f);
		rectTransform.offsetMax = default;
		rectTransform.offsetMin = default;
		rectTransform.anchoredPosition = default;
		material3.color = color;
		material3.SetFloat(threshold, 0f);
		obj.SetActive(value: true);
		float t = 0f;
		while ((Reverting || t <= 1f) && t <= 40f)
		{
			yield return null;
			t += Time.deltaTime * 2f;
			material3.SetFloat(threshold, t);
		}
		material3.color = color.WithAlpha(0f);
		material3.SetFloat(threshold, 0f);
		t = 0f;
		while (t <= 1f)
		{
			yield return null;
			t += Time.deltaTime * 2f;
			material3.SetFloat(threshold, t);
		}
		obj.SetActive(value: false);
		obj.Destroy();
		if (sprite != UIManager.instance.DefaultSquare)
		{
			sprite.Destroy();
		}
		ClearGlimpses();
		Reverting = false;
	}

	public static bool RemoveGlimpse(Guid GlimpseID)
	{
		return true;
	}

	public static void ClearGlimpses()
	{
		TextureCaptured = false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDie");
		Registrar.Register("EndTurn");
		Registrar.Register("CommandPrecognition");
		Registrar.Register("CommandPrecognitionRevert");
		Registrar.Register("GameRestored");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You peer into your near future.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("You may activate this power and then later revert to the point in time when you activated it.\n" + "Duration between use and reversion: {{rules|" + GetDuration(Level) + "}} rounds\n", "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public int GetCooldown()
	{
		return GetCooldown(Level);
	}

	public virtual int GetCooldown(int Level)
	{
		return 500;
	}

	public int GetDuration()
	{
		return GetDuration(Level);
	}

	public virtual int GetDuration(int Level)
	{
		return 4 * Level + 12;
	}

	public static Guid Save()
	{
		Guid glimpseID = Guid.NewGuid();
		SingletonWindowBase<LoadingStatusWindow>.instance.StayHidden = true;
		The.Core.SaveGame("Precognition");
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			CaptureGlimpse(glimpseID);
		});
		SingletonWindowBase<LoadingStatusWindow>.instance.StayHidden = false;
		return glimpseID;
	}

	public static void Load(Guid GlimpseID, GameObject obj = null)
	{
		Reverting = true;
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			GameManager.Instance.StartCoroutine(GlimpseRoutine(GlimpseID));
		});
		LoadCommand.Issue();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (TurnsLeft > 0)
			{
				TurnsLeft--;
				if (TurnsLeft <= 0)
				{
					MyActivatedAbility(RevertActivatedAbilityID).Enabled = false;
					if (WasPlayer && ParentObject.IsPlayer() && ActivatedSegment < The.Game.Segments)
					{
						if (Popup.ShowYesNo("Your precognition is about to run out. Would you like to return to the start of your vision?") == DialogResult.Yes)
						{
							ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_precognition_timeRevert");
							AutoAct.Interrupt();
							Load(GlimpseID, ParentObject);
							ActivatedSegment = The.Game.Segments + 100;
						}
						else if (GlimpseID != Guid.Empty)
						{
							RemoveGlimpse(GlimpseID);
						}
					}
				}
			}
		}
		else if (E.ID == "CommandPrecognition")
		{
			if (TurnsLeft > 0)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You are already within a precognitive vision.");
				}
				return false;
			}
			if (RealityDistortionBased && !ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return false;
			}
			SoundManager.PreloadClipSet("Sounds/Abilities/sfx_ability_mutation_precognition_timeRevert");
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_precognition_activate");
			CooldownMyActivatedAbility(ActivatedAbilityID, 500);
			if (ParentObject.IsPlayer())
			{
				GlimpseID = Save();
				WasPlayer = true;
				IComponent<GameObject>.AddPlayerMessage("You peer into the future.");
			}
			else
			{
				WasPlayer = false;
				if (SensePsychic.SensePsychicFromPlayer(ParentObject) != null)
				{
					IComponent<GameObject>.AddPlayerMessage("You sense a subtle psychic disturbance.");
				}
			}
			HitpointsAtSave = ParentObject.hitpoints;
			TemperatureAtSave = ParentObject.Physics.Temperature;
			MyActivatedAbility(RevertActivatedAbilityID).Enabled = true;
			TurnsLeft = GetDuration(Level) + 1;
			if (ParentObject.HasRegisteredEvent("InitiatePrecognition"))
			{
				Event obj = Event.New("InitiatePrecognition", "Duration", TurnsLeft);
				ParentObject.FireEvent(obj, E);
				TurnsLeft = obj.GetIntParameter("Duration");
			}
		}
		else if (E.ID == "CommandPrecognitionRevert")
		{
			if (TurnsLeft > 0)
			{
				if (WasPlayer && ParentObject.IsPlayer())
				{
					ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_precognition_timeRevert");
					IComponent<GameObject>.AddPlayerMessage("Your focus returns to the present.");
					Load(GlimpseID, ParentObject);
					MyActivatedAbility(RevertActivatedAbilityID).Enabled = false;
				}
				else if (ParentObject.IsPlayer())
				{
					Popup.Show("You cannot access someone else's precognitive vision.");
				}
			}
		}
		else if (E.ID == "BeforeDie")
		{
			return OnBeforeDie(ParentObject, RevertActivatedAbilityID, GlimpseID, ref TurnsLeft, ref HitpointsAtSave, ref TemperatureAtSave, ref ActivatedSegment, WasPlayer, RealityDistortionBased, this);
		}
		return base.FireEvent(E);
	}

	public static bool OnBeforeDie(GameObject Object, Guid RevertAAID, Guid GlimpseID, ref int TurnsLeft, ref int HitpointsAtSave, ref int TemperatureAtSave, ref long ActivatedSegment, bool WasPlayer, bool RealityDistortionBased, IPart Mutation)
	{
		if (ActivatedSegment >= The.Game.Segments)
		{
			Object.hitpoints = HitpointsAtSave;
			if (Object.Physics != null)
			{
				Object.Physics.Temperature = TemperatureAtSave;
			}
			return false;
		}
		if (TurnsLeft <= 0)
		{
			return true;
		}
		if (Object.IsPlayer())
		{
			AutoAct.Interrupt();
			if (WasPlayer)
			{
				Achievement.FORESEE_DEATH.Unlock();
				if (Popup.ShowYesNo("You sense your imminent demise. Would you like to return to the start of your vision?") == DialogResult.Yes)
				{
					Object?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_precognition_timeRevert");
					IComponent<GameObject>.AddPlayerMessage("Your focus returns to the present.");
					Load(GlimpseID, Object);
					ActivatedSegment = The.Game.Segments + 100;
					return false;
				}
			}
		}
		else if (!Object.IsOriginalPlayerBody() && (!RealityDistortionBased || IComponent<GameObject>.CheckRealityDistortionUsability(Object, null, Object, null, Mutation)))
		{
			TurnsLeft = 0;
			if (RevertAAID != Guid.Empty)
			{
				Object.DisableActivatedAbility(RevertAAID);
			}
			if (Object.HasStat("Hitpoints"))
			{
				ActivatedSegment = The.Game.Segments + 1;
				Object.hitpoints = HitpointsAtSave;
				if (Object.Physics != null)
				{
					Object.Physics.Temperature = TemperatureAtSave;
				}
				Object.DilationSplat();
				Object?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_precognition_timeRevert");
				Object.Physics.DidX("swim", "before your eyes", "!", null, null, Object);
				return false;
			}
		}
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Precognition - Start vision", "CommandPrecognition", "Mental Mutations", null, "!", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		RevertActivatedAbilityID = AddMyActivatedAbility("Precognition - End vision", "CommandPrecognitionRevert", "Mental Mutations", null, "?", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		MyActivatedAbility(RevertActivatedAbilityID).Enabled = false;
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		RemoveMyActivatedAbility(ref RevertActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
