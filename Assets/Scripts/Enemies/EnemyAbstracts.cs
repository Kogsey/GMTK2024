using System.Collections;

public enum EnemyActionType
{
	Attack,
	Block,
	Evade,
	Hidden,
}

public abstract class Enemy : Entity
{
	public abstract EnemyActionType DisplayAction { get; }
	public abstract int DisplayActionCount { get; }

	public override IEnumerator PostTurn()
	{
		yield return PickNextAction();
	}

	public abstract IEnumerator PickNextAction();
}