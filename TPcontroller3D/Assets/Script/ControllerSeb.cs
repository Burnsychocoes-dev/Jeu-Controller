using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSeb : MonoBehaviour {
	// Use this for initialization
	public float speed = 0.1f;
	/*
	 * Offset necessaire pour que les collisions droites et gauches ne se confondent pas
	 * avec les collisions haut et bas (cas visible sur la scene si on enleve les offsets 
	 * et qu'on teste des collisions) je l'ai reglé en live en le changeant depuis l'inspector
	 * la il sera privé
	*/
	public float offset = 0.08f;
	private int frameNumber = 0;
	// Variables physiques
	public float gravity = 1f;
	private bool isCollidingUp = false; // Collision avec le haut
	private bool isCollidingDown = false; // Collision avec le sol
	private bool isCollidingRight = false; // Collision avec un mur à droite
	private bool isCollidingLeft = false; // Collision avec un mur à gauche
	private Vector2 velocity = Vector2.zero; // vitesse
	private bool isJumping = false;
	private bool doubleJump = false;
	private bool buttonJumpDown = false;
	private int buttonJumpDownCounter = 0;
	private bool collidedLeft = false;
	private bool collidedRight = false;
	private bool isWallJumpingLeft = false;
	private bool isWallJumpingRight = false;
	private float distanceToRightCollide = 0f;
	private float distanceToLeftCollide = 0f;
	private float distanceToDownCollide = 0f;
	private float distanceToUpCollide = 0f;
	public float colliderMarge = 3f;
	public float jumpVelocity = 5f;
	private float velocityXMultiplicator = 1f;
	private float velocityYMultiplicator = 1f;

	private RectTransform rectTransform;
	// Constant donnant le nbr de trait à créer pour la collision vertical
	private static int verticalRays = 4;

	// Box collder
	private BoxCollider2D mBoxCollider;

	// Partie box
	private Rect box;
	private Vector2 boxStartPoint = Vector2.zero;
	private Vector2 boxEndPoint = Vector2.zero;

	// Conteneur d'info des collisions 
	private RaycastHit2D[] RayCollisionInfos;

	void Start ()
	{
		mBoxCollider = GetComponent<BoxCollider2D>();
		rectTransform = GetComponent<RectTransform>();
		//transform.Translate((new Vector3(1, 0, 0))*Time.deltaTime);
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	private void FixedUpdate()
	{
		bool buttonA = Input.GetButton("buttonA") || Input.GetButton("Jump");
		float inputX = Input.GetAxis("HorizontalStickGauche") + Input.GetAxis("Horizontal") + Input.GetAxis("HorizontalCroix");
		float inputY = Input.GetAxis("VerticalStickGauche") + Input.GetAxis("Vertical") + Input.GetAxis("VerticalCroix");
		//On calcule la velocité du mouvement souhaité par l'utilisateur
		if (buttonJumpDown)
		{
			buttonJumpDownCounter++;
		}
		CalculateVelocity(inputX, inputY, buttonA, buttonJumpDown);
		buttonJumpDown = Input.GetButtonDown("Jump") || Input.GetButtonDown("buttonA");
		bool jump = false;
		if(buttonA || inputY > 0)
		{
			jump = true;
			//Debug.Log("isJumping");
			//Debug.Log(doubleJump);
		}
		

		//On regarde s'il y a des collisions à venir avec la velocité souhaitée
		InitBox();
		
			
		HandleCollisionDown();

		MultiplyVelocity();

		HandleCollisionUp();
		HandleCollisionRight();
		HandleCollisionLeft();

		//Gestion de la gravité
		if(!jump || isJumping)
		{
			HandleGravity();
		}

		HandleMovement();
	}

	void LateUpdate()
	{
		
		//Debug.Log(transform.position.y);
		//Debug.Log(velocity.y);
		isJumping = !isCollidingDown;
		if(isCollidingRight)
		{
			collidedRight = true;
			transform.Translate(new Vector2(distanceToRightCollide, velocity.y * Time.deltaTime));
		}
		else if(isCollidingLeft)
		{
			collidedLeft = true; ;
			transform.Translate(new Vector2(-distanceToLeftCollide, velocity.y * Time.deltaTime));
		}
		else if (isCollidingUp)
		{
			transform.Translate(new Vector2(velocity.x * Time.deltaTime, distanceToUpCollide));
		}
		else if (isCollidingDown)
		{
			doubleJump = false;
			isWallJumpingLeft = false;
			isWallJumpingRight = false;
			frameNumber = 0;
			buttonJumpDownCounter = 0;
			transform.Translate(new Vector2(velocity.x * Time.deltaTime, -distanceToDownCollide));
		}
		else
		{
			collidedRight = false;
			collidedLeft = false;
			transform.Translate(velocity * Time.deltaTime);
		}		
		
		//Debug.Log("Late Update");
		//Debug.Log(doubleJump);
		InitCollisionBool();
	}

	void CalculateVelocity(float inputX, float inputY, bool jump, bool buttonJumpDown)
	{
		if(inputY > 0 || jump)
		{
			if(!isJumping)
			{
				velocity.y = jumpVelocity;
			}
			//wall jump left
			else if (isJumping && collidedLeft && buttonJumpDown && !isWallJumpingLeft)
			{
				Debug.Log("wall jump left !");
				velocity.y = jumpVelocity;
				velocity.x = jumpVelocity/2;
				isWallJumpingLeft = true;
				isWallJumpingRight = false;
				return;
			}
			//wall jump right
			else if (isJumping && collidedRight && buttonJumpDown && !isWallJumpingRight)
			{
				Debug.Log("wall jump right!");
				velocity.y = jumpVelocity;
				velocity.x = -jumpVelocity/2;
				isWallJumpingRight = true;
				isWallJumpingLeft = false;
				return;
			}
			//double jump
			else if(isJumping && buttonJumpDown && buttonJumpDownCounter==2 )
			{// && !doubleJump
				Debug.Log("double jump !");
				velocity.y = jumpVelocity;
				doubleJump = true;
			}
			

			else
			{
				
				velocity.y = velocity.y -  gravity;
			}
		}
		/*else if(isWallJumpingLeft && frameNumber < 3)
		{
			Debug.Log("wall jump left 2!");
			velocity.y = jumpVelocity;
			velocity.x = jumpVelocity * 2;
			frameNumber++;
			return;
		}*/
		else
		{
			
			velocity.y = velocity.y -  gravity;
		}
		if(!isWallJumpingLeft && !isWallJumpingRight)
		{
			velocity.x = inputX * speed;
		}
		//velocity.x = inputX * speed;
	}

	void HandleChangeColor()
	{
		
		if (Input.GetButton("buttonA"))
		{
			GetComponent<Renderer>().material.color = Color.green;
		}
		else if (Input.GetButton("buttonB"))
		{
			GetComponent<Renderer>().material.color = Color.red;
		}
		else if (Input.GetButton("buttonX"))
		{
			GetComponent<Renderer>().material.color = Color.blue;
		}
		else if (Input.GetButton("buttonY"))
		{
			GetComponent<Renderer>().material.color = Color.yellow;
		}
		else
		{
			GetComponent<Renderer>().material.color = Color.white;
		}

	}

	void HandleCollisionDown()
	{
		if (velocity.y <= 0)
		{
			Vector3[] vertices = new Vector3[4];
			rectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 up = rectTransform.up;
			Vector2 down = new Vector2(-up[0], -up[1]);
			float angle = transform.eulerAngles.z * Mathf.PI / 180;
			boxStartPoint = new Vector2(vertices[0].x + offset * Mathf.Cos(angle), vertices[0].y + offset * Mathf.Sin(angle));
			//On initialise endPoint à droite de la box et le milieu de la box pour le y
			boxEndPoint = new Vector2(vertices[3].x - offset * Mathf.Cos(angle), vertices[3].y + offset * Mathf.Sin(angle));
			//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
			RayCollisionInfos = new RaycastHit2D[verticalRays];

			//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
			float distance = Mathf.Abs(velocity.y * Time.deltaTime + (velocity.y - gravity) * Time.deltaTime);
			//Debug.Log("distance down");
			//Debug.Log(distance);
			for (int i = 0; i < verticalRays; i++)
			{
				//Ces petits calculs nous permettent de placer de manière proportionnelle tout les traits
				float lerpAmount = (float)i / (float)(verticalRays - 1);
				Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);
				//envoie les rayons par rapport au bas de l'objet (rotation incluse)
				//Vector2 direction = new Vector2(Mathf.Sin(transform.eulerAngles.z * Mathf.PI / 180), -Mathf.Cos(transform.eulerAngles.z * Mathf.PI / 180));
				
				//Ensuite on tire notre trait vers le bas
				RayCollisionInfos[i] = Physics2D.Raycast(origin,down, distance, 1 << LayerMask.NameToLayer("Default"));
				Debug.DrawRay(origin, down, Color.green);
				distanceToDownCollide = RayCollisionInfos[i].distance;
				//Gestion de collision d'un rayon
				if (RayCollisionInfos[i].collider != null && distanceToDownCollide<colliderMarge)
				{
					isCollidingDown = true;					
					RayCollisionInfos[i].collider.GetComponent<ColliderScript>().ColliderEffect();
					Debug.DrawRay(origin, down, Color.red);
				}
			}
		}
		
	}

	void HandleCollisionUp()
	{
		if(velocity.y > 0)
		{
			Vector3[] vertices = new Vector3[4];
			rectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 up = rectTransform.up;
			Vector2 up2 = new Vector2(up[0], up[1]);
			float angle = transform.eulerAngles.z * Mathf.PI/180;
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			boxStartPoint = new Vector2(vertices[1].x + offset * Mathf.Cos(angle), vertices[1].y + offset * Mathf.Sin(angle));
			//On initialise endPoint à droite de la box et le milieu de la box pour le y
			boxEndPoint = new Vector2(vertices[2].x - offset * Mathf.Cos(angle), vertices[2].y + offset * Mathf.Sin(angle));

			//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
			RayCollisionInfos = new RaycastHit2D[verticalRays];

			//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
			float distance = Mathf.Abs(velocity.y * Time.deltaTime);


			for (int i = 0; i < verticalRays; i++)
			{
				//Ces petits calculs nous permettent de placer de manière proportionnelle tout les traits
				float lerpAmount = (float)i / (float)(verticalRays - 1);
				Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);

				//Ensuite on tire notre trait vers le bas
				RayCollisionInfos[i] = Physics2D.Raycast(origin, up2, distance, 1 << LayerMask.NameToLayer("Default"));
				Debug.DrawRay(origin, up2, Color.green);

				distanceToUpCollide = RayCollisionInfos[i].distance;

				//Gestion de collision d'un rayon
				if (RayCollisionInfos[i].collider != null && distanceToUpCollide < colliderMarge)
				{
					if (!RayCollisionInfos[i].collider.GetComponent<ColliderScript>().isCrossableFromDown)
					{
						isCollidingUp = true;
						Debug.DrawRay(origin, up2, Color.red);
					}
					
				}
			}
		}
		
	}

	void HandleCollisionRight()
	{
		if(velocity.x >= 0)
		{
			Vector3[] vertices = new Vector3[4];
			rectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 right = rectTransform.right;
			Vector2 right2 = new Vector2(right[0], right[1]);
			float angle = transform.eulerAngles.z * Mathf.PI / 180;
			//init de la délimitation des points entre la gauche et la droite de la box
			boxStartPoint = new Vector2(vertices[2].x + offset * Mathf.Sin(angle), vertices[2].y - offset * Mathf.Cos(angle));
			//On initialise endPoint à droite de la box et le milieu de la box pour le y
			boxEndPoint = new Vector2(vertices[3].x + offset * Mathf.Sin(angle), vertices[3].y + offset * Mathf.Cos(angle));

			//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
			RayCollisionInfos = new RaycastHit2D[verticalRays];

			//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
			float distance = Mathf.Abs(velocity.x * Time.fixedDeltaTime * 2);

			for (int i = 0; i < verticalRays; i++)
			{
				//on place les traits proportionnellement
				float lerpAmount = (float)i / (float)(verticalRays - 1);
				Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);
				//On tire les traits vers la droite
				RayCollisionInfos[i] = Physics2D.Raycast(origin, right2, distance, 1 << LayerMask.NameToLayer("Default"));
				Debug.DrawRay(origin, right2, Color.green);
				distanceToRightCollide = RayCollisionInfos[i].distance;
				//if we hit sth
				if (RayCollisionInfos[i].collider != null && distanceToRightCollide < colliderMarge)
				{
					isCollidingRight = true;
					Debug.DrawRay(origin, right2, Color.red);
					if (isJumping)
					{
						ResetVelocity();
					}
				}
			}
		}
		
	}

	void HandleCollisionLeft()
	{
		if(velocity.x <= 0)
		{
			Vector3[] vertices = new Vector3[4];
			rectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 right = rectTransform.right;
			Vector2 left = new Vector2(-right[0], -right[1]);
			float angle = transform.eulerAngles.z * Mathf.PI / 180;
			//init de la délimitation des points entre la gauche et la droite de la box
			boxStartPoint = new Vector2(vertices[0].x + offset * Mathf.Sin(angle), vertices[0].y + offset * Mathf.Cos(angle));
			//On initialise endPoint à droite de la box et le milieu de la box pour le y
			boxEndPoint = new Vector2(vertices[1].x + offset * Mathf.Sin(angle), vertices[1].y - offset * Mathf.Cos(angle));

			//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
			RayCollisionInfos = new RaycastHit2D[verticalRays];

			//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
			float distance = Mathf.Abs(velocity.x * Time.fixedDeltaTime * 2);

			for (int i = 0; i < verticalRays; i++)
			{
				//on place les traits proportionnellement
				float lerpAmount = (float)i / (float)(verticalRays - 1);
				Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);
				//On tire les traits vers la droite
				RayCollisionInfos[i] = Physics2D.Raycast(origin, left, distance, 1 << LayerMask.NameToLayer("Default"));
				Debug.DrawRay(origin, left, Color.green);
				distanceToLeftCollide = RayCollisionInfos[i].distance;
				//if we hit sth
				if (RayCollisionInfos[i].collider != null && distanceToLeftCollide < colliderMarge)
				{
					isCollidingLeft = true;
					Debug.DrawRay(origin, left, Color.red);
					if (isJumping)
					{
						ResetVelocity();
					}
				}
			}
		}
		
	}

	void HandleGravity()
	{
		if(isCollidingDown)
		{
			velocity.y = 0f;
		}
	}

	void HandleMovement()
	{
		if(isCollidingUp)
		{
			if(velocity.y > 0)
			{
				velocity.y = 0f;
			}
		}
		if (isCollidingDown)
		{
			if (velocity.y < 0)
			{
				velocity.y = 0f;
			}
		}
		if (isCollidingRight)
		{
			if (velocity.x > 0)
			{
				velocity.x = 0;
			}
		}
		if (isCollidingLeft)
		{
			if (velocity.x < 0)
			{
				velocity.x = 0;
			}
		}
	}

	public static void DrawRect(Rect box, Color color, float duration)
	{
		Debug.DrawLine(new Vector2(box.min.x, box.max.y), new Vector2(box.max.x, box.max.y), color, duration);
		Debug.DrawLine(new Vector2(box.min.x, box.max.y), new Vector2(box.min.x, box.min.y), color, duration);
		Debug.DrawLine(new Vector2(box.min.x, box.min.y), new Vector2(box.max.x, box.min.y), color, duration);
		Debug.DrawLine(new Vector2(box.max.x, box.min.y), new Vector2(box.max.x, box.max.y), color, duration);
	}

	void InitBox()
	{
		box = new Rect(
			transform.position.x - mBoxCollider.size.x / 2,
			transform.position.y - mBoxCollider.size.y / 2,
			mBoxCollider.size.x,
			mBoxCollider.size.y);
	}

	void InitCollisionBool()
	{
		isCollidingUp = false;
		isCollidingDown = false; 
		isCollidingRight = false; 
		isCollidingLeft = false;
	}

	public void SetVelocityXMultiplicator(float multiplicator)
	{
		velocityXMultiplicator = multiplicator;
	}
	public void SetVelocityYMultiplicator(float multiplicator)
	{
		velocityYMultiplicator = multiplicator;
	}

	private void MultiplyVelocity()
	{
		velocity.x *= velocityXMultiplicator;
		velocity.y *= velocityYMultiplicator;
	}

	private void ResetVelocity()
	{
		velocity.x /= velocityXMultiplicator;
		velocity.y /= velocityYMultiplicator;
		velocityXMultiplicator = 1f;
		velocityYMultiplicator = 1f;
	}
}
