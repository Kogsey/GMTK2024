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
	public abstract IconIDs DisplayAction { get; }
	public abstract int DisplayActionCount { get; }

	public override IEnumerator DeathEffect()
	{
		yield return base.DeathEffect();
		Singleton<LevelManager>.instance.UpdateKill(this);
	}

	public override IEnumerator PostTurn()
	{
		yield return base.PostTurn();
		yield return PickNextAction();
		SetMoveSprite();
	}

	protected void SetMoveSprite()
		=> MoveSprite.sprite = SpriteBank.Instance.GetSprite(DisplayAction);

	public abstract IEnumerator PickNextAction();

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
}