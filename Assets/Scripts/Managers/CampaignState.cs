using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Application = UnityEngine.Application;
using Random = UnityEngine.Random;

public class MapNode : IEnumerable<MapNode>
{
	public enum LevelType
	{
		Start,
		Treasure,
		EnemyNormal,
		EnemyHard,
		SunBoss,
	}

	public static MapNode MakeBossNode(List<MapNode> lastNodes, int levelCount)
	{
		MapNode bossNode = new(LevelType.SunBoss, levelCount, levelCount);
		foreach (MapNode node in lastNodes)
			node.Next.Add(bossNode);
		return bossNode;
	}

	public static MapNode MakeStartNode(int levelCount)
		=> new(LevelType.Start, 0, levelCount)
		{
			Visited = true,
			Locked = false,
		};

	private MapNode(LevelType levelType, int levelNumber, int levelCount)
	{
		NodeLevelType = levelType;
		EnemyGeneration = levelNumber <= levelCount / 3
			? EnemyGeneration.Robot
			: levelNumber <= levelCount / 3 * 2
				? EnemyGeneration.Plants : EnemyGeneration.Space;

		if (levelNumber == 1)
		{
			Visited = false;
			Locked = false;
		}
		LevelNumber = levelNumber;
	}

	private List<MapNode> Next { get; } = new List<MapNode>();
	private MapNode GeneratedFrom { get; set; }
	public int LevelNumber { get; private set; }
	public LevelType NodeLevelType { get; private set; }
	public EnemyGeneration EnemyGeneration { get; private set; } = EnemyGeneration.Robot;
	public bool Visited { get; set; }
	public bool Locked { get; set; } = true;

	public LevelData GetLevelData() => new()
	{
		Difficulty = NodeLevelType != LevelType.EnemyHard ? Random.Range(0, 6) : Random.Range(4, 10),
		EnemyTypes = EnemyGeneration,
		Level = LevelNumber,
		SunBoss = NodeLevelType == LevelType.SunBoss,
		Endless = false,
	};

	public static MapNode GenerateNextNode(MapNode lastNode, int levelNumber, int levelCount)
	{
		static LevelType GenericLevelGen(int hardNodeBaseChance, int levelNumber) =>
			Helpers.PercentCheck(hardNodeBaseChance + levelNumber * 2) ? LevelType.EnemyHard :
			Helpers.PercentCheck(10) ? LevelType.Treasure :
			LevelType.EnemyNormal;
		MapNode newNode = new(lastNode.NodeLevelType switch
		{
			LevelType.Start => LevelType.EnemyNormal,
			LevelType.EnemyNormal => GenericLevelGen(10, levelNumber),
			LevelType.Treasure => GenericLevelGen(50, levelNumber),
			LevelType.EnemyHard => GenericLevelGen(5, levelNumber),
			LevelType.SunBoss => throw new NotImplementedException("Sun boss node should be last node"),
			_ => throw new NotImplementedException(),
		}, levelNumber, levelCount);

		lastNode.Next.Add(newNode);
		newNode.GeneratedFrom = lastNode;
		return newNode;
	}

	/// <summary> Returns the DELETED node. </summary>
	public static MapNode MergeNodes(MapNode node1, MapNode node2)
	{
		MapNode keepNode;
		MapNode killNode;
		if (Random.Range(0, 100) < 50)
		{
			keepNode = node1;
			killNode = node2;
		}
		else
		{
			keepNode = node2;
			killNode = node1;
		}
		killNode.GeneratedFrom.Next.Remove(killNode);
		killNode.GeneratedFrom.Next.Add(keepNode);
		return killNode;
	}

	public IEnumerator<MapNode> GetEnumerator() => Next.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class MapData : IEnumerable<IEnumerable<MapNode>>
{
	private MapData(int length, int pathSplitChange, int pathMergeChance, int maximumNodesPerLayer)
	{
		Length = length;
		PathSplitChange = pathSplitChange;
		PathMergeChance = pathMergeChance;
		MaximumNodesPerLayer = maximumNodesPerLayer;
	}

	public MapNode StartNode { get; private set; }
	public int Length { get; private set; }
	public int PathSplitChange { get; private set; }
	public int PathMergeChance { get; private set; }
	public int MaximumNodesPerLayer { get; private set; }

	public static MapData GenerateMap(int length = 15, int splitChance = 30, int mergeChance = 30, int maxNodesPerLayer = 5)
	{
		MapData newMap = new(length, splitChance, mergeChance, maxNodesPerLayer);
		List<MapNode> lastLayerNodes = new();
		List<MapNode> newLayerNodes = new();
		newMap.StartNode = MapNode.MakeStartNode(length);
		lastLayerNodes.Add(newMap.StartNode);
		bool forceSplit = true;

		for (int levelNum = 1; levelNum < length; levelNum++)
		{
			int availableExtraNodes = maxNodesPerLayer - lastLayerNodes.Count;

			foreach (MapNode node in lastLayerNodes)
			{
				MapNode newNode = MapNode.GenerateNextNode(node, levelNum, length);
				newLayerNodes.Add(newNode);
				while (forceSplit || availableExtraNodes > 0 && Helpers.PercentCheck(splitChance))
				{
					newNode = MapNode.GenerateNextNode(node, levelNum, length);
					newLayerNodes.Add(newNode);
					availableExtraNodes--;
					forceSplit = false;
				}
			}

			for (int nodeIndex = 0; nodeIndex < newLayerNodes.Count; nodeIndex++)
			{
				if (nodeIndex + 1 < newLayerNodes.Count && Helpers.PercentCheck(mergeChance))
				{
					newLayerNodes.Remove(MapNode.MergeNodes(newLayerNodes[nodeIndex], newLayerNodes[nodeIndex + 1]));
				}
			}

			lastLayerNodes = newLayerNodes;
			newLayerNodes = new List<MapNode>();
		}

		MapNode.MakeBossNode(lastLayerNodes, length);

		return newMap;
	}

	public IEnumerator<IEnumerable<MapNode>> GetEnumerator()
	{
		IEnumerable<MapNode> lastLayerNodes = new MapNode[] { StartNode };
		IEnumerable<MapNode> newLayerNodes = Array.Empty<MapNode>();
		while (lastLayerNodes.Count() > 0)
		{
			yield return lastLayerNodes;
			foreach (MapNode node in lastLayerNodes)
				newLayerNodes = newLayerNodes.Union(node.AsEnumerable());
			lastLayerNodes = newLayerNodes;
			newLayerNodes = Array.Empty<MapNode>();
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class LevelData
{
	public int Difficulty;
	public int Level;
	public bool Endless;
	public bool SunBoss;
	public EnemyGeneration EnemyTypes;
}

public class CampaignState : MonoBehaviour
{
	public static CampaignState Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		RegenCamp();
	}

	private void RegenCamp()
	{
		Deck = GenerateStartDeck();
		MapData = MapData.GenerateMap();
		Instance.PlayerHealth = Instance.PlayerHealthMax;

		campaignCurrentMapLevelData = null;
		endlessLevel = 1;
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (SceneManager.GetActiveScene().name == "MainMenu")
				Application.Quit();
		}
	}

	public static bool Endless = true;

	public static void StartCampaign()
	{
		Endless = false;
		SceneManager.LoadScene("Map");
	}

	public static void StartEndless()
	{
		Endless = true;
		SceneManager.LoadScene("BattleScene");
	}

	public static void RestartGame()
	{
		Instance.RegenCamp();
		if (Endless)
			StartEndless();
		else
			StartCampaign();
	}

	public static void MainMenuOnEnd()
	{
		Instance.RegenCamp();
		SceneManager.LoadScene("MainMenu");
	}

	public static void NavigateMainMenu()
		=> SceneManager.LoadScene("MainMenu");

	public static void NavigateThankyou()
		=> SceneManager.LoadScene("ThankYou");

	public static void ExitButton()
		=> Application.Quit();

	public static void SetCurrentLevel(MapNode mapNode)
	{
		mapNode.Visited = true;
		foreach (MapNode nextNodes in mapNode)
			nextNodes.Locked = false;
		Instance.campaignCurrentMapLevelData = mapNode.GetLevelData();

		if (mapNode.NodeLevelType == MapNode.LevelType.Treasure)
			SceneManager.LoadScene("CardPickScene");
		else
			SceneManager.LoadScene("BattleScene");
	}

	private LevelData campaignCurrentMapLevelData;
	private int endlessLevel = 1;

	public static LevelData GetLevelData() => Endless ? new()
	{
		Difficulty = Instance.endlessLevel,
		Level = Instance.endlessLevel,
		Endless = true,
	} : Instance.campaignCurrentMapLevelData;

	public List<CardData> GenerateStartDeck()
	{
		List<CardData> starterDeck = new();
		for (int i = 0; i < StarterCards.Length; i++)
		{
			for (int j = 0; j < 10; j++)
				starterDeck.Add(StarterCards[i].NewCardVariant());
		}
		return starterDeck;
	}

	public CardData[] StarterCards;
	public List<CardData> Deck { get; private set; }
	private int _playerHealth;

	public int PlayerHealth
	{
		get => Endless ? PlayerHealthMax : _playerHealth;
		set => _playerHealth = value;
	}

	public int PlayerHealthMax { get; set; } = 40;
	public MapData MapData { get; private set; }

	public CardData[] AllCards;

	private float? totalWeight;

	private float TotalWeight
	{
		get
		{
			if (totalWeight == null)
			{
				totalWeight = 0;
				foreach (CardData card in AllCards)
					if (card.Drawable)
						totalWeight += RarityToWeight(card.BaseRarity);
			}
			return totalWeight.Value;
		}
	}

	public CardData PickRandomCard()
	{
		float pick = Random.Range(0, TotalWeight);
		foreach (CardData card in AllCards)
		{
			if (card.Drawable)
			{
				pick -= RarityToWeight(card.BaseRarity);
				if (pick < 0)
					return card;
			}
		}
		return AllCards[^1];
	}

	public static void PostCardPicked()
	{
		Instance.endlessLevel++;
		if (Endless)
			SceneManager.LoadScene("BattleScene");
		else
			SceneManager.LoadScene("Map");
	}

	public static float RarityToWeight(Rarity rarity) => rarity switch
	{
		Rarity.Common => 10f,
		Rarity.Rare => 2,
		Rarity.Epic => 0.5f,
		Rarity.Legendary => 0.5f,
		_ => 0f,
	};
}