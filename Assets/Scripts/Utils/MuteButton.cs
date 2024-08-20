using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
	public Sprite Unmuted;
	public Sprite Muted;
	public Image Image;

	public void ResetSprite()
		=> Image.sprite = SoundManager.Muted ? Muted : Unmuted;

	public void Start()
		=> ResetSprite();

	public void OnClick()
	{
		SoundManager.ToggleMute();
		ResetSprite();
	}
}