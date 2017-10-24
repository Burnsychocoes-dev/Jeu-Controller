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
	private float offset = 0.08f;
	private int frameNumber = 0;
	// Variables physiques
	public float gravity = 1f;
	private bool isCollidingUp = false; // Collision avec le haut
	private bool isCollidingDown = false; // Collision avec le sol
	private bool isCollidingRight = false; // Collision avec un mur à droite
	private bool isCollidingLeft = false; // Collision avec un mur à gauche
	private Vector2 velocity = Vector2.zero; // vitesse
	private Vector3 translate = Vector3.zero;
	bool jump = false;
	private bool isJumping = false;
	private bool doubleJump = false;
	private bool buttonJumpDown = false;
	private int buttonJumpDownCounter = 0;
	private bool collidedLeft = false;
	private bool collidedRight = false;
	private int collidedLeftCounter = 0;
	private int collidedRightCounter = 0;
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
	private int collidedDownCounter = 0;
	private bool collidingAMovingPlatform = false;
	private Vector2 downContactPlatformVelocity;
    //private float downContactPlatformVX;
	private Vector3 positionInitial;

	private RectTransform mRectTransform;
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


    public float antiTranslate = 0.0035f;

    void Start ()
	{
		mBoxCollider = GetComponent<BoxCollider2D>();
		mRectTransform = GetComponent<RectTransform>();
		positionInitial = mRectTransform.position;
		//velocity.y = -50;

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
		jump = false;
		if(buttonA || inputY > 0)
		{
			jump = true;
		}
		

		//On regarde s'il y a des collisions à venir avec la velocité souhaitée
		//InitBox();
		
			
		HandleCollisionDown();

		MultiplyVelocity();
        if (!isCollidingDown)
        {
            downContactPlatformVelocity.x = 0;
            downContactPlatformVelocity.y = 0;
        }
		//velocity.x += downContactPlatformVelocity.x;		
		//velocity.y += downContactPlatformVelocity.y;
		//il faut aussi annuler la gravité
		
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
			if (collidedRightCounter < 5)
			{
				if (Mathf.Abs(Mathf.Sin(transform.eulerAngles.z)) > 0.1)
				{
					transform.Translate(new Vector2(distanceToRightCollide * Mathf.Abs(Mathf.Sin(transform.eulerAngles.z)), velocity.y * Time.fixedDeltaTime));
					
				}
				else
				{
					transform.Translate(new Vector2(distanceToRightCollide, velocity.y * Time.fixedDeltaTime));
				}
				collidedRightCounter++;
			}
			
		}
		else
		{
			collidedRight = false;
			collidedRightCounter = 0;
		}
		if(isCollidingLeft)
		{
			collidedLeft = true;
			if (collidedLeftCounter < 5)
			{
				if (Mathf.Abs(Mathf.Sin(transform.eulerAngles.z)) > 0.1)
				{
					transform.Translate(new Vector2(-distanceToLeftCollide * Mathf.Abs(Mathf.Sin(transform.eulerAngles.z)), velocity.y * Time.fixedDeltaTime));
					
				}
				else
				{
					transform.Translate(new Vector2(-distanceToLeftCollide, velocity.y * Time.fixedDeltaTime));
				}
                collidedLeftCounter++;
            }
			
		}
		else
		{			
			collidedLeft = false;
			collidedLeftCounter = 0;
		}
		if (isCollidingUp)
		{
			transform.Translate(new Vector2(velocity.x * Time.fixedDeltaTime, distanceToUpCollide));
		}
		if (isCollidingDown)
		{
			doubleJump = false;
			isWallJumpingLeft = false;
			isWallJumpingRight = false;
			frameNumber = 0;
			buttonJumpDownCounter = 0;
            float antiGravity = 0;
            float correctionTranslate = 0;
			//Debug.Log(Mathf.Sin(transform.eulerAngles.z));
			
            if (downContactPlatformVelocity.y > 0)
            {
                antiGravity = gravity * Time.fixedDeltaTime;
            }
            if(downContactPlatformVelocity.x > 0)
            {
                correctionTranslate = antiTranslate;
            }
            if(downContactPlatformVelocity.x < 0)
            {
                correctionTranslate = -antiTranslate;
            }
            transform.Translate(translate + new Vector3(correctionTranslate, antiGravity,0));

            //correction
            if (collidedDownCounter < 2)
            {
                //transform.Translate(translate);
                if (Mathf.Abs(Mathf.Cos(transform.eulerAngles.z)) > 0.1)
                {
                    transform.Translate(new Vector2(velocity.x * Time.fixedDeltaTime, -distanceToDownCollide * Mathf.Abs(Mathf.Cos(transform.eulerAngles.z)) ));
                }
                else
                {
                    transform.Translate(new Vector2(velocity.x * Time.fixedDeltaTime, -distanceToDownCollide ));
                }
                collidedDownCounter++;
            }
            else
            {
                transform.Translate(new Vector2(velocity.x * Time.fixedDeltaTime, 0));
            }
			

		}
		else
		{
            collidedDownCounter = 0;
			collidingAMovingPlatform = false;
		}		
		if(!isCollidingDown && !isCollidingLeft && !isCollidingRight && !isCollidingUp)
		{						
			transform.Translate(velocity * Time.fixedDeltaTime);
		}		
		
		//Debug.Log("Late Update");
		//Debug.Log(doubleJump);
		InitCollisionBool();
        //Debug.Log(Time.fixedDeltaTime);
	}

	void CalculateVelocity(float inputX, float inputY, bool jump, bool buttonJumpDown)
	{
		float angle = transform.eulerAngles.z * Mathf.PI / 180;
		if (!isWallJumpingLeft && !isWallJumpingRight)
		{
			velocity.x = inputX * speed * Mathf.Cos(angle);
		}
        //si on jump
		if (inputY > 0 || jump)
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
				//Debug.Log(angle);
				velocity.y = velocity.y -  gravity;
				velocity.x = velocity.x - gravity * Mathf.Sin(angle);			
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
			velocity.y = velocity.y - gravity;
			velocity.x = velocity.x - gravity * Mathf.Sin(angle);			
		}
		//velocity.x = inputX * speed;
		//velocity += downContactPlatformVelocity;
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
		if (velocity.y <= 0 || collidingAMovingPlatform)
		{
			Vector3[] vertices = new Vector3[4];
			mRectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 up = mRectTransform.up;
			Vector2 down = new Vector2(-up[0], -up[1]);
			float angle = transform.eulerAngles.z * Mathf.PI / 180;
			boxStartPoint = new Vector2(vertices[0].x + offset * Mathf.Cos(angle), vertices[0].y + offset * Mathf.Sin(angle));
			//On initialise endPoint à droite de la box et le milieu de la box pour le y
			boxEndPoint = new Vector2(vertices[3].x - offset * Mathf.Cos(angle), vertices[3].y - offset * Mathf.Sin(angle));
			//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
			RayCollisionInfos = new RaycastHit2D[verticalRays];

			//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
			float distance = Mathf.Abs(velocity.y * Time.deltaTime + (velocity.y - gravity) * Time.fixedDeltaTime);
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
				if (RayCollisionInfos[i].collider != null && distanceToDownCollide < colliderMarge)
				{
					isCollidingDown = true;					
					RayCollisionInfos[i].collider.GetComponent<ColliderScript>().ColliderEffect();
					mRectTransform.rotation = RayCollisionInfos[i].collider.transform.rotation;
					//RayCollisionInfos[i].collider.GetComponent<ColliderScript>().ActiveMovement();
					downContactPlatformVelocity = new Vector2(RayCollisionInfos[i].collider.GetComponent<PlateformeMovingScript>().speedX,
															RayCollisionInfos[i].collider.GetComponent<PlateformeMovingScript>().speedY);
                    //downContactPlatformVX = RayCollisionInfos[i].collider.GetComponent<PlateformeMovingScript>().speedXTest;
					if(downContactPlatformVelocity.y != 0)
					{
						collidingAMovingPlatform = true;
					}
					translate = RayCollisionInfos[i].collider.GetComponent<PlateformeMovingScript>().translate;
					/*
					float platformAngle = RayCollisionInfos[i].collider.GetComponent<Transform>().eulerAngles.z;
					mRectTransform.RotateAround(
						new Vector3(distanceToDownCollide * Mathf.Sin(platformAngle), distanceToDownCollide * Mathf.Cos(platformAngle), RayCollisionInfos[i].collider.GetComponent<Transform>().position.z),
						new Vector3(0, 0, 1),
						platformAngle
						);
					*/
					//Debug.Log(Mathf.Abs(Mathf.Cos(transform.eulerAngles.z)));
					Debug.DrawRay(origin, down, Color.red);
                    //Debug.Log(new Vector2(RayCollisionInfos[i].collider.GetComponent<PlateformeMovingScript>().speedX, RayCollisionInfos[i].collider.GetComponent<PlateformeMovingScript>().speedY));
				}
			}
		}
		
	}

	void HandleCollisionUp()
	{
		if(velocity.y >= 0)
		{
			Vector3[] vertices = new Vector3[4];
			mRectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 up = mRectTransform.up;
			Vector2 up2 = new Vector2(up[0], up[1]);
			float angle = transform.eulerAngles.z * Mathf.PI/180;
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			boxStartPoint = new Vector2(vertices[1].x + offset * Mathf.Cos(angle), vertices[1].y + offset * Mathf.Sin(angle));
			//On initialise endPoint à droite de la box et le milieu de la box pour le y
			boxEndPoint = new Vector2(vertices[2].x - offset * Mathf.Cos(angle), vertices[2].y - offset * Mathf.Sin(angle));

			//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
			RayCollisionInfos = new RaycastHit2D[verticalRays];

			//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
			float distance = Mathf.Abs(velocity.y * Time.fixedDeltaTime);


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
                    RayCollisionInfos[i].collider.GetComponent<ColliderScript>().IsLava();
                    if (!RayCollisionInfos[i].collider.GetComponent<ColliderScript>().isCrossableFromDown)
					{
						isCollidingUp = true;
						mRectTransform.rotation = RayCollisionInfos[i].collider.transform.rotation;
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
			mRectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 right = mRectTransform.right;
			Vector2 right2 = new Vector2(right[0], right[1]);
			float angle = transform.eulerAngles.z * Mathf.PI / 180;
			//init de la délimitation des points entre la gauche et la droite de la box
			boxStartPoint = new Vector2(vertices[2].x + offset * Mathf.Sin(angle), vertices[2].y - offset * Mathf.Cos(angle));
			//On initialise endPoint à droite de la box et le milieu de la box pour le y
			boxEndPoint = new Vector2(vertices[3].x - offset * Mathf.Sin(angle), vertices[3].y + offset * Mathf.Cos(angle));

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
                    RayCollisionInfos[i].collider.GetComponent<ColliderScript>().IsLava();
                    isCollidingRight = true;
					mRectTransform.Rotate(-transform.eulerAngles);
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
			mRectTransform.GetWorldCorners(vertices);
			//On initialise startPoint à gauche de la box et le milieu de la box pour le y
			Vector3 right = mRectTransform.right;
			Vector2 left = new Vector2(-right[0], -right[1]);
			float angle = transform.eulerAngles.z * Mathf.PI / 180;
			//init de la délimitation des points entre la gauche et la droite de la box
			boxStartPoint = new Vector2(vertices[0].x - offset * Mathf.Sin(angle), vertices[0].y + offset * Mathf.Cos(angle));
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
                    RayCollisionInfos[i].collider.GetComponent<ColliderScript>().IsLava();
                    isCollidingLeft = true;
					mRectTransform.Rotate(-transform.eulerAngles);
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
			//transform.Translate(new Vector3(0, translate.y, 0));
			//velocity.y = downContactPlatformVelocity.y;
			//velocity.y = 0;
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

	public void Dead()
	{
		velocity = new Vector2(0, 0);
		mRectTransform.position = positionInitial;
	}

	public void Jump()
	{
		jump = true;
		velocity.y = jumpVelocity;
	}
}
