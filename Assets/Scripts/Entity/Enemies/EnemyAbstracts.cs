using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Flags]
public enum EnemyGeneration
{
	None = 0b_0000,
	Robot = 0b_0001,
	Plants = 0b_0010,
	Space = 0b_0100,
	Rare = 0b_1000,
}

public abstract class Enemy : Entity
{
	public override Color HighlightColour => Color.red * Color.white;
	public SpriteRenderer MoveSprite;
	public abstract IconID DisplayAction { get; }
	public abstract int DisplayActionCount { get; }

	public override IEnumerator DeathEffect()
	{
		yield return base.DeathEffect();
		Singleton<LevelManager>.instance.UpdateKill(this);
	}

	public override IEnumerator PreTurnEffects()
	{
		foreach (IEntityEffect<Enemy> effect in Effects)
			yield return effect.OnTurnStart(this);
	}

	public override IEnumerator PostTurn()
	{
		yield return base.PostTurn();
		PickNextAction();
		SetMoveSprite();
	}

	protected void SetMoveSprite()
		=> MoveSprite.sprite = SpriteBank.Instance.GetSprite(DisplayAction);

	public abstract void PickNextAction();

	private readonly List<IEntityAnimation<Enemy>> animations = new();

	protected override void UpdateAnimations()
	{
		foreach (IEntityAnimation<Enemy> animation in animations)
			animation.UpdateAnimation(this);
	}

	public void AddAnimation(IEntityAnimation<Enemy> animation)
		=> animations.Add(animation);

	public override void AddAnimation(IEntityAnimation<Entity> animation)
		=> AddAnimation(animation);

	private List<IEntityEffect<Enemy>> Effects { get; } = new List<IEntityEffect<Enemy>>();

	public override float ModifyDamage(float damage)
	{
		foreach (IEntityEffect<Enemy> effect in Effects)
			damage = effect.ModifyDamage(damage);
		return damage;
	}

	public override IEnumerator InflictEffect(IEntityEffect<Entity> effect)
	{
		Effects.Add(effect);
		effect.SetupIcon(EffectsHolder.transform);
		yield return effect.OnInflict(this);
	}

	protected override IEnumerator PostTurnEffects()
	{
		for (int index = 0; index < Effects.Count; index++)
		{
			yield return Effects[index].OnTurnEnd(this);
			if (Effects[index].TurnsLeft == 0)
			{
				Effects[index].RemoveIcon();
				Effects.RemoveAt(index);
				index--;
			}
		}
	}

	public override void ClearEffects()
	{
		foreach (IEntityEffect<Enemy> effect in Effects)
			effect.RemoveIcon();
		Effects.Clear();
	}
}