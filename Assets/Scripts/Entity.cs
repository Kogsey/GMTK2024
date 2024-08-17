using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public enum DamageResult
{
	Dodged,
	Blocked,
	Landed,
	Kill,
}

public abstract class Entity : MonoBehaviour
{
	public int Health { get; protected set; }
	public int ExtraHealth { get; protected set; }
	public int MaxHealth { get; protected set; }
	public int Block { get; protected set; }

	/// <summary> An <see cref="EvasionChance"/>% chance to dodge an attack. </summary>
	public int EvasionChance { get; protected set; }

	public virtual DamageResult Damage(int damage)
	{
		if (EvasionChance > 0)
		{
			if (Random.Range(0, 100) > EvasionChance)
				return DamageResult.Dodged;
		}

		int newDamage = damage;
		if (Block > 0)
		{
			int blockChange = math.min(Block, damage);
			newDamage -= blockChange;
			Block -= blockChange;
		}

		if (newDamage == 0)
			return DamageResult.Blocked;

		if (ExtraHealth > 0)
		{
			int extraHealthChange = math.min(ExtraHealth, damage);
			newDamage -= extraHealthChange;
			ExtraHealth -= extraHealthChange;
		}

		Health -= newDamage;
		return Health <= 0 ? DamageResult.Kill : DamageResult.Landed;
	}

	public virtual void RoundStart() => Block = 0;

	public virtual void RoundEnd()
	{
	}

	// Start is called before the first frame update
	private void Start()
		=> Health = MaxHealth;
}