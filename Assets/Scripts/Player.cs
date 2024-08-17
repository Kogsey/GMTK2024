using Unity.VisualScripting;

[Singleton(Automatic = true, Persistent = true)]
public class Player : Entity
{
	public static Player Instance { get; private set; }

	public Player() => Instance = this;

	// Start is called before the first frame update
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
	}
}