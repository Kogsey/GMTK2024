using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CardBehave : MonoBehaviour
{
	private CardData card;

	public Vector2 RootPosition { get; set; }

	public Vector2 Position
	{
		get => (Vector2)transform.position;
		set => transform.position = (Vector3)value;
	}

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
	public SpriteRenderer CardBackground;

	public Canvas Canvas;
	public Collider2D Collider;

	public TextMeshProUGUI Actions;
	public TextMeshProUGUI Description;
	public TextMeshProUGUI Name;

	public int CardLayer
	{
		set
		{
			//CardBackground.sortingOrder = value * 5 - 1;
			Border.sortingOrder = value * 5;
			Canvas.sortingOrder = value * 5 + 1;
		}
	}

	private void RegenCard()
	{
		Border.sprite = Singleton<CardManager>.instance.Borders[(int)card.Rarity];
		Name.text = card.Name;
		Description.text = string.Format(card.Description, card.CurrentValue);
		Actions.text = card.Energy.ToString();
	}
	public int PopDistance = 10;
	private Vector2 PopupTarget => RootPosition + new Vector2(0, PopDistance);
	public static bool IgnoreNewInteract { get; set; }

	private Coroutine MouseDragRoutine;
	public Coroutine ReturnRoutine { get; private set; }
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
				currentTarget.Highlight = false;
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
				if (currentTarget != null)
					currentTarget.Highlight = false;
				currentTarget = target;
				if (currentTarget != null)
					currentTarget.Highlight = true;
			}

			yield return null;
		}
	}

	public void StartMoveToRoot()
		=> ReturnRoutine ??= StartCoroutine(MoveToRoot());

	public IEnumerator MoveToRoot()
	{
		while (Vector2.Distance(Position, RootPosition) > 0.1f)
		{
			Position = Helpers.SmoothInterpolate(Position, RootPosition);
			yield return null;
		}
		Position = RootPosition;
		ReturnRoutine = null;
	}

	public IEnumerator PlayAndDiscard(Entity target)
	{
		Collider.enabled = false;
		Singleton<Player>.instance.SetNextCard(card, target);
		Singleton<CardManager>.instance.PlayedCard(this);
		Destroy(gameObject);
		yield return null;
	}

	public float DropRange;

	public bool DropTarget([NotNullWhen(true)] out Entity target)
	{
		float bestDistance = DropRange;
		target = null;

		foreach (Entity entity in Singleton<GameManager>.instance.AllCardTargets)
		{
			if (card.CanPlayCard(entity))
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