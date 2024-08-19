using System.Collections;
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
	private bool PickNewTurn = true;
	private IconID displayAction;
	protected override Vector3 BaseScale => intensity * ScaleVIMultiplier * base.BaseScale;

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
		MovementTick += MoveSpeed * Time.smoothDeltaTime * intensity * 1.4f;
		Ring.transform.localRotation = Quaternion.AngleAxis(MovementTick * RingSpeedMultiplier, Vector3.forward);
		Sun.transform.position = intensity * PositionMultiplier * new Vector3(Mathf.Cos(MovementTick * SunSpeedMultiplier), 0.5f * Mathf.Sin(2 * MovementTick * SunSpeedMultiplier));
	}

	public override IEnumerator Turn()
	{
		IEnumerator turnEnumerator = TurnFunc();
		while (turnEnumerator.MoveNext())
			yield return turnEnumerator.Current;
	}

	public override void PickNextAction()
	{
		if (PickNewTurn)
		{
			TurnFunc = HeatUp;
			displayAction = IconID.Fire;
		}
	}

	public IEnumerator HeatUp()
	{
		for (int i = 0; i < 10; i++)
		{
			Absorption += 2;
			intensity *= 1.01f;
			yield return new WaitForSeconds(0.1f);
			DrawColour = Color.red;
			yield return new WaitForSeconds(0.1f);
			DrawColour = TargetColour;
		}
	}
}