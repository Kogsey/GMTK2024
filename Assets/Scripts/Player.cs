using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class Player : Entity, ISingleton
{
	public override Color HighlightColour => Color.green;
	private CardData PlayCard = null;
	private Entity Target = null;
	public TextMeshProUGUI EnergyText;
	public void SetNextCard(CardData cardData, Entity entity)
	{
		PlayCard = cardData;
		Target = entity;
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
	public int DamageMultiplier { get; set; } = 1;

	public override int ModifyDamage(int damage)
		=> base.ModifyDamage(damage * DamageMultiplier);
	public bool ForceEndTurn;
	public void EndTurnPressed()
		=> ForceEndTurn = true;
	public bool EndPlayerTurn()
		=> Energy <= 0 || ForceEndTurn;

	public override IEnumerator Turn()
	{
		while (!EndPlayerTurn())
		{
			if (Target != null && PlayCard != null)
			{
				yield return PlayCard.PlayCard(this, Target);
				PlayCard = null;
				Target = null;
			}

			yield return null;
		}

		ForceEndTurn = false;
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

	public override IEnumerator PreTurn()
	{
		Energy = MaxEnergy + ExtraEnergy;
		ExtraEnergy = 0;
		DamageMultiplier = 1;
		yield return base.PreTurn();
	}

	public override IEnumerator PostTurn()
	{
		yield break;
	}

	protected override void Start()
	{
		base.Start();
		transform.position = new Vector3(-35, 0, 0);
	}
}