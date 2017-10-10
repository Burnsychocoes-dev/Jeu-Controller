using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider : MonoBehaviour {
	public enum ColliderType
	{
		SLOWING,
		ICE,
		BOUNCING,
		WATER,
		
	}

	[SerializeField]
	private ColliderType colliderType;
	
	public void ColliderEffect(float velocityX, float velocityY, float jumpVelocity =0)
	{
		switch (colliderType)
		{
			case ColliderType.SLOWING:
				velocityX*=0.66f;
				break;
			case ColliderType.ICE:
				velocityX *= 1.5f;
				break;
			case ColliderType.BOUNCING:
				velocityY = jumpVelocity;
				break;
			case ColliderType.WATER:
				velocityX *= 0.66f;
				velocityY *= 0.66f;
				break;
		}
			

	}
}
