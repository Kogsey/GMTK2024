using UnityEngine;

public class PathHelper : MonoBehaviour
{
	public SpriteRenderer SpriteRenderer;
	public GameMapNode Node1 { get; set; }
	public GameMapNode Node2 { get; set; }

	public void Update()
	{
		Vector2 endPosition = Node2.transform.position;
		Vector2 startPosition = Node1.transform.position;
		float angle = Mathf.Rad2Deg * Mathf.Atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x);
		SpriteRenderer.size = new Vector2(Vector2.Distance(startPosition, endPosition), 1.25f);

		Vector2 centrePos = startPosition + (endPosition - startPosition) / 2;
		transform.SetPositionAndRotation(centrePos, Quaternion.AngleAxis(angle, Vector3.forward));
	}
}