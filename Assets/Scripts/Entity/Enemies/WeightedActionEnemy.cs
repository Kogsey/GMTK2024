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
	public IconID ActionToHint(WeightedEnemyActionType weightedEnemyAction)
		=> weightedEnemyAction switch
		{
			WeightedEnemyActionType.Attack => IconID.Sword,
			WeightedEnemyActionType.Block => IconID.ShieldCircle,
			WeightedEnemyActionType.Evade => IconID.BlueLightning,
			WeightedEnemyActionType.BigAttack => IconID.Hammer,
			WeightedEnemyActionType.Weaken => IconID.PurpleSwirl,
			WeightedEnemyActionType.Burn => IconID.Fire,
			WeightedEnemyActionType.Heal => IconID.Heart,
			WeightedEnemyActionType.ElectricAttack => IconID.Lightning,
			_ => throw new NotImplementedException(),
		};

	public WeightedEnemyData WeightedEnemyData
	{
		set
		{
			MoveSprite.transform.localPosition += HealthBar.transform.localPosition * (value.HeightMultiplier - 1);
			HealthBar.transform.localPosition *= value.HeightMultiplier;
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
	public override IconID DisplayAction => ActionToHint(NextAction.ActionDisplay);
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

			case WeightedEnemyActionType.Weaken:
				yield return Singleton<Player>.instance.InflictEffect(new DamageModMultiplier(0.5f, IconID.PurpleSwirl));
				break;

			case WeightedEnemyActionType.Burn:
				yield return Singleton<Player>.instance.InflictEffect(new DoTEffect(3, 3, IconID.Fire));
				break;

			case WeightedEnemyActionType.ElectricAttack:
				yield return Singleton<Player>.instance.InflictEffect(new DoTEffect(2, 3, IconID.Lightning));
				break;

			default:
				throw new NotImplementedException();
		}
		yield return null;
	}

	public override void PickNextAction()
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
				return;
			}
		}
	}
}

[Serializable]
public class GenericEnemyAction
{
	public WeightedEnemyActionType ActionDisplay;
	public int Amount;
	public float Weight;
}