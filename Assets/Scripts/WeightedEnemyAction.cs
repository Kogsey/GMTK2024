using System.Collections.Generic;
using UnityEngine;

public class WeightedActionEnemy : Enemy
{
	private (GenericEnemyAction action, float weight)[] _actionPool;
	public List<(EnemyActionType ActionType, int Amount, float Weight)> ActionPool { get; set; }

	protected override void PreTurn()
		=> NextAction.PrepAction();

	public override void DoTurn()
		=> NextAction.DoAction();

	protected override void PickNextAction()
	{
		float totalWeight = 0;
		foreach ((EnemyAction _, float weight) in _actionPool)
		{
			totalWeight += weight;
		}

		float pick = Random.Range(0, totalWeight);
		foreach ((EnemyAction action, float weight) in _actionPool)
		{
			pick -= weight;
			if (pick < 0)
			{
				NextAction = action;
				return;
			}
		}
	}
}