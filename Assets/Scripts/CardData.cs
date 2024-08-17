using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardLibrary
{
	public static List<Card> AllCards { get; } = new List<Card>();
	public static List<Card> CurrentDeck { get; } = new List<Card>();
}

public interface IReplicable
{
	public Card Replicate();
}

public abstract class Card
{
	/// <summary> New card is current card with random variance in damage block etc.. </summary>
	public abstract Card Replicate();

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