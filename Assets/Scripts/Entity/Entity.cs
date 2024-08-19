using System;
using System.Collections;
using System.Collections.Generic;
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

public class GenericEntityAnimations : IEntityAnimation<Entity>
{
	public bool Complete => false;

	public GenericEntityAnimations(AnimationEffect effect)
	{
		animationEffects = effect;
		myTimer = Random.value * 100;
	}

	private readonly AnimationEffect animationEffects;
	private Vector3 localPos = Vector3.zero;
	private Quaternion localRot = Quaternion.identity;
	private Vector3 localScale = Vector3.one;
	private float myTimer;

	public void UpdateAnimation(Entity entity)
	{
		myTimer += Time.smoothDeltaTime;
		localScale = Vector3.one;
		if (EnumUtility.HasFlag(animationEffects, AnimationEffect.Bob))
			localPos = new Vector3(0, Mathf.Sin(myTimer)) * 0.25f;
		if (EnumUtility.HasFlag(animationEffects, AnimationEffect.SquishX))
			localScale = Helpers.MemberWiseMultiply(localScale, new Vector3(1 - Mathf.Cos(myTimer) * 0.1f, 1, 1));
		if (EnumUtility.HasFlag(animationEffects, AnimationEffect.SquishY))
			localScale = Helpers.MemberWiseMultiply(localScale, new Vector3(1, 1 - Mathf.Sin(myTimer) * 0.1f, 1));
		if (EnumUtility.HasFlag(animationEffects, AnimationEffect.Wobble))
			localRot.eulerAngles = new Vector3(0, 0, Mathf.Sin(myTimer * 2) * 10f);

		entity.SpriteRenderer.transform.localPosition += localPos;
		entity.SpriteRenderer.transform.localScale = Helpers.MemberWiseMultiply(entity.SpriteRenderer.transform.localScale, localScale);
		entity.SpriteRenderer.transform.localRotation *= localRot;
	}
}

public abstract class Entity : MonoBehaviour
{
	public SpriteRenderer SpriteRenderer;
	public HealthBar HealthBar;
	public RectTransform EffectsHolder;
	public int Health;
	public int Absorption;
	public int Block;
	public int MaxHealth;
	public int StackingEvade;

	private bool _highlight;

	public bool Highlight
	{
		get => _highlight; set
		{
			_highlight = value;
			SpriteRenderer.color = TargetColour;
		}
	}

	private int _evasionChance;

	/// <summary> An <see cref="EvasionChance"/>% chance to dodge an attack. </summary>
	public int EvasionChance
	{
		get => _evasionChance; set
		{
			_evasionChance = value;
			SpriteRenderer.color = TargetColour;
		}
	}

	public abstract Color HighlightColour { get; }
	public Color TargetColour => (Highlight ? HighlightColour : Color.white).WithAlpha(1f - _evasionChance / 100f);
	public List<IEntityEffect<Entity>> Effects { get; } = new List<IEntityEffect<Entity>>();
	public bool PauseAnimation { get; set; } = false;

	protected virtual void Start()
	{
		Health = MaxHealth;
		HealthBar.Setup(MaxHealth);
	}

	public virtual float ModifyDamage(float damage)
	{
		foreach (IEntityEffect<Entity> effect in Effects)
			damage = effect.ModifyDamage(damage);
		return damage;
	}

	public virtual IEnumerator Damage(Entity source, int damage)
	{
		if (Health < 0)
			yield break;

		damage = (int)source.ModifyDamage(damage);

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
		StartCoroutine(Health <= 0 ? DeathEffect() : OnHitEffect());
		yield return null;
	}

	#region Effects

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

	public virtual IEnumerator DeathEffect()
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
		for (int i = 0; i < 10; i++)
		{
			yield return new WaitForSeconds(0.05f);
			SpriteRenderer.color = Color.gray;
			yield return new WaitForSeconds(0.05f);
			SpriteRenderer.color = TargetColour;
		}
	}

	private readonly float attackAnimationDistance = 7.5f;
	private readonly int attackAnimationFrames = 20;

	public virtual IEnumerator AttackAnimation(int direction = -1)
	{
		PauseAnimation = true;
		for (int i = 0; i < attackAnimationFrames; i++)
		{
			SpriteRenderer.transform.position += new Vector3(direction * (attackAnimationDistance / attackAnimationFrames), 0);
			yield return null;
		}
		for (int i = 0; i < attackAnimationFrames; i++)
		{
			SpriteRenderer.transform.position -= new Vector3(direction * (attackAnimationDistance / attackAnimationFrames), 0);
			yield return null;
		}
		PauseAnimation = false;
	}

	public IEnumerator DodgeEffect()
	{
		SpriteRenderer.color = new Color(0, 0, 0, 0);
		yield return new WaitForSeconds(0.5f);
		SpriteRenderer.color = TargetColour;
	}

	#endregion Effects

	public virtual IEnumerator ResetStats()
	{
		StackingEvade = 0;
		Block = 0;
		EvasionChance = 0;
		yield return null;
	}

	public abstract IEnumerator Turn();

	public virtual IEnumerator PostTurn()
	{
		for (int index = 0; index < Effects.Count; index++)
		{
			yield return Effects[index].OnTurnEnd(this);
			if (Effects[index].TurnsLeft <= 0)
			{
				Effects.RemoveAt(index);
				index--;
			}
		}
	}

	protected virtual void Update()
	{
		HealthBar.UpdateBar(Health, Block, Absorption);

		if (!PauseAnimation)
		{
			SpriteRenderer.transform.localScale = Vector3.one * 4;
			SpriteRenderer.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			UpdateAnimations();
		}
	}

	protected abstract void UpdateAnimations();

	public abstract void AddAnimation(IEntityAnimation<Entity> animation);

	public virtual IEnumerator InflictEffect(IEntityEffect<Entity> effect)
	{
		Effects.Add(effect);
		yield return effect.OnInflict(this);
	}
}