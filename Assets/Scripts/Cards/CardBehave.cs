using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseCardBehave : MonoBehaviour
{
	public Vector2 RootPosition { get; set; }
	public Vector2 RootScale { get; set; } = Vector2.one;

	public Vector2 Position
	{
		get => (Vector2)transform.position;
		set => transform.position = (Vector3)value;
	}

	public Vector2 Scale
	{
		get => (Vector2)transform.localScale;
		set => transform.localScale = (Vector3)value;
	}

	private CardData card;

	public CardData Card
	{
		get => card;
		set
		{
			card = value;
			RegenCard();
		}
	}

	public SpriteRenderer Border;
	public SpriteRenderer CardBack;
	public SpriteRenderer CardArt;

	public Canvas Canvas;
	public Collider2D Collider;

	public TextMeshProUGUI Actions;
	public TextMeshProUGUI Description;
	public TextMeshProUGUI Name;

	public int CardLayer
	{
		set
		{
			CardArt.sortingOrder = value * 10 - 1;
			Border.sortingOrder = value * 10;
			Canvas.sortingOrder = value * 10 + 1;
		}
	}

	private void RegenCard()
	{
		Border.sprite = SpriteBank.Instance.GetBorder(Card.Rarity);
		CardArt.sprite = Card.CardArt;
		Name.text = Card.Name;
		Description.text = string.Format(card.Description, Card.CurrentValue);
		Actions.text = Card.Energy.ToString();
	}

	public Coroutine ReturnRoutine { get; protected set; }

	public void StartMoveToRoot()
		=> ReturnRoutine ??= StartCoroutine(MoveToRoot());

	public virtual void ForceMoveToRoot()
		=> StartMoveToRoot();

	public IEnumerator MoveToRoot()
	{
		while (Vector2.Distance(Position, RootPosition) > 0.1f)
		{
			Position = Helpers.SmoothInterpolate(Position, RootPosition);
			Scale = Helpers.SmoothInterpolate(Scale, RootScale);
			yield return null;
		}
		Position = RootPosition;
		Scale = RootScale;
		ReturnRoutine = null;
	}
}

public class CardBehave : BaseCardBehave
{
	public int PopDistance = 10;
	private Vector2 PopupTarget => RootPosition + new Vector2(0, PopDistance);
	public static bool IgnoreNewInteract { get; set; }

	private Coroutine MouseDragRoutine;

	public override void ForceMoveToRoot()
	{
		if (MouseDragRoutine != null)
		{
			StopCoroutine(MouseDragRoutine);
			MouseDragRoutine = null;
		}
		ResetTargetColour();
		base.ForceMoveToRoot();
	}

	public void OnMouseExit()
	{
		if (!IgnoreNewInteract)
			StartMoveToRoot();
	}

	public void OnMouseOver()
	{
		if (!IgnoreNewInteract)
		{
			if (ReturnRoutine != null)
			{
				StopCoroutine(ReturnRoutine);
				ReturnRoutine = null;
			}
			Position = Helpers.SmoothInterpolate(Position, PopupTarget);
		}
	}

	public void OnMouseDown()
	{
		if (!IgnoreNewInteract)
		{
			MouseDragRoutine = StartCoroutine(DragRoutine(Helpers.WorldMousePosition - (Position - RootPosition)));
			IgnoreNewInteract = true;
		}
	}

	public void OnMouseUp()
	{
		if (MouseDragRoutine != null)
		{
			StopCoroutine(MouseDragRoutine);
			IgnoreNewInteract = false;
			MouseDragRoutine = null;
			if (currentTarget != null)
			{
				ResetTargetColour();
				StartCoroutine(PlayAndDiscard(currentTarget));
			}
			else
				StartMoveToRoot();
		}
	}

	private Entity currentTarget;

	public IEnumerator DragRoutine(Vector2 startMousePos)
	{
		while (true)
		{
			Vector2 targetOffset = Helpers.WorldMousePosition - startMousePos;
			Position = Helpers.SmoothInterpolate(Position, RootPosition + targetOffset);

			DropTarget(out Entity target);
			if (currentTarget != target)
			{
				ResetTargetColour();
				currentTarget = target;
				if (currentTarget != null)
					currentTarget.Highlight = true;
			}

			yield return null;
		}
	}

	private void ResetTargetColour()
	{
		if (currentTarget != null)
			currentTarget.Highlight = false;
	}

	public IEnumerator PlayAndDiscard(Entity target)
	{
		Collider.enabled = false;
		Singleton<Player>.instance.SetNextCard(Card, target);
		Singleton<CardManager>.instance.PlayedCard(this);
		yield return ReturnRoutine;
		Destroy(gameObject);
		yield return null;
	}

	public float DropRange;

	public bool DropTarget([NotNullWhen(true)] out Entity target)
	{
		float bestDistance = DropRange;
		target = null;

		foreach (Entity entity in Singleton<LevelManager>.instance.AllCardTargets)
		{
			if (Card.CanPlayCard(entity))
			{
				float myDistance = Vector2.Distance(entity.transform.position, transform.position);
				if (myDistance < bestDistance)
				{
					bestDistance = myDistance;
					target = entity;
				}
			}
		}

		return target != null;
	}
}