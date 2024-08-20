using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameMapNode : MonoBehaviour
{
	public SpriteRenderer SpriteRenderer;
	public Collider2D Collider;
	private MapNode mapNode;
	public bool Debug_ForceAllowPlay;
	public bool Debug_ForceUpdate;
	private Color Colour => MapNode.Visited ? Color.green : MapNode.Locked ? Color.grey : Color.white;

	public MapNode MapNode
	{
		get => mapNode; set
		{
			mapNode = value;
			UpdateMapNode();
		}
	}

	public void Update()
	{
		if (Debug_ForceUpdate)
			UpdateMapNode();
	}

	private void UpdateMapNode()
	{
		SpriteRenderer.sprite = Singleton<MapManager>.instance.GetSprite(MapNode);
		SpriteRenderer.color = Colour;
		Collider.enabled = Debug_ForceAllowPlay || !MapNode.Visited && !MapNode.Locked;
	}

	public void OnMouseEnter()
	{
		SpriteRenderer.color = Color.yellow;
		StartCoroutine(Scale(1.1f));
	}

	public void OnMouseExit()
	{
		SpriteRenderer.color = Colour;
		StartCoroutine(Scale(-1.1f));
	}

	private IEnumerator Scale(float scaleChange, int frameCount = 10)
	{
		for (int i = 0; i < frameCount; i++)
		{
			transform.localScale += Vector3.one * scaleChange / frameCount;
			yield return null;
		}
	}

	public void OnMouseUpAsButton()
		=> CampaignState.SetCurrentLevel(mapNode);
}