using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[Flags]
public enum AnimationEffect
{
	None = 0b_0000,
	Bob = 0b_0001,
	SquishX = 0b_0010,
	SquishY = 0b_0100,
	Wobble = 0b_1000,
}

public abstract class Entity : MonoBehaviour
{
	public int Health;
	public int Absorption;
	public int Block;
	public int MaxHealth;
	public int StackingEvade;
	public AnimationEffect AnimationEffects;

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
			{
				StartCoroutine(DodgeEffect());
				yield break;
			}
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
		{
			StartCoroutine(BlockEffect());
			yield break;
		}

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
		for (int i = 0; i < 10; i++)
		{
			yield return new WaitForSeconds(0.05f);
			SpriteRenderer.enabled = false;
			yield return new WaitForSeconds(0.05f);
			SpriteRenderer.enabled = true;
		}
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

	public virtual IEnumerator BlockEffect()
	{
		Color startColour = SpriteRenderer.color;
		for (int i = 0; i < 10; i++)
		{
			yield return new WaitForSeconds(0.05f);
			SpriteRenderer.color = Color.gray;
			yield return new WaitForSeconds(0.05f);
			SpriteRenderer.color = startColour;
		}
	}
	public IEnumerator DodgeEffect()
	{
		yield return null; // TODO
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

	protected virtual void Start()
	{
		Health = MaxHealth;
		HealthBar.Setup(MaxHealth);
		myTimer = Random.value * 100;
	}

	public SpriteRenderer SpriteRenderer;
	public HealthBar HealthBar;
	public RectTransform EffectsHolder;
	private float myTimer;
	protected virtual void Update()
	{
		HealthBar.UpdateBar(Health, Block, Absorption);
		myTimer += Time.smoothDeltaTime;
		if (EnumUtility.HasFlag(AnimationEffects, AnimationEffect.Bob))
			SpriteRenderer.transform.localPosition = new Vector3(0, Mathf.Sin(myTimer)) * 0.25f;
		if (EnumUtility.HasFlag(AnimationEffects, AnimationEffect.SquishX))
			SpriteRenderer.transform.localScale = new Vector3(4 - Mathf.Cos(myTimer) * 0.4f, 4, 4);

		if (EnumUtility.HasFlag(AnimationEffects, AnimationEffect.SquishY))
			SpriteRenderer.transform.localScale = new Vector3(4, 4 - Mathf.Sin(myTimer) * 0.4f, 4);
		if (EnumUtility.HasFlag(AnimationEffects, AnimationEffect.Wobble))
		{
			Quaternion localRot = SpriteRenderer.transform.localRotation;
			localRot.eulerAngles = new Vector3(0, 0, Mathf.Sin(myTimer * 2) * 10f);
			SpriteRenderer.transform.localRotation = localRot;
		}
	}
}