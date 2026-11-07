using System.Collections.Generic;

public class CombatJuiceEntryMissileWeaponVFX : CombatJuiceEntry
{
	private MissileWeaponVFXConfiguration config;

	private List<BaseMissileWeaponVFX> obj = new List<BaseMissileWeaponVFX>();

	public void configure(MissileWeaponVFXConfiguration config)
	{
		this.config = config;
		duration = 1.5f;
		t = 0f;
		if (config.paths.TryGetValue(0, out var value) && value.projectileVFXConfiguration.TryGetValue("Duration", out var value2) && int.TryParse(value2, out var result))
		{
			config.duration = result;
		}
	}

	public override bool canFinishUpToTurn()
	{
		return false;
	}

	public override void start()
	{
		duration = config.duration;
		t = 0f;
		foreach (KeyValuePair<int, MissileWeaponVFXConfiguration.MissileVFXPathDefinition> path in config.paths)
		{
			if (path.Value.used)
			{
				BaseMissileWeaponVFX component = PooledPrefabManager.Instantiate(path.Value.projectileVFX).GetComponent<BaseMissileWeaponVFX>();
				component.configure(config, path.Key, ref duration);
				obj.Add(component);
			}
		}
		base.start();
		foreach (BaseMissileWeaponVFX item in obj)
		{
			item.start();
		}
	}

	public override void update()
	{
		base.update();
		foreach (BaseMissileWeaponVFX item in obj)
		{
			item.OnUpdate(t, duration);
		}
	}

	public override void finish()
	{
		if (finished)
		{
			return;
		}
		finished = true;
		foreach (BaseMissileWeaponVFX item in obj)
		{
			item.pool();
		}
		obj.Clear();
		if (config != null)
		{
			MissileWeaponVFXConfiguration.repool(config);
		}
		base.finish();
	}
}
