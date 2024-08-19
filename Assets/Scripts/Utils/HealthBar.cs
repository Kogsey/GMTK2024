using System;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
	private const float RefScale = 8;
	private const float BorderPadding = 0.1f;
	private float maxWidth;
	public SpriteRenderer Border;
	public SpriteRenderer Bar;
	public SpriteRenderer Block;
	public SpriteRenderer Absorption;

	public void Setup(int maxHealth)
	{
		maxWidth = maxHealth / RefScale;
		Border.size = new Vector2(maxWidth + BorderPadding, Border.size.y);
		UpdateBar(maxHealth, 0, 0);
	}

	public void UpdateBar(int health, int block, int absorption)
	{
		float bar = MathF.Max(health / RefScale, 0.5f);
		Bar.size = new Vector2(bar, 1);
		Bar.transform.localPosition = new Vector3((Bar.size.x - maxWidth) / 2, 0, 0);

		Absorption.size = new Vector2(Bar.size.x + absorption / RefScale, 1);
		Absorption.transform.localPosition = new Vector3((Absorption.size.x - maxWidth) / 2, 0, 0);

		Block.size = new Vector2(Absorption.size.x + block / RefScale, 1);
		Block.transform.localPosition = new Vector3((Block.size.x - maxWidth) / 2, 0, 0);
	}
}