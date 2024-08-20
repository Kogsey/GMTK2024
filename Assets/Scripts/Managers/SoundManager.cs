using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance { get; private set; }

	public AudioSource SoundPlayer;
	public AudioClip[] CardShuffles;
	public AudioClip LoseSound;
	public AudioClip WinGameSound;
	public AudioClip WinSound;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
	}

	/// <returns> If the sound source is muted </returns>
	public static bool ToggleMute()
	{
		Instance.SoundPlayer.mute = !Instance.SoundPlayer.mute;
		return Instance.SoundPlayer.mute;
	}

	public static bool Muted
		=> Instance.SoundPlayer.mute;

	public static void PlayCardShuffle()
		=> Instance.SoundPlayer.PlayOneShot(Instance.CardShuffles[Random.Range(0, Instance.CardShuffles.Length)]);

	public static void PlayWinSound()
		=> Instance.SoundPlayer.PlayOneShot(Instance.WinSound);

	public static void PlayLoseSound()
		=> Instance.SoundPlayer.PlayOneShot(Instance.LoseSound);

	public static void PlayWinGameSound()
		=> Instance.SoundPlayer.PlayOneShot(Instance.WinGameSound);
}