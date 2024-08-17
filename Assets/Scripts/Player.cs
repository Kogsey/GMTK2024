using Unity.VisualScripting;

[Singleton(Persistent = true)]
public class Player : Entity, ISingleton
{
	public static Player Instance { get; private set; }

	public Player() => Instance = this;
}