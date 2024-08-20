using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class MapManager : MonoBehaviour, ISingleton
{
	public Sprite[] Sprites;
	private MapData MapData { get; set; }
	private List<GameMapNode> GameMapNodes { get; } = new List<GameMapNode>();

	public QuickSpreadY QuickSpreadYPrefab;
	public GameMapNode MapNodePrefab;
	public QuickSpreadX MapQuickSpread;
	public PathHelper PathPrefab;

	public Sprite GetSprite(MapNode node) => node.NodeLevelType switch
	{
		MapNode.LevelType.Start => Sprites[0],
		MapNode.LevelType.Treasure => Sprites[node.Visited ? 5 : 4],
		MapNode.LevelType.EnemyNormal => Sprites[2],
		MapNode.LevelType.EnemyHard => Sprites[3],
		MapNode.LevelType.SunBoss => Sprites[12],
		_ => throw new System.Exception("Invalid node type"),
	};

	public void Awake()
	{
		MapData = CampaignState.Instance.MapData;
		int TwoAgoCount = 0;
		int LastCount = 0;
		RectTransform lastTransform;
		foreach (IEnumerable<MapNode> mapNodeLayer in MapData)
		{
			QuickSpreadY layer = Instantiate(QuickSpreadYPrefab, MapQuickSpread.transform);

			int count = 0;
			foreach (MapNode mapNode in mapNodeLayer)
			{
				count++;
				GameMapNode mNode = Instantiate(MapNodePrefab, layer.transform);
				GameMapNodes.Add(mNode);
				mNode.MapNode = mapNode;
			}

			RectTransform trans = layer.GetComponent<RectTransform>();
			Vector2 sizeDelta = trans.sizeDelta;
			float minScale = Mathf.Lerp(0.4f, 0.9f, (float)count / MapData.MaximumNodesPerLayer);
			float maxScale = Mathf.Lerp(0.6f, 1.1f, (float)count / MapData.MaximumNodesPerLayer);
			sizeDelta.y *= Random.Range(minScale, maxScale);
			trans.sizeDelta = sizeDelta;

			if (TwoAgoCount == 1 && LastCount == 2 && count == 1)
			{
				sizeDelta = trans.sizeDelta;
				sizeDelta.y *= Random.Range(0.1f, 0.2f);
				trans.sizeDelta = sizeDelta;
			}
			TwoAgoCount = LastCount;
			LastCount = count;
			lastTransform = trans;
		}

		foreach (GameMapNode mapNode in GameMapNodes)
		{
			foreach (GameMapNode nextNodes in mapNode.MapNode.Select(n => GetGameNodeFromNode(n)))
			{
				PathHelper pathHelper = Instantiate(PathPrefab);
				pathHelper.Node1 = mapNode;
				pathHelper.Node2 = nextNodes;
			}
		}
	}

	private GameMapNode GetGameNodeFromNode(MapNode node)
	{
		foreach (GameMapNode gameMapNode in GameMapNodes)
			if (gameMapNode.MapNode == node)
				return gameMapNode;
		throw new System.Exception("No game map node had that node.");
	}
}