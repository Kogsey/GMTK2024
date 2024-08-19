using System.Collections;

public interface IEntityEffect<in T> where T : Entity
{
	public IconIDs IconID { get; }

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
	public DamageModMultiplier(float multiplier, IconIDs icon)
	{
		Multiplier = multiplier;
		IconID = icon;
	}

	public float Multiplier { get; }
	public IconIDs IconID { get; }
	public int TurnsLeft { get; set; }

	public float ModifyDamage(float damage)
		=> (int)(damage * Multiplier);
}

/// <summary> Animations should be additive and not interfere with other animations </summary>
/// <typeparam name="T"> </typeparam>
public interface IEntityAnimation<in T> where T : Entity
{
	public bool Complete { get; }

	public void UpdateAnimation(T entity);
}

public static class Extensions
{
	public static IEnumerator AwaitComplete<T, U>(this T animation) where T : IEntityAnimation<U> where U : Entity
	{
		while (!animation.Complete)
			yield return null;
	}
}