using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderScript : MonoBehaviour {
	public enum ColliderType
	{
		NORMAL,
		SLOWING,
		ICE,
		BOUNCING,
		WATER,
		
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
				seb.SetVelocityXMultiplicator(0.66f);
				break;
			case ColliderType.ICE:
				seb.SetVelocityXMultiplicator(3f);
				break;
			case ColliderType.BOUNCING:
				
				break;
			case ColliderType.WATER:
				seb.SetVelocityXMultiplicator(0.66f);
				seb.SetVelocityYMultiplicator(0.66f);
				break;
		}
			

	}
}
