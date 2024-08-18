using System.Collections;
using UnityEngine;

public static class Helpers
{
	public static Vector2 SmoothInterpolate(Vector2 a, Vector2 b, float decayScale = 16)
		=> b + (a - b) * Mathf.Exp(-decayScale * Time.smoothDeltaTime);

	public static Vector2 WorldMousePosition
		=> Camera.main.ScreenToWorldPoint(Input.mousePosition);

	public static IEnumerator Chain(this IEnumerator enumerator1, IEnumerator enumerator2)
	{
		yield return enumerator1;
		yield return enumerator2;
	}
}