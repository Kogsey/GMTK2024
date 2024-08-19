using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Helpers
{
	public static Vector2 SmoothInterpolate(Vector2 a, Vector2 b, float decayScale = 16)
		=> b + (a - b) * Mathf.Exp(-decayScale * Time.smoothDeltaTime);

	public static Vector2 WorldMousePosition
		=> Camera.main.ScreenToWorldPoint(Input.mousePosition);

	public static bool PercentCheck(int percent)
		=> Random.Range(0, 100) < percent;

	public static Vector3 MemberWiseMultiply(Vector3 v1, Vector3 v2)
		=> new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);

	public static IEnumerator Chain(this IEnumerator enumerator1, IEnumerator enumerator2)
	{
		yield return enumerator1;
		yield return enumerator2;
	}
}