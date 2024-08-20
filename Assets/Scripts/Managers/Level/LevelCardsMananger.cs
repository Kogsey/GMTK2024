using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

[Singleton]
public class CardManager : MonoBehaviour, ISingleton
{
	private const int HandSize = 5;
	public CardBehave CardPrefab;
	public TextMeshProUGUI DeckText;
	public TextMeshProUGUI DiscardText;
	public Transform Canvas;

	public CardData[] DebugDeck;
	public List<CardData> GameDeck { get; private set; } = new List<CardData>();
	public List<CardBehave> Hand { get; } = new List<CardBehave>();
	public List<CardData> DiscardDeck { get; private set; } = new List<CardData>();

	private void CopyDeck()
	{
		GameDeck.Clear();
		GameDeck.AddRange(CampaignState.Instance.Deck.Select(card => card.Clone()));
		GameDeck.AddRange(DebugDeck);
		Shuffle(GameDeck);
	}

	private void Shuffle<T>(List<T> list)
	{
		SoundManager.PlayCardShuffle();
		for (int i = 0; i < list.Count - 1; i++)
		{
			int j = Random.Range(i, list.Count);
			(list[j], list[i]) = (list[i], list[j]);
		}
	}

	public Vector2 DeckPosition;
	public Vector2 DiscardPosition;

	private IEnumerator DiscardHand()
	{
		SoundManager.PlayCardShuffle();
		Debug.WriteLine("DiscardHand");
		CardBehave.IgnoreNewInteract = true;

		while (Hand.Count > 0)
		{
			CardBehave cardBehave = Hand[0];
			MoveCardToDiscard(cardBehave);

			Debug.WriteLine("WaitingForSeconds");
			yield return new WaitForSeconds(0.2f);
			Destroy(cardBehave.gameObject);
		}

		Debug.WriteLine("Ended discard hand routine.");
		CardBehave.IgnoreNewInteract = false;
		DrawingHand = null;
	}

	private IEnumerator DrawHand(int size)
	{
		Debug.WriteLine("DrawHandRun");
		CardBehave.IgnoreNewInteract = true;
		ReCalculateCardRoots();
		MoveCardsToRoot();

		while (Hand.Count < size)
		{
			CardBehave cardBehave = DrawCard();
			cardBehave.Position = DeckPosition;
			cardBehave.Scale = Vector2.one * 0.001f;
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

	/// <summary> Instantiates a new card, sets that cards values, removes the card from the game deck, and checks if the deck is empty. </summary>

	public CardBehave DrawCard()
	{
		Debug.WriteLine("DrawCardRun");
		CardBehave cardBehave = Instantiate(CardPrefab, Canvas.transform);
		cardBehave.Card = GameDeck[^1];
		GameDeck.RemoveAt(GameDeck.Count - 1);
		DeckText.text = GameDeck.Count.ToString();
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
			StartCoroutine(ChangeCount(DiscardText, GameDeck.Count, DiscardDeck));
			StartCoroutine(ChangeCount(DeckText, DiscardDeck.Count, GameDeck));
		}
	}

	private IEnumerator ChangeCount(TextMeshProUGUI mesh, int from, List<CardData> to)
	{
		for (; from != to.Count; from -= (int)math.sign(from - to.Count))
		{
			mesh.text = from.ToString();
			yield return null;
		}
	}

	private Coroutine DrawingHand;

	public void Start()
	{
		CopyDeck();
		DrawingHand = StartCoroutine(DrawHand(HandSize));
	}

	public IEnumerator EndRound()
	{
		Debug.WriteLine("EndRoundRun");
		if (DrawingHand != null)
			yield return DrawingHand;
		yield return DiscardHand();
		yield return DrawHand(HandSize);
		Debug.WriteLine("EndRoundEnd");
	}

	public void PlayedCard(CardBehave cardBehave)
	{
		Debug.WriteLine($"CardPlayed: {cardBehave}");
		MoveCardToDiscard(cardBehave);
		if (Hand.Count != 0)
			ReCalculateCardRoots(Hand.Count);
		MoveCardsToRoot();
	}

	private void MoveCardToDiscard(CardBehave cardBehave)
	{
		Hand.Remove(cardBehave);
		DiscardDeck.Add(cardBehave.Card);
		DiscardText.text = DiscardDeck.Count.ToString();
		cardBehave.RootPosition = DiscardPosition;
		cardBehave.RootScale = Vector2.one * 0.001f;
		cardBehave.ForceMoveToRoot();
	}

	/// <summary> Pass either the current number of cards or <see cref="HandSize"/> </summary>
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
			card.ForceMoveToRoot();
	}
}