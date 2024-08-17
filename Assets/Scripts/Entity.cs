using System.Collections;
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
	public int Health;
	public int Absorption;
	public int Block;
	public int MaxHealth;
	public int StackingEvade;

	/// <summary> An <see cref="EvasionChance"/>% chance to dodge an attack. </summary>
	public int EvasionChance { get; set; }
	public virtual int ModifyDamage(int damage)
		=> damage;
	public virtual IEnumerator Damage(Entity source, int damage)
	{
		if (Health < 0)
			yield break;

		damage = source.ModifyDamage(damage);

		if (EvasionChance > 0)
		{
			if (Random.Range(0, 100) > EvasionChance)
				yield break; //TODO
		}

		EvasionChance += StackingEvade;

		int newDamage = damage;
		if (Block > 0)
		{
			int blockChange = math.min(Block, damage);
			newDamage -= blockChange;
			Block -= blockChange;
		}

		if (newDamage == 0)
			yield break; //TODO

		if (Absorption > 0)
		{
			int extraHealthChange = math.min(Absorption, damage);
			newDamage -= extraHealthChange;
			Absorption -= extraHealthChange;
		}

		Health -= newDamage;
		yield return Health <= 0 ? DeathEffect() : OnHitEffect();
	}

	public IEnumerator OnHitEffect()
	{
		if (SpriteRenderer != null)
			for (int i = 0; i < 10; i++)
			{
				yield return new WaitForSeconds(0.05f);
				SpriteRenderer.enabled = false;
				yield return new WaitForSeconds(0.05f);
				SpriteRenderer.enabled = true;
			}
		else
			yield return null;
	}

	public IEnumerator DeathEffect()
	{
		StartCoroutine(OnHitEffect());
		SpriteRenderer.color = Color.red;
		for (int i = 0; i < 10; i++)
		{
			transform.Rotate(Vector3.forward, 1f);
			yield return null;
		}
	}

	public virtual IEnumerator PreTurn()
	{
		StackingEvade = 0;
		Block = 0;
		EvasionChance = 0;
		yield return null;
	}

	public abstract IEnumerator Turn();

	public virtual IEnumerator PostTurn()
	{
		yield return null;
	}

	// Start is called before the first frame update
	private void Start()
		=> StartB();

	protected virtual void StartB()
	{
		Health = MaxHealth;
		HealthBar.Setup(MaxHealth);
	}

	public SpriteRenderer SpriteRenderer;
	public HealthBar HealthBar;

	public void Update() => HealthBar.UpdateBar(Health, Block, Absorption);
}