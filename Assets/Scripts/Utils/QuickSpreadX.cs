using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class QuickSpreadX : MonoBehaviour
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
			float xVal = Mathf.Lerp(RectTransform.rect.xMin + AssumeWidth, RectTransform.rect.xMax - AssumeWidth, invLerp);
			children[childIndex].localPosition = new Vector3(xVal, 0, 0);
		}
	}
}