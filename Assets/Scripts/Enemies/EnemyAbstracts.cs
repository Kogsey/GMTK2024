using System;
using System.Collections;
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
	public SpriteRenderer MoveSprite;
	public abstract IconIDs DisplayAction { get; }
	public abstract int DisplayActionCount { get; }

	public override IEnumerator PostTurn()
	{
		yield return PickNextAction();
		SetMoveSprite();
	}

	protected virtual void SetMoveSprite()
		=> MoveSprite.sprite = Singleton<IconBank>.instance.GetSprite(DisplayAction);

	public abstract IEnumerator PickNextAction();
}