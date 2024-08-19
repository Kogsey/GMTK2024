using UnityEngine;

public class PickCardBehave : BaseCardBehave
{
	public int PopDistance = 10;
	private Vector2 PopupTarget => RootPosition + new Vector2(0, PopDistance);
	public static bool IgnoreNewInteract { get; set; }

	public void Awake()
	{
		RootPosition = Position;
		Card = CampaignState.Instance.PickRandomCard();
		IgnoreNewInteract = false;
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

	public void OnMouseUpAsButton()
	{
		if (!IgnoreNewInteract)
		{
			IgnoreNewInteract = true;
			CampaignState.Instance.Deck.Add(Card);
			CampaignState.PostCardPicked();
		}
	}
}