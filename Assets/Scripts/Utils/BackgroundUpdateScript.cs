using UnityEngine;

public class BackgroundUpdateScript : MonoBehaviour
{
	public bool Dark;

	public void Start()
	{
		if (Dark)
			SpriteBank.Instance.SetBackgroundColourDark();
		else
			SpriteBank.Instance.SetBackgroundColourLight();
	}
}