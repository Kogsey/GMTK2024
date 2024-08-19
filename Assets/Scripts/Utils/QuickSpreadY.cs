using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class QuickSpreadY : MonoBehaviour
{
	public float AssumeWidth = 4;
	private RectTransform RectTransform { get; set; }

	public void Update()
	{
		if (RectTransform == null)
			RectTransform = GetComponent<RectTransform>();
		List<Transform> children = new();
		foreach (Transform t in transform)
			children.Add(t);

		if (children.Count == 1)
			children[0].localPosition = Vector3.zero;
		if (children.Count <= 1)
			return;

		for (int childIndex = 0; childIndex < children.Count; childIndex++)
		{
			float invLerp = Mathf.InverseLerp(0, children.Count - 1, childIndex);
			float yVal = Mathf.Lerp(RectTransform.rect.yMin + AssumeWidth, RectTransform.rect.yMax - AssumeWidth, invLerp);
			children[childIndex].localPosition = new Vector3(0, yVal, 0);
		}
	}
}