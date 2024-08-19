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
	Heal,
	BigAttack,
	Weaken,
	Burn,
	ElectricAttack,
}

public class WeightedActionEnemy : Enemy
{
	public IconIDs ActionToHint(WeightedEnemyActionType weightedEnemyAction)
		=> weightedEnemyAction switch
		{
			WeightedEnemyActionType.Attack => IconIDs.Sword,
			WeightedEnemyActionType.Block => IconIDs.ShieldCircle,
			WeightedEnemyActionType.Evade => IconIDs.BlueLightning,
			WeightedEnemyActionType.BigAttack => IconIDs.Hammer,
			WeightedEnemyActionType.Weaken => IconIDs.PurpleSwirl,
			WeightedEnemyActionType.Burn => IconIDs.Fire,
			WeightedEnemyActionType.Heal => IconIDs.Heart,
			WeightedEnemyActionType.ElectricAttack => IconIDs.Lightning,
			_ => throw new NotImplementedException(),
		};

	public WeightedEnemyData WeightedEnemyData
	{
		set
		{
			if (value.Tall)
				HealthBar.transform.localPosition *= 2;
			MaxHealth = Random.Range(value.MinHealth, value.MaxHealth + 1);
			Health = MaxHealth;
			Block = value.StartBlock;
			Absorption = value.StartAbsorb;
			SpriteRenderer.sprite = value.Sprite;
			ActionPool = new List<GenericEnemyAction>(value.ActionPool);
			AddAnimation(new GenericEntityAnimations(value.AnimationEffects));
		}
	}

	private List<GenericEnemyAction> ActionPool;
	private GenericEnemyAction NextAction;
	public override IconIDs DisplayAction => ActionToHint(NextAction.ActionDisplay);
	public override int DisplayActionCount => NextAction.Amount;

	public override IEnumerator Turn()
	{
		switch (NextAction.ActionDisplay)
		{
			case WeightedEnemyActionType.Attack:
				StartCoroutine(AttackAnimation());
				yield return Singleton<Player>.instance.Damage(this, NextAction.Amount);
				break;

			case WeightedEnemyActionType.Block:
				Block += NextAction.Amount;
				break;

			case WeightedEnemyActionType.Evade:
				EvasionChance += NextAction.Amount;
				break;

			case WeightedEnemyActionType.Heal:
				Health += NextAction.Amount;
				Health = Math.Min(Health, MaxHealth);
				break;

			case WeightedEnemyActionType.BigAttack:
				goto case WeightedEnemyActionType.Attack;

			case WeightedEnemyActionType.Weaken: // TODO
				break;

			case WeightedEnemyActionType.Burn: // TODO
				break;

			case WeightedEnemyActionType.ElectricAttack:
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