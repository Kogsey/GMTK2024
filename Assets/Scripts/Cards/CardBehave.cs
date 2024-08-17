using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CardBehave : MonoBehaviour
{
	private CardData card;

	public CardData Card
	{
		get => card;
		set
		{
			card = value;
			RegenCard();
		}
	}

	public SpriteRenderer Border;
	public SpriteRenderer CardBack;
	public TextMeshProUGUI Actions;
	public TextMeshProUGUI Description;
	public TextMeshProUGUI Name;

	private void RegenCard()
	{
		Border.sprite = Singleton<CardManager>.instance.Borders[(int)card.Rarity];
		Name.text = card.Name;
		Description.text = string.Format(card.Description, card.CurrentValue);
		Actions.text = card.Energy.ToString();
	}
}