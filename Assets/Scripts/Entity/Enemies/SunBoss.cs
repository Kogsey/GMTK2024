using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SunBoss : Enemy
{
	private delegate IEnumerator TurnFunction();

	private float intensity = 1f;
	public float ScaleVIMultiplier = 1.1f;

	public float MoveSpeed;
	public float RingSpeedMultiplier;
	public float SunSpeedMultiplier;
	public float PositionMultiplier;
	private float MovementTick;
	public SpriteRenderer Sun;
	public SpriteRenderer Ring;
	private TurnFunction TurnFunc;
	private int MovePickVar;
	private int MoveRepeatsLeft = 0;
	private IconID displayAction;
	protected override Vector3 BaseScale => intensity * ScaleVIMultiplier * base.BaseScale;
	public override Color TargetColour => Color.Lerp(base.TargetColour, Color.red, (intensity - 1) / 2);

	protected override Color DrawColour
	{
		set
		{
			Sun.color = value;
			Ring.color = value;
			base.DrawColour = value;
		}
	}

	public override IconID DisplayAction => displayAction;
	public override int DisplayActionCount { get; }

	protected override void Start()
	{
		AddAnimation(new GenericEntityAnimations(AnimationEffect.SquishY | AnimationEffect.SquishX));
		base.Start();
	}

	protected override void UpdateAnimations()
	{
		base.UpdateAnimations();
		intensity = Mathf.Max(intensity, 2f);
		MovementTick += MoveSpeed * Time.smoothDeltaTime * intensity * 1.4f;
		Ring.transform.localRotation = Quaternion.AngleAxis(MovementTick * RingSpeedMultiplier, Vector3.forward);
		Sun.transform.position = intensity * PositionMultiplier * new Vector3(Mathf.Cos(MovementTick * SunSpeedMultiplier), 0.5f * Mathf.Sin(2 * MovementTick * SunSpeedMultiplier));
	}

	public override IEnumerator Turn()
	{
		MoveRepeatsLeft--;
		IEnumerator turnEnumerator = TurnFunc();
		while (turnEnumerator.MoveNext())
			yield return turnEnumerator.Current;
	}

	public override void PickNextAction()
	{
		if (MoveRepeatsLeft <= 0)
		{
			switch (MovePickVar)
			{
				case 0:
					TurnFunc = HeatUp;
					MoveRepeatsLeft = Random.Range(1, 5);
					displayAction = IconID.Fire;
					MovePickVar = 1;
					break;

				default:
				case 1:
					TurnFunc = BigAttack;
					MoveRepeatsLeft = 1;
					displayAction = IconID.Hammer;
					MovePickVar = 0;
					break;
			}
		}
	}

	public IEnumerator HeatUp()
	{
		yield return InflictEffect(new DamageModMultiplier(1.2f, IconID.Skull) { TurnsLeft = -1 });
		for (int i = 0; i < 10; i++)
		{
			intensity *= 1.01f;
			yield return new WaitForSeconds(0.1f);
			DrawColour = Color.red;
			yield return new WaitForSeconds(0.1f);
			DrawColour = TargetColour;
		}
	}

	public IEnumerator BigAttack()
	{
		for (int i = 0; i < 10; i++)
		{
			intensity = Helpers.SmoothInterpolate(intensity, 1f);
			DrawColour = TargetColour;
			yield return new WaitForSeconds(0.1f);
		}
		yield return Singleton<Player>.instance.Damage(this, 10, new DoTEffect(5, 2, IconID.Fire));
		ClearEffects();
	}
}