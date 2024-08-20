using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public interface IEntityEffect<in T> where T : Entity
{
	public IconID IconID { get; }
	public SpriteRenderer Icon { get; set; }
	public int TurnsLeft { get; set; }

	public IEnumerator OnTurnEnd(T entity)
	{
		TurnsLeft -= 1;
		yield return null;
	}

	public IEnumerator OnTurnStart(T entity)
	{
		yield return null;
	}

	public IEnumerator OnInflict(T entity) { yield return null; }

	public float ModifyDamage(float damage) => damage;
}

public class DamageModMultiplier : IEntityEffect<Entity>
{
	public DamageModMultiplier(float multiplier, IconID icon)
	{
		Multiplier = multiplier;
		IconID = icon;
	}

	public float Multiplier { get; }
	public IconID IconID { get; }
	public int TurnsLeft { get; set; } = 1;
	public SpriteRenderer Icon { get; set; }

	public float ModifyDamage(float damage)
		=> (int)(damage * Multiplier);
}

public class DoTEffect : IEntityEffect<Entity>
{
	public DoTEffect(int DoT, int turnsLeft, IconID iconID)
	{
		DamagePerTurn = DoT;
		IconID = iconID;
		TurnsLeft = turnsLeft;
	}

	public SpriteRenderer Icon { get; set; }
	public IconID IconID { get; }
	public int TurnsLeft { get; set; }
	public int DamagePerTurn { get; }
}

public class TurnModEffect : IEntityEffect<Player>
{
	public TurnModEffect(int turnCountChange) => TurnCountChange = turnCountChange;

	public int TurnCountChange { get; }
	public IconID IconID => IconID.ZZZZZZ;
	public SpriteRenderer Icon { get; set; }
	public int TurnsLeft { get; set; }

	public IEnumerator OnTurnStart(Player entity)
	{
		entity.Energy += TurnCountChange;
		yield return null;
	}
}

/// <summary> Animations should be additive and not interfere with other animations </summary>
/// <typeparam name="T"> </typeparam>
public interface IEntityAnimation<in T> where T : Entity
{
	public bool Complete { get; }

	public void UpdateAnimation(T entity);
}

public class StaticScale : IEntityAnimation<Entity>
{
	public StaticScale(Vector3 scale) => Scale = scale;

	public StaticScale(float scale) : this(new Vector3(scale, scale, scale))
	{ }

	public Vector3 Scale { get; }
	public bool Complete => false;

	public void UpdateAnimation(Entity entity)
		=> entity.SpriteRenderer.transform.localScale = Helpers.MemberWiseMultiply(entity.SpriteRenderer.transform.localScale, Scale);
}

public static class Extensions
{
	public static IEnumerator AwaitComplete<T, U>(this T animation) where T : IEntityAnimation<U> where U : Entity
	{
		while (!animation.Complete)
			yield return null;
	}

	public static void SetupIcon<T>(this IEntityEffect<T> effect, Transform parent) where T : Entity
	{
		SpriteRenderer renderer = Object.Instantiate(Singleton<LevelManager>.instance.IconPrefab, parent);
		effect.Icon = renderer;
		effect.Icon.sprite = SpriteBank.Instance.GetSprite(effect.IconID);
	}

	public static void RemoveIcon<T>(this IEntityEffect<T> effect) where T : Entity
		=> Object.Destroy(effect.Icon.gameObject);
}