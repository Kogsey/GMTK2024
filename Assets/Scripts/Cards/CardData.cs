using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CardLibrary
{
	public static List<Card> AllCards { get; } = new List<Card>();
	public static List<Card> CurrentDeck { get; } = new List<Card>();

	public static float RarityToWeight(Rarity rarity) => rarity switch
	{
		Rarity.Common => 1f,
		Rarity.Rare => 0.5f,
		Rarity.Epic => 0.25f,
		Rarity.Legendary => 0.1f,
		_ => 0f,
	};

	public static (Card, Card, Card) GetCardDraw()
	{
		float totalWeight = 0;
		foreach (Card card in AllCards)
			if (card.Drawable)
				totalWeight += RarityToWeight(card.BaseRarity);
		return (PickCard(Random.Range(0, totalWeight)).NewCardVariant(),
			PickCard(Random.Range(0, totalWeight)).NewCardVariant(),
			PickCard(Random.Range(0, totalWeight)).NewCardVariant());
	}

	private static Card PickCard(float pick)
	{
		foreach (Card card in AllCards)
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
}

public interface IReplicable
{
	public Card Replicate();
}

public enum Rarity
{
	Common,
	Rare,
	Epic,
	Legendary,
}

public abstract class Card
{
	public virtual bool Drawable => true;
	public abstract Rarity BaseRarity { get; }
	public abstract Rarity Rarity { get; }

	/// <summary> New card is current card with random variance in damage block etc.. </summary>
	public abstract Card NewCardVariant();

	[Flags]
	public enum TargetType
	{
		Enemy,
		Player,
	}

	public virtual TargetType Target { get; set; }

	public virtual void OnDraw()
	{
	}

	public virtual void OnUse(Entity usedOn)
		=> Debug.Assert(Target == TargetType.Enemy ? usedOn is Enemy : usedOn is Player);
}