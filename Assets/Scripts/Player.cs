using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class Player : Entity, ISingleton
{
	public CardData PlayCard = null;
	public Entity Target = null;
	private const int MaxEnergy = 3;
	public int Energy { get; private set; } = MaxEnergy;
	private int ExtraEnergy = 1;
	private int DamageMultiplier = 1;

	public override int ModifyDamage(int damage)
		=> base.ModifyDamage(damage * DamageMultiplier);

	public override IEnumerator Turn()
	{
		while (Energy > 0)
		{
			while (PlayCard == null || Target == null)
			{
				yield return null;
			}

			switch (PlayCard.CardType)
			{
				case CardType.Attack:
					yield return Target.Damage(this, PlayCard.CurrentValue);
					break;

				case CardType.Block:
					Target.Block += PlayCard.CurrentValue;
					break;

				case CardType.Evade:
					Target.EvasionChance += PlayCard.CurrentValue;
					break;

				case CardType.AoE:
					IEnumerable<Coroutine> routines = Singleton<GameManager>.instance.Enemies.Select(enemy => StartCoroutine(enemy.Damage(this, PlayCard.CurrentValue)));
					foreach (Coroutine coroutine in routines)
						yield return coroutine;
					break;

				case CardType.DoubleAttack:
					yield return Target.Damage(this, PlayCard.CurrentValue);
					yield return Target.Damage(this, PlayCard.CurrentValue);
					break;

				case CardType.Feint:
					EvasionChance += 10;
					yield return Target.Damage(this, PlayCard.CurrentValue);
					break;

				case CardType.StackingEvade:
					EvasionChance += PlayCard.CurrentValue;
					StackingEvade += 20;
					break;

				case CardType.Heal:
					Health += PlayCard.CurrentValue;
					Health = Math.Min(Health, MaxHealth);
					PlayCard.RootCard.CurrentValue /= 2;
					if (PlayCard.RootCard.CurrentValue < 1)
						PlayCard.RootCard.CurrentValue = 1;
					break;

				case CardType.HealthBuffer:
					Absorption += PlayCard.CurrentValue;
					PlayCard.RootCard.CurrentValue /= 2;
					if (PlayCard.RootCard.CurrentValue < 1)
						PlayCard.RootCard.CurrentValue = 1;
					break;

				case CardType.FaultyReplicate:
					StartCoroutine(OnHitEffect());
					Health -= PlayCard.CurrentValue;
					DamageMultiplier += 1;
					break;

				case CardType.Prepare:
					ExtraEnergy += 1;
					break;

				default:
					throw new NotImplementedException();
			}

			Energy -= PlayCard.Energy;

			PlayCard = null;
			Target = null;

			yield return null;
		}
	}
	public Sprite HardBee;
	public override IEnumerator BlockEffect()
	{
		Sprite startSprite = SpriteRenderer.sprite;
		for (int i = 0; i < 5; i++)
		{
			yield return new WaitForSeconds(0.1f);
			SpriteRenderer.sprite = HardBee;
			yield return new WaitForSeconds(0.1f);
			SpriteRenderer.sprite = startSprite;
		}
	}

	public bool CanPlayCard(CardData card, Entity target) => card.Energy <= Energy
		&& (target is Player ? EnumUtility.HasFlag(card.Target, TargetType.Player) : EnumUtility.HasFlag(card.Target, TargetType.Enemy));
	public override IEnumerator PreTurn()
	{
		Energy = MaxEnergy + ExtraEnergy;
		ExtraEnergy = 0;
		DamageMultiplier = 1;
		yield return base.PreTurn();
	}

	protected override void Start()
	{
		base.Start();
		transform.position = new Vector3(-35, 0, 0);
	}
}