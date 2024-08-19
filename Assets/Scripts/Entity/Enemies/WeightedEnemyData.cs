using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultEnemy", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class WeightedEnemyData : ScriptableObject
{
	public bool Tall;
	public int MinHealth;
	public int MaxHealth;
	public int StartBlock;
	public int StartAbsorb;
	public EnemyGeneration GenerationData;
	public AnimationEffect AnimationEffects;
	public Sprite Sprite;
	public List<GenericEnemyAction> ActionPool;

	public int MinLevel;
}