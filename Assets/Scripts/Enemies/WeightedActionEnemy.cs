using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum WeightedEnemyActionType
{
	Attack,
	Block,
	Evade,
}

public class WeightedActionEnemy : Enemy
{
	public WeightedEnemyData WeightedEnemyData
	{
		set
		{
			MaxHealth = Random.Range(value.MinHealth, value.MaxHealth + 1);
			Health = MaxHealth;
			Block = value.StartBlock;
			Absorption = value.StartAbsorb;
			SpriteRenderer.sprite = value.Sprite;
			ActionPool = new List<GenericEnemyAction>(value.ActionPool);
			AnimationEffects = value.AnimationEffects;
		}
	}

	private List<GenericEnemyAction> ActionPool;
	private GenericEnemyAction NextAction;
	public override IconIDs DisplayAction => ActionToHint(NextAction.ActionDisplay);
	public override int DisplayActionCount => NextAction.Amount;

	public IconIDs ActionToHint(WeightedEnemyActionType weightedEnemyAction)
		=> weightedEnemyAction switch
		{
			WeightedEnemyActionType.Attack => IconIDs.Sword,
			WeightedEnemyActionType.Block => IconIDs.ShieldCircle,
			WeightedEnemyActionType.Evade => IconIDs.BlueLightning,
			_ => throw new NotImplementedException(),
		};

	public override IEnumerator Turn()
	{
		switch (NextAction.ActionDisplay)
		{
			case WeightedEnemyActionType.Attack:
				yield return Singleton<Player>.instance.Damage(this, NextAction.Amount);
				break;

			case WeightedEnemyActionType.Block:
				Block += NextAction.Amount;
				break;

			case WeightedEnemyActionType.Evade:
				EvasionChance += NextAction.Amount;
				break;

			default:
				throw new NotImplementedException();
		}
		yield return null;
	}

	public override IEnumerator PickNextAction()
	{
		float totalWeight = 0;
		foreach (GenericEnemyAction action in ActionPool)
			totalWeight += action.Weight;

		float pick = Random.Range(0, totalWeight);
		foreach (GenericEnemyAction action in ActionPool)
		{
			pick -= action.Weight;
			if (pick < 0)
			{
				NextAction = action;
				break;
			}
		}
		yield return null;
	}
}

[Serializable]
public class GenericEnemyAction
{
	public WeightedEnemyActionType ActionDisplay;
	public int Amount;
	public float Weight;
}