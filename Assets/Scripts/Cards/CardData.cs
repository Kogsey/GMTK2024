using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

	public CardData DeckCard { get; private set; }
	public Sprite CardArt;
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
		clone.DeckCard = this;
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

	public bool CanPlayCard(Entity target) => Energy <= Singleton<Player>.instance.Energy
		&& (target is Player ? EnumUtility.HasFlag(Target, TargetType.Player) : EnumUtility.HasFlag(Target, TargetType.Enemy));

	public IEnumerator PlayCard(Player player, Entity Target)
	{
		player.Energy -= Energy;

		if (name == "Power Scaling")
			player.AddAnimation(new PowerScalingAnimation());

		switch (CardType)
		{
			case CardType.Attack:
				player.StartCoroutine(player.AttackAnimation(1));
				yield return Target.Damage(player, CurrentValue);
				break;

			case CardType.Block:
				Target.Block += CurrentValue;
				break;

			case CardType.Evade:
				Target.EvasionChance += CurrentValue;
				break;

			case CardType.AoE:
				player.StartCoroutine(player.AttackAnimation(1));
				IEnumerable<Coroutine> routines = Singleton<LevelManager>.instance.Enemies.Select(enemy => player.StartCoroutine(enemy.Damage(player, CurrentValue)));
				foreach (Coroutine coroutine in routines)
					yield return coroutine;
				break;

			case CardType.DoubleAttack:
				player.StartCoroutine(player.AttackAnimation(1).Chain(player.AttackAnimation(1)));
				yield return Target.Damage(player, CurrentValue);
				yield return Target.Damage(player, CurrentValue);
				break;

			case CardType.Feint:
				player.StartCoroutine(player.AttackAnimation(1));
				player.EvasionChance += 10;
				yield return Target.Damage(player, CurrentValue);
				break;

			case CardType.StackingEvade:
				player.EvasionChance += CurrentValue;
				player.StackingEvade += 20;
				break;

			case CardType.Heal:
				player.Health += CurrentValue;
				player.Health = Math.Min(player.Health, player.MaxHealth);
				CurrentValue /= 2;
				DeckCard.CurrentValue /= 2;
				if (DeckCard.CurrentValue < 1)
					DeckCard.CurrentValue = 1;
				break;

			case CardType.HealthBuffer:
				player.Absorption += CurrentValue;
				CurrentValue /= 2;
				DeckCard.CurrentValue /= 2;
				if (DeckCard.CurrentValue < 1)
					DeckCard.CurrentValue = 1;
				break;

			case CardType.FaultyReplicate:
				player.StartCoroutine(player.OnHitEffect());
				player.Health -= CurrentValue;
				yield return player.InflictEffect(new DamageModMultiplier(2, IconID.Sword));
				break;

			case CardType.Prepare:
				player.ExtraEnergy += 1;
				break;

			default:
				throw new NotImplementedException();
		}
	}
}