public abstract class EnemyAction
{
	public abstract EnemyActionType ActionDisplay { get; }

	public abstract void PrepAction();

	public abstract void DoAction();
}

public class GenericEnemyAction : EnemyAction
{
	public override EnemyActionType ActionDisplay { get; }
	public int Amount { get; }

	public GenericEnemyAction(EnemyActionType action, int amount)
	{
		Amount = amount;
		ActionDisplay = action;
	}

	public override void PrepAction() => throw new System.NotImplementedException();

	public override void DoAction() => throw new System.NotImplementedException();
}

public class SpecificEnemyAction : EnemyAction
{
	public override EnemyActionType ActionDisplay => EnemyActionType.Hidden;

	public override void DoAction() => throw new System.NotImplementedException();

	public override void PrepAction() => throw new System.NotImplementedException();
}

public enum EnemyActionType
{
	Attack,
	Block,
	Evade,
	Hidden,
}

public abstract class Enemy : Entity
{
	public EnemyAction NextAction { get; protected set; }

	public sealed override void RoundStart()
	{
		base.RoundStart();
		PickNextAction();
		PreTurn();
	}

	protected abstract void PickNextAction();

	/// <summary> Add block, evasion etc.. </summary>
	protected abstract void PreTurn();

	/// <summary> Attack player </summary>
	public abstract void DoTurn();

	// Start is called before the first frame update
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
	}
}