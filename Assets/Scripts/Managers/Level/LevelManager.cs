using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

[Singleton]
public class LevelManager : MonoBehaviour, ISingleton
{
	public IEnumerable<Entity> AllCardTargets
		=> Enemies.Append<Entity>(Player);

	private List<Enemy> ToRemove { get; } = new List<Enemy>();
	public List<Enemy> Enemies { get; } = new List<Enemy>();
	public Coroutine Coroutine { get; private set; }

	public WeightedEnemyData[] EnemyData;
	public SunBoss SunBossPrefab;
	public WeightedActionEnemy Prefab;
	public RectTransform EnemyParent;
	public Player Player;
	public SpriteRenderer IconPrefab;
	public bool LevelIsOver { get; set; }

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
		GenerateLevel();
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
					yield return Player.ResetStats();
					yield return Player.PreTurnEffects();
					yield return Player.Turn();
					yield return Player.PostTurn();
					break;

				case RoundState.EnemyTurn:
					foreach (Enemy enemy in Enemies)
					{
						Debug.WriteLine(enemy);
						yield return enemy.ResetStats();
						yield return enemy.PreTurnEffects();
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
		LevelIsOver = true;
	}

	public void UpdateKill(Enemy killMe)
	{
		Debug.WriteLine($"{killMe} killed");
		Destroy(killMe.gameObject);
		if (CurrentRoundState == RoundState.EnemyTurn)
			ToRemove.Add(killMe);
		else
			Enemies.Remove(killMe);

		if (HasGameEnded())
		{
			StopCoroutine(Coroutine);
			LevelIsOver = true;
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
		if (enemiesDead)
		{
			if (CampaignState.GetLevelData().SunBoss)
			{
				SceneManager.LoadScene("Win Game", LoadSceneMode.Single);
				CampaignState.OnEndState();
			}
			else
			{
				SceneManager.LoadScene("CardPickScene", LoadSceneMode.Single);
				CampaignState.OnEndState();
			}
		}
		else if (Player.Health <= 0)
		{
			SceneManager.LoadScene("Lose Game", LoadSceneMode.Single);
			CampaignState.OnEndState();
		}
		return enemiesDead || Player.Health <= 0;
	}

	public bool DebugGenSunBoss;

	private void GenerateLevel()
	{
		LevelData levelData = CampaignState.GetLevelData();
		if (DebugGenSunBoss)
			levelData.SunBoss = true;
		if (levelData.SunBoss)
		{
			SunBoss sunBoss = Instantiate(SunBossPrefab);
			Enemies.Add(sunBoss);
		}
		else
		{
			IEnumerable<WeightedEnemyData> pool = levelData.Endless ? EnemyData : EnemyData.Where(enemy => EnumUtility.HasFlag(enemy.GenerationData, levelData.EnemyTypes) && enemy.MinLevel <= levelData.Level);

			for (int i = 0; i < levelData.EnemyCount; i++)
			{
				WeightedActionEnemy weightedActionEnemy = Instantiate(Prefab, EnemyParent);
				WeightedEnemyData data = GetRandom(pool);
				if (EnumUtility.HasFlag(data.GenerationData, EnemyGeneration.Rare) && Random.value < 0.5f)
					data = GetRandom(pool);
				weightedActionEnemy.WeightedEnemyData = data;
				Enemies.Add(weightedActionEnemy);
			}
		}

		SpriteBank.Instance.SetBackGround(levelData);
	}

	private T GetRandom<T>(IEnumerable<T> values)
		=> values.ElementAt(Random.Range(0, values.Count()));
}