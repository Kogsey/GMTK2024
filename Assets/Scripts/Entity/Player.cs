using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class Player : Entity, ISingleton
{
	public enum BeeSprite
	{
		NormalBee,
		HardBee,
		PowerBee,
	}

	public Sprite[] BeeSprites;
	public BeeSprite CurrentSpriteID { get; set; } = BeeSprite.NormalBee;
	public Sprite CurrentSprite => BeeSprites[(int)CurrentSpriteID];

	public override Color HighlightColour => Color.green;

	public TextMeshProUGUI EnergyText;
	private CardData _nextCard = null;
	private Entity _nextTarget = null;

	public void SetNextCard(CardData cardData, Entity entity)
	{
		_nextCard = cardData;
		_nextTarget = entity;
	}

	private const int MaxEnergy = 3;
	private int energy = MaxEnergy;

	public int Energy
	{
		get => energy; set
		{
			EnergyText.text = value.ToString();
			energy = value;
		}
	}

	public int ExtraEnergy { get; set; }

	public bool ForceEndTurn;

	public void EndTurnPressed()
		=> ForceEndTurn = true;

	public bool EndPlayerTurn()
		=> Energy <= 0 || ForceEndTurn;

	public override IEnumerator Turn()
	{
		while (!EndPlayerTurn())
		{
			if (_nextTarget != null && _nextCard != null)
			{
				yield return _nextCard.PlayCard(this, _nextTarget);
				_nextCard = null;
				_nextTarget = null;
			}

			yield return null;
		}

		ForceEndTurn = false;
	}

	public override IEnumerator BlockEffect()
	{
		for (int i = 0; i < 5; i++)
		{
			yield return new WaitForSeconds(0.1f);
			SpriteRenderer.sprite = BeeSprites[(int)BeeSprite.HardBee];
			yield return new WaitForSeconds(0.1f);
			SpriteRenderer.sprite = CurrentSprite;
		}
	}

	public override IEnumerator ResetStats()
	{
		Energy = MaxEnergy + ExtraEnergy;
		ExtraEnergy = 0;
		yield return base.ResetStats();
	}

	public override IEnumerator PreTurnEffects()
	{
		foreach (IEntityEffect<Player> effect in Effects)
			yield return effect.OnTurnStart(this);
	}

	protected override void Start()
	{
		base.Start();
		MaxHealth = CampaignState.Instance.PlayerHealthMax;
		Health = CampaignState.Instance.PlayerHealth;
		HealthBar.Setup(MaxHealth);
		AddAnimation(new GenericEntityAnimations(AnimationEffect.SquishY | AnimationEffect.SquishX | AnimationEffect.Wobble | AnimationEffect.Bob));
		transform.position = new Vector3(-35, 0, 0);
	}

	private readonly List<IEntityAnimation<Player>> animations = new();

	protected override void UpdateAnimations()
	{
		foreach (IEntityAnimation<Player> animation in animations)
			animation.UpdateAnimation(this);
	}

	public void AddAnimation(IEntityAnimation<Player> animation)
		=> animations.Add(animation);

	public override void AddAnimation(IEntityAnimation<Entity> animation)
		=> AddAnimation(animation);

	private List<IEntityEffect<Player>> Effects { get; } = new List<IEntityEffect<Player>>();

	public override float ModifyDamage(float damage)
	{
		foreach (IEntityEffect<Player> effect in Effects)
			damage = effect.ModifyDamage(damage);
		return damage;
	}

	public override IEnumerator InflictEffect(IEntityEffect<Entity> effect)
		=> InflictEffect(effect);

	public IEnumerator InflictEffect(IEntityEffect<Player> effect)
	{
		Effects.Add(effect);
		effect.SetupIcon(EffectsHolder.transform);
		yield return effect.OnInflict(this);
	}

	protected override IEnumerator PostTurnEffects()
	{
		for (int index = 0; index < Effects.Count; index++)
		{
			yield return Effects[index].OnTurnEnd(this);
			if (Effects[index].TurnsLeft == 0)
			{
				Effects[index].RemoveIcon();
				Effects.RemoveAt(index);
				index--;
			}
		}
	}

	public override void ClearEffects()
	{
		foreach (IEntityEffect<Player> effect in Effects)
			effect.RemoveIcon();
		Effects.Clear();
	}
}

public class PowerScalingAnimation : IEntityAnimation<Player>
{
	public bool Complete { get; }
	private float timer = 0;

	public void UpdateAnimation(Player entity)
	{
		if (timer == 0)
		{
			entity.CurrentSpriteID = Player.BeeSprite.PowerBee;
			entity.SpriteRenderer.sprite = entity.CurrentSprite;
		}
		timer += Time.smoothDeltaTime;

		if (timer > 10)
		{
			entity.CurrentSpriteID = Player.BeeSprite.NormalBee;
			entity.SpriteRenderer.sprite = entity.CurrentSprite;
		}
	}
}