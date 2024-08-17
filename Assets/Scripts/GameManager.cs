using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class GameManager : MonoBehaviour, ISingleton
{
	public Coroutine Coroutine { get; private set; }
	public List<Enemy> Enemies;
	public Player Player;

	public enum RoundState
	{
		PreStart,
		PlayerTurn,
		EnemyTurn,
		RoundEnd,
	}

	public RoundState CurrentRoundState { get; private set; }

	public void Start()
	{
		Enemies[^1].transform.position = new Vector3(35, 0, 0);
		Coroutine = StartCoroutine(StepRoundLoop());
	}

	public IEnumerator StepRoundLoop()
	{
		yield return null;
		while (!RoundEnded())
		{
			switch (CurrentRoundState)
			{
				case RoundState.PreStart:
					foreach (Enemy enemy in Enemies)
						yield return enemy.PickNextAction();
					break;

				case RoundState.PlayerTurn:
					yield return Player.PreTurn();
					yield return Player.Turn();
					yield return Player.PostTurn();
					break;

				case RoundState.EnemyTurn:
					foreach (Enemy enemy in Enemies)
					{
						yield return enemy.PreTurn();
						yield return enemy.Turn();
						yield return enemy.PostTurn();
					}
					break;

				case RoundState.RoundEnd:
					break;

				default:
					throw new System.ArgumentOutOfRangeException();
			}

			CurrentRoundState += 1;
			if (CurrentRoundState == RoundState.RoundEnd)
				CurrentRoundState = RoundState.PlayerTurn;
		}
	}

	private bool RoundEnded()
	{
		bool enemiesDead = true;
		foreach (Enemy enemy in Enemies)
		{
			if (enemy.Health > 0)
				enemiesDead = false;
		}
		return enemiesDead || Player.Health <= 0;
	}
}