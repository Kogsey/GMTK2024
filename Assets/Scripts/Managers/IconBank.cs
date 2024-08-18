using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class IconBank : MonoBehaviour, ISingleton
{
	public SpriteRenderer IconPrefab;
	public Sprite[] Sprites;

	public Sprite GetSprite(IconIDs id)
		=> Sprites[(int)id];

	public SpriteRenderer GetSpiteObject(IconIDs id, Transform parent)
	{
		SpriteRenderer spriteRenderer = Instantiate(IconPrefab, parent);
		spriteRenderer.sprite = GetSprite(id);
		return spriteRenderer;
	}
}

public enum IconIDs
{
	Heart,
	DeathHeart,
	Sword,
	ShieldTriangle,
	ShieldCircle,
	Lightning,
	BlueLightning,
	Hammer,
	GreenSwirl,
	PurpleSwirl,
	Skull,
	Fire,
	IceFire,
	Potion,
	BrownQuestion,
	YellowQuestion,
	redQuestion,
	BrownExclaim,
	YellowExclaim,
	RedExclaim,
}