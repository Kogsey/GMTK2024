using UnityEngine;
using UnityEngine.SceneManagement;

public class PickCardBehave : BaseCardBehave
{
	public bool IsMainMenu;

	public enum MainMenuButton
	{
		Campaign,
		Endless,
		HowToPlay,
	}

	public MainMenuButton ButtonType;
	public int PopDistance = 10;
	private Vector2 PopupTarget => RootPosition + new Vector2(0, PopDistance);
	public static bool IgnoreNewInteract { get; set; }

	public void Awake()
	{
		RootPosition = Position;
		if (!IsMainMenu)
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
			if (!IsMainMenu)
			{
				IgnoreNewInteract = true;
				CampaignState.Instance.Deck.Add(Card);
				CampaignState.PostCardPicked();
			}
			else
			{
				switch (ButtonType)
				{
					case MainMenuButton.Campaign:
						CampaignState.StartCampaign();
						break;

					case MainMenuButton.Endless:
						CampaignState.StartEndless();
						break;

					case MainMenuButton.HowToPlay:
						SceneManager.LoadScene("HowToPlay");
						break;

					default:
						throw new System.NotImplementedException();
				}
			}
		}
	}
}