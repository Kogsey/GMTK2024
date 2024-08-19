using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpriteBank : MonoBehaviour
{
	public static SpriteBank Instance { get; private set; }

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
	}

	public Image BackgroundRenderer;
	public SpriteRenderer IconPrefab;
	public Sprite[] Sprites;

	public Sprite GetSprite(IconID id)
		=> Sprites[(int)id];

	public SpriteRenderer GetSpiteObject(IconID id, Transform parent)
	{
		SpriteRenderer spriteRenderer = Instantiate(IconPrefab, parent);
		spriteRenderer.sprite = GetSprite(id);
		return spriteRenderer;
	}

	public Sprite[] Borders;

	public Sprite GetBorder(Rarity rarity)
		=> Borders[(int)rarity];

	public Sprite[] Backgrounds;

	public void SetBackGround(EnemyGeneration level) => BackgroundRenderer.sprite =
		EnumUtility.HasFlag(level, EnemyGeneration.Robot) ? Backgrounds[0] :
		EnumUtility.HasFlag(level, EnemyGeneration.Plants) ? Backgrounds[1] :
		Backgrounds[2];
}

public enum IconID
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