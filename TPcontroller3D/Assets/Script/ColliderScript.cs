using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderScript : MonoBehaviour {
	public bool isCrossableFromDown = false;
	public float slowMultiplicator = 0.66f;
	public float iceMultiplicator = 3f;
	//public float bouncingMultiplicator = 1f;
	public float waterMultiplicator = 0.66f;
	public enum ColliderType
	{
		NORMAL,
		SLOWING,
		ICE,
		BOUNCING,
		WATER,
		LAVA
		
	}

	[SerializeField]
	private ColliderType colliderType;
	
	public void ColliderEffect()
	{
		ControllerSeb seb = GameObject.Find("Seb").GetComponent<ControllerSeb>();
		
		switch (colliderType)
		{
			case ColliderType.NORMAL:
				seb.SetVelocityXMultiplicator(1f);
				seb.SetVelocityYMultiplicator(1f);
				break;
			case ColliderType.SLOWING:
				seb.SetVelocityXMultiplicator(slowMultiplicator);
				break;
			case ColliderType.ICE:
				seb.SetVelocityXMultiplicator(iceMultiplicator);
				break;
			case ColliderType.BOUNCING:
				//Activer le jump du joueur
				seb.Jump();
				break;
			case ColliderType.WATER:
				seb.SetVelocityXMultiplicator(waterMultiplicator);
				seb.SetVelocityYMultiplicator(waterMultiplicator);
				break;
			case ColliderType.LAVA:
				//Mettre l'état mort au joueur -> on remet sa vitesse à 0 et on le tp aux coordonnées du début
				seb.Dead();
				break;
		}
			

	}
}
