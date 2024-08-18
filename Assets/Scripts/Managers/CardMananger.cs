using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

[Singleton]
public class CardManager : MonoBehaviour, ISingleton
{
	private const int HandSize = 5;
	public CardData[] AllCards;
	public Sprite[] Borders;
	public CardBehave CardPrefab;
	public Transform Canvas;
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
	public Vector2 DeckPosition;
	private IEnumerator DrawHand(int size)
	{
		Debug.WriteLine("DrawHandRun");
		CardBehave.IgnoreNewInteract = true;
		ReCalculateCardRoots();
		MoveCardsToRoot();

		while (Hand.Count < size)
		{
			CardBehave cardBehave = DrawCard();
			cardBehave.transform.position = DeckPosition;
			Hand.Add(cardBehave);
			ReCalculateCardRoots();
			cardBehave.StartMoveToRoot();

			Debug.WriteLine("WaitingForSeconds");
			yield return new WaitForSeconds(0.2f);
		}

		Debug.WriteLine("Ended Draw hand routine.");
		CardBehave.IgnoreNewInteract = false;
		DrawingHand = null;
	}
	/// <summary>
	/// Instantiates a new card, sets that cards values, removes the card from the game deck, and checks if the deck is empty.
	/// </summary>

	public CardBehave DrawCard()
	{
		Debug.WriteLine("DrawCardRun");
		CardBehave cardBehave = Instantiate(CardPrefab, Canvas.transform);
		cardBehave.Card = GameDeck[^1];
		GameDeck.RemoveAt(GameDeck.Count - 1);
		CheckReshuffle();
		return cardBehave;
	}

	private void CheckReshuffle()
	{
		Debug.WriteLine("CheckReshuffleRun");
		if (GameDeck.Count == 0)
		{
			(GameDeck, DiscardDeck) = (DiscardDeck, GameDeck);
			Shuffle(GameDeck);
		}
	}

	private Coroutine DrawingHand;

	public void Start()
	{
		GenerateStartDeck(20);
		CopyDeck();
		DrawingHand = StartCoroutine(DrawHand(HandSize));
	}

	public List<CardData> GameDeck { get; private set; } = new List<CardData>();
	public List<CardBehave> Hand { get; } = new List<CardBehave>();
	public List<CardData> DiscardDeck { get; private set; } = new List<CardData>();

	public IEnumerator EndRound()
	{
		Debug.WriteLine("EndRoundRun");
		if (DrawingHand != null)
			yield return DrawingHand;
		yield return DrawHand(HandSize);
		Debug.WriteLine("EndRoundEnd");
	}

	public void PlayedCard(CardBehave cardBehave)
	{
		Debug.WriteLine($"CardPlayed: {cardBehave}");
		Hand.Remove(cardBehave);
		DiscardDeck.Add(cardBehave.Card);
		if (Hand.Count != 0)
			ReCalculateCardRoots(Hand.Count);
		MoveCardsToRoot();
	}

	/// <summary>
	/// Pass either the current number of cards or <see cref="HandSize"/>
	/// </summary>
	/// <param name="cardCount"> not the actual amount of cards. The amount of card "slots" the function will assume exist. </param>
	public void ReCalculateCardRoots(int cardCount = HandSize)
	{
		Debug.WriteLine("ReCalculateCardRootsRun");
		if (cardCount == 1)
		{
			Hand[0].RootPosition = new Vector2(0, -20);
		}
		else
		{
			float half = 60f / 5 * cardCount / 2;
			for (int i = 0; i < Hand.Count; i++)
			{
				float invLerp = Mathf.InverseLerp(0, cardCount, i);
				float xVal = Mathf.Lerp(-half, half, invLerp);
				Hand[i].RootPosition = new Vector2(xVal, -20);
				Hand[i].CardLayer = i;
			}
		}
	}

	public void MoveCardsToRoot()
	{
		Debug.WriteLine("MoveCardsToRootRun");
		foreach (CardBehave card in Hand)
			card.StartMoveToRoot();
	}
}