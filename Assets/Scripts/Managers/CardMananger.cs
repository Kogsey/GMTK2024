using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class CardManager : MonoBehaviour, ISingleton
{
	private const int HandSize = 5;
	public CardData[] AllCards;
	public Sprite[] Borders;
	public CardBehave CardPrefab;
	public Canvas Canvas;
	public List<CardData> CurrentDeck { get; } = new List<CardData>();

	private float? totalWeight;

	private float TotalWeight
	{
		get
		{
			if (totalWeight == null)
			{
				totalWeight = 0;
				foreach (CardData card in AllCards)
					if (card.Drawable)
						totalWeight += RarityToWeight(card.BaseRarity);
			}
			return totalWeight.Value;
		}
	}

	public void GenerateStartDeck(int count)
	{
		CurrentDeck.Clear();
		for (int i = 0; i < count; i++)
			CurrentDeck.Add(PickRandomCard().NewCardVariant());
	}

	public static float RarityToWeight(Rarity rarity) => rarity switch
	{
		Rarity.Common => 10f,
		Rarity.Rare => 1f,
		Rarity.Epic => 0.1f,
		Rarity.Legendary => 0.5f,
		_ => 0f,
	};

	private CardData PickRandomCard()
	{
		float pick = Random.Range(0, TotalWeight);
		foreach (CardData card in AllCards)
		{
			if (card.Drawable)
			{
				pick -= RarityToWeight(card.BaseRarity);
				if (pick < 0)
					return card;
			}
		}
		return AllCards[^1];
	}

	private void CopyDeck()
	{
		GameDeck.Clear();
		GameDeck.AddRange(CurrentDeck.Select(card => card.Clone()));
		Shuffle(GameDeck);
	}

	private void Shuffle<T>(List<T> list)
	{
		for (int i = 0; i < list.Count - 1; i++)
		{
			int j = Random.Range(i, list.Count);
			(list[j], list[i]) = (list[i], list[j]);
		}
	}

	private void GenerateHand(int size)
	{
		while (Hand.Count < size)
		{
			Hand.Add(Instantiate(CardPrefab, Canvas.transform));
			Hand[^1].Card = GameDeck[^1];
			GameDeck.RemoveAt(GameDeck.Count - 1);
			CheckReshuffle();
		}

		for (int i = 0; i < Hand.Count; i++)
		{
			Hand[i].transform.position = new Vector3((i - 2) * 15, -20);
		}
	}

	private void CheckReshuffle()
	{
		if (GameDeck.Count == 0)
		{
			(GameDeck, DiscardDeck) = (DiscardDeck, GameDeck);
			Shuffle(GameDeck);
		}
	}

	public void Start()
	{
		GenerateStartDeck(20);
		CopyDeck();
		GenerateHand(HandSize);
	}

	public List<CardData> GameDeck { get; private set; } = new List<CardData>();
	public List<CardBehave> Hand { get; } = new List<CardBehave>();
	public List<CardData> DiscardDeck { get; private set; } = new List<CardData>();
}