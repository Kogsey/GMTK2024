using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class WeightedActionEnemy : Enemy
{
	private GenericEnemyAction NextAction;
	public override EnemyActionType DisplayAction => NextAction.ActionDisplay;
	public override int DisplayActionCount => NextAction.Amount;

	public List<GenericEnemyAction> ActionPool;

	public override IEnumerator Turn()
	{
		switch (NextAction.ActionDisplay)
		{
			case EnemyActionType.Attack:
				yield return Singleton<Player>.instance.Damage(this, NextAction.Amount);
				break;

			case EnemyActionType.Block:
				Block += NextAction.Amount;
				break;

			case EnemyActionType.Evade:
				EvasionChance += NextAction.Amount;
				break;

			case EnemyActionType.Hidden:
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
	public EnemyActionType ActionDisplay;
	public int Amount;
	public float Weight;
}