using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

[Singleton]
public class GameManager : MonoBehaviour, ISingleton
{
	public IEnumerable<Entity> AllCardTargets
		=> Enemies.Append<Entity>(Player);

	private List<Enemy> ToRemove { get; } = new List<Enemy>();
	public List<Enemy> Enemies { get; } = new List<Enemy>();
	public Coroutine Coroutine { get; private set; }

	[Min(1)]
	public int DifficultyRating;

	public List<WeightedEnemyData> EnemyData;
	public WeightedActionEnemy Prefab;
	public RectTransform EnemyParent;
	public Player Player;
	public EnemyGeneration CurrentMode = EnemyGeneration.Robot;
	public bool GameIsOver { get; set; }

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
		GenerateRound(DifficultyRating);
		Coroutine = StartCoroutine(StepRoundLoop());
	}

	public IEnumerator StepRoundLoop()
	{
		yield return null;
		while (!HasGameEnded())
		{
			switch (CurrentRoundState)
			{
				case RoundState.PreStart:
					Debug.WriteLine("PreStart");
					foreach (Enemy enemy in Enemies)
						yield return enemy.PostTurn();
					break;

				case RoundState.PlayerTurn:
					Debug.WriteLine("PlayerTurn");
					yield return Player.PreTurn();
					yield return Player.Turn();
					yield return Player.PostTurn();
					break;

				case RoundState.EnemyTurn:
					foreach (Enemy enemy in Enemies)
					{
						Debug.WriteLine(enemy);
						yield return enemy.PreTurn();
						yield return enemy.Turn();
						yield return enemy.PostTurn();
					}
					break;

				case RoundState.RoundEnd:
					Debug.WriteLine("RoundEnd");
					yield return Singleton<CardManager>.instance.EndRound();
					break;

				default:
					throw new System.ArgumentOutOfRangeException();
			}

			CurrentRoundState += 1;
			if (CurrentRoundState > RoundState.RoundEnd)
				CurrentRoundState = RoundState.PlayerTurn;

			foreach (Enemy killMe in ToRemove)
				Enemies.Remove(killMe);
			ToRemove.Clear();
		}
		GameIsOver = true;
	}

	public void UpdateKill(Enemy killMe)
	{
		Debug.WriteLine($"{killMe} killed");
		Destroy(killMe.gameObject);
		if (CurrentRoundState == RoundState.EnemyTurn)
			ToRemove.Add(killMe);
		else
			Enemies.Remove(killMe);

		if (Enemies.Count == 0)
		{
			StopCoroutine(Coroutine);
			GameIsOver = true;
		}
	}

	private bool HasGameEnded()
	{
		bool enemiesDead = true;
		foreach (Enemy enemy in Enemies)
		{
			if (enemy.Health > 0)
				enemiesDead = false;
		}
		return enemiesDead || Player.Health <= 0;
	}

	private void GenerateRound(int level)
	{
		IEnumerable<WeightedEnemyData> pool = EnemyData.Where(d => EnumUtility.HasFlag(d.GenerationData, CurrentRoundState));
		int enemyCount = level + 1;
		int offsetCount = enemyCount / 2;
		float enemyOffMultiplier = 35 / offsetCount;
		for (int i = 0; i < enemyCount; i++)
		{
			WeightedActionEnemy weightedActionEnemy = Instantiate(Prefab, EnemyParent);
			WeightedEnemyData data = GetRandom(pool);
			if (EnumUtility.HasFlag(data.GenerationData, EnemyGeneration.Rare) && Random.value < 0.5f)
				data = GetRandom(pool);
			weightedActionEnemy.WeightedEnemyData = data;
			Enemies.Add(weightedActionEnemy);
			Enemies[^1].transform.position = new Vector3(35 + (i - offsetCount) * enemyOffMultiplier, 0, 0);
		}
	}

	private T GetRandom<T>(IEnumerable<T> values)
		=> values.ElementAt(Random.Range(0, values.Count()));

	public void Update()
	{
		if (GameIsOver)
			throw new System.Exception("Round ended");
	}
}