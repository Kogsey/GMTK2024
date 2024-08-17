using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class CardManager : MonoBehaviour, ISingleton
{
	public static List<Card> GameDeck { get; } = new List<Card>();
	public static List<Card> DiscardDeck { get; } = new List<Card>();
}