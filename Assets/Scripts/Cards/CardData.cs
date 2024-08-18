using System;
using UnityEngine;

public enum Rarity
{
	Common,
	Rare,
	Epic,
	Legendary,
}

[Flags]
public enum TargetType
{
	Enemy = 1,
	Player = 2,
}

public enum CardType
{
	Attack,
	Block,
	Evade,
	AoE,
	DoubleAttack,
	Feint,
	StackingEvade,
	Heal,
	HealthBuffer,
	FaultyReplicate,
	Prepare,
}

[CreateAssetMenu(fileName = "DefaultCard", menuName = "ScriptableObjects/CardData", order = 1)]
public class CardData : ScriptableObject
{
	/// <summary> If the card can be drawn from the greater pool </summary>
	public bool Drawable = false;

	public CardData RootCard { get; private set; }
	public string Name;
	public string Description;
	public int Energy;

	public Rarity BaseRarity = Rarity.Common;

	public int MinValue;
	public int MaxValue;
	public int RareValue = -1;
	public CardType CardType;

	public CardData Clone()
	{
		CardData clone = (CardData)MemberwiseClone();
		if (RootCard == null)
			clone.RootCard = this;
		return clone;
	}

	private const int RarityUpChance = 10;
	public int CurrentValue { get; set; }

	public Rarity Rarity => RareValue > 0 && CurrentValue == RareValue
				? (Rarity)Math.Min((int)(BaseRarity + 1), (int)Rarity.Legendary)
				: BaseRarity;

	public TargetType Target => CardType switch
	{
		CardType.Attack => TargetType.Enemy,
		CardType.DoubleAttack => TargetType.Enemy,
		CardType.Block => TargetType.Player,
		CardType.Evade => TargetType.Player,
		CardType.AoE => TargetType.Enemy,
		CardType.Feint => TargetType.Enemy,
		CardType.StackingEvade => TargetType.Player,
		CardType.Heal => TargetType.Player,
		CardType.HealthBuffer => TargetType.Player,
		CardType.FaultyReplicate => TargetType.Player,
		CardType.Prepare => TargetType.Player,
		_ => throw new NotImplementedException(),
	};

	public void OnEnable() => SetupAsDefault();

	public void SetupAsDefault() => CurrentValue = MinValue + (MaxValue - MinValue) / 2;

	/// <summary> New card is current card with random variance in damage block etc.. </summary>
	public CardData NewCardVariant()
	{
		CardData clone = Clone();
		clone.CurrentValue = (RareValue > 0 && UnityEngine.Random.Range(0, 100) < RarityUpChance)
			? RareValue
			: UnityEngine.Random.Range(MinValue, MaxValue + 1);
		return clone;
	}
}