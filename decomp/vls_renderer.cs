using Kobold;
using UnityEngine;

[ExecuteInEditMode]
public class vls_renderer : BaseMissileWeaponVFX
{
	public LineRenderer[] beamLayers;

	public GameObject sourceVfx;

	public GameObject impactVfx;

	public GameObject missileVfx;

	public ex3DSprite2 rocketRenderer;

	private exTextureInfo _rocketTexture;

	public string rocketTexture = "Assets_Content_Textures_Combat3C_rocket.png";

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		missileVfx.transform.localPosition = new Vector3(0f, 0f, 0f);
		if (_rocketTexture == null)
		{
			_rocketTexture = SpriteManager.GetTextureInfo(rocketTexture);
		}
		rocketRenderer.textureInfo = null;
		rocketRenderer.shader = SpriteManager.GetShaderMode(0);
		rocketRenderer.textureInfo = _rocketTexture;
		rocketRenderer.backcolor = new Color(0f, 0f, 0f, 0f);
		rocketRenderer.color = new Color(0f, 1f, 1f, 1f);
		rocketRenderer.detailcolor = new Color(1f, 0f, 0f, 1f);
		base.configure(configuration, path, ref duration);
		if (configuration.paths[path].first != null && configuration.paths[path].last != null)
		{
			transform.position = GameManager.Instance.getTileCenter(configuration.paths[path].first.X, configuration.paths[path].first.Y) + new Vector3(0f, 4f);
			if (sourceVfx != null)
			{
				sourceVfx.transform.position = GameManager.Instance.getTileCenter(configuration.paths[path].first.X, configuration.paths[path].first.Y) + new Vector3(0f, 4f);
			}
			if (impactVfx != null)
			{
				impactVfx.transform.position = GameManager.Instance.getTileCenter(configuration.paths[path].last.X, configuration.paths[path].last.Y);
			}
			LineRenderer[] array = beamLayers;
			foreach (LineRenderer obj in array)
			{
				obj.transform.position = GameManager.Instance.getTileCenter(configuration.paths[path].last.X, configuration.paths[path].last.Y);
				obj.GetComponent<LineRenderer>().SetPosition(0, GameManager.Instance.getTileCenter(configuration.paths[path].first.X, configuration.paths[path].first.Y) + new Vector3(0f, 4f));
				obj.GetComponent<LineRenderer>().SetPosition(1, GameManager.Instance.getTileCenter(configuration.paths[path].last.X, configuration.paths[path].last.Y));
			}
			MissileWeaponVFXConfiguration.MissileVFXPathDefinition missileVFXPathDefinition = configuration.paths[path];
			if (missileVFXPathDefinition.projectileVFXConfiguration == null)
			{
				missileVFXPathDefinition.projectileVFXConfiguration = "duration::0.25;;beamColor0::#FF0000;;beamColor1::#FF8000;;beamColor2::#FFFFFF".CachedDictionaryExpansion();
			}
			if (configuration.paths[path].getConfigValue("beamColor0") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("beamColor0"), out var color) && beamLayers.Length >= 1)
			{
				beamLayers[0].startColor = color;
				beamLayers[0].endColor = color;
			}
			if (configuration.paths[path].getConfigValue("beamColor1") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("beamColor1"), out var color2) && beamLayers.Length >= 2)
			{
				beamLayers[1].startColor = color2;
				beamLayers[1].endColor = color2;
			}
			if (configuration.paths[path].getConfigValue("beamColor2") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("beamColor2"), out var color3) && beamLayers.Length >= 3)
			{
				beamLayers[2].startColor = color3;
				beamLayers[2].endColor = color3;
			}
		}
	}
}
