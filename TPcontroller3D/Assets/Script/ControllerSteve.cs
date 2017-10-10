using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSteve : MonoBehaviour {
	// Use this for initialization
	public float speed = 0.1f;
	/*
	 * Offset necessaire pour que les collisions droites et gauches ne se confondent pas
	 * avec les collisions haut et bas (cas visible sur la scene si on enleve les offsets 
	 * et qu'on teste des collisions) je l'ai reglé en live en le changeant depuis l'inspector
	 * la il sera privé
	*/
	public float offset = 0.08f; 

	// Variables physiques
	public float gravity = 9.8f;
	public float maxGravity = 3f;
	private bool isCollidingUp = false; // Collision avec le haut
	private bool isCollidingDown = false; // Collision avec le sol
	private bool isCollidingRight = false; // Collision avec un mur à droite
	private bool isCollidingLeft = false; // Collision avec un mur à gauche
	private Vector2 velocity = Vector2.zero; // vitesse
	private bool isJumping = false;
	public float jumpVelocity = 3f;
	public float colliderDownMarge = 3f;
	private float distanceBetweenDownCrash = 0f;
	private int frameNumber = 0;
	public int maxFrameNumber = 5;
	private bool buttonJumpDown = false;

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
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	private void FixedUpdate()
	{
		bool buttonA = Input.GetButton("buttonA");
		float inputX = Input.GetAxis("HorizontalStickGauche") + Input.GetAxis("Horizontal") + Input.GetAxis("HorizontalCroix");
		float inputY = Input.GetAxis("VerticalStickGauche") + Input.GetAxis("Vertical") + Input.GetAxis("VerticalCroix");
		buttonJumpDown = Input.GetButtonDown("buttonA") || Input.GetButtonDown("VerticalStickGauche") || Input.GetButtonDown("Vertical") || Input.GetButtonDown("VerticalCroix");
		if(buttonJumpDown)
		{
			frameNumber = 1;
		}
		//On calcule la velocité du mouvement souhaité par l'utilisateur
		CalculateVelocity(inputX, inputY, buttonA);
		bool jump = false;
		if(buttonA || inputY > 0)
		{
			jump = true;
			Debug.Log("is jumping");
		}

		//On regarde s'il y a des collisions à venir avec la velocité souhaitée
		InitBox();
		HandleCollisionDown();
		HandleCollisionUp();
		HandleCollisionRight();
		HandleCollisionLeft();

		//Gestion de la gravité
		if(!jump || isJumping)
		{
			HandleGravity();
		}

		HandleMovement(inputX, inputY, buttonA);
	}

	void LateUpdate()
	{
		if(isCollidingDown && frameNumber < 1)
		{
			transform.Translate(new Vector2(velocity.x * Time.deltaTime, -distanceBetweenDownCrash));
			frameNumber = 1;
		}
		else
		{
			transform.Translate(velocity * Time.fixedDeltaTime);
		}	
		//Debug.Log(transform.position.y);
		//Debug.Log(velocity.y);
		isJumping = !isCollidingDown;
		if(isJumping && velocity.y<0)
		{
			frameNumber = 0;
		}

		if(isCollidingRight || isCollidingLeft)
		{
			isJumping = false;
		}

		InitCollisionBool();
	}

	void CalculateVelocity(float inputX, float inputY, bool jump)
	{
		if(inputY > 0 || jump)
		{
			if(!isJumping)
			{
				velocity.y = jumpVelocity;
			}
			else
			{
				velocity.y = velocity.y - gravity;
			}
		}
		else
		{
			velocity.y = velocity.y - gravity;
		}
		velocity.x = inputX * speed;
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
		//On initialise startPoint à gauche de la box et le milieu de la box pour le y
		boxStartPoint = new Vector2(box.xMin + offset, box.yMin);
		//On initialise endPoint à droite de la box et le milieu de la box pour le y
		boxEndPoint = new Vector2(box.xMax - offset, box.yMin);

		//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
		RayCollisionInfos = new RaycastHit2D[verticalRays];

		//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
		float distance = offset / 2 + Mathf.Abs(velocity.y * Time.deltaTime + (velocity.y - gravity) * Time.deltaTime);

		for (int i = 0; i < verticalRays; i++)
		{
			//Ces petits calculs nous permettent de placer de manière proportionnelle tout les traits
			float lerpAmount = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);

			//Ensuite on tire notre trait vers le bas
			RayCollisionInfos[i] = Physics2D.Raycast(origin, Vector2.down, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.down, Color.green);

			//Gestion de collision d'un rayon
			if (RayCollisionInfos[i].collider != null)
			{
				distanceBetweenDownCrash = RayCollisionInfos[i].distance;
				//Debug.Log(RayCollisionInfos[i].collider.name);
				if (distanceBetweenDownCrash < colliderDownMarge)
				{
					isCollidingDown = true;
					Debug.Log("is colliding down");
				}
				//Debug.Log(RayCollisionInfos[i].fraction);
				Debug.DrawRay(origin, Vector2.down, Color.red);
			}		
	
		}		
	}

	void HandleCollisionUp()
	{
		//On initialise startPoint à gauche de la box et le milieu de la box pour le y
		boxStartPoint = new Vector2(box.xMin + offset, box.yMax);
		//On initialise endPoint à droite de la box et le milieu de la box pour le y
		boxEndPoint = new Vector2(box.xMax - offset, box.yMax);

		//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
		RayCollisionInfos = new RaycastHit2D[verticalRays];

		//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
		float distance = offset / 2 + Mathf.Abs(velocity.y * Time.fixedDeltaTime);

		for (int i = 0; i < verticalRays; i++)
		{
			//Ces petits calculs nous permettent de placer de manière proportionnelle tout les traits
			float lerpAmount = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);

			//Ensuite on tire notre trait vers le bas
			RayCollisionInfos[i] = Physics2D.Raycast(origin, Vector2.up, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.up, Color.green);

			//Gestion de collision d'un rayon
			if (RayCollisionInfos[i].collider != null)
			{
				isCollidingUp = true;
				Debug.Log(RayCollisionInfos[i].fraction);
				Debug.DrawRay(origin, Vector2.up, Color.red);
			}
		}
	}

	void HandleCollisionRight()
	{
		//init de la délimitation des points entre la gauche et la droite de la box
		boxStartPoint = new Vector2(box.xMax - offset, box.yMin + offset);
		boxEndPoint = new Vector2(box.xMax - offset, box.yMax - offset);

		//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
		RayCollisionInfos = new RaycastHit2D[verticalRays];

		//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
		float distance = offset / 2 + Mathf.Abs(velocity.x * Time.fixedDeltaTime);

		for (int i = 0; i < verticalRays; i++)
		{
			//on place les traits proportionnellement
			float lerpAmount = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);
			//On tire les traits vers la droite
			RayCollisionInfos[i] = Physics2D.Raycast(origin, Vector2.right, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.right, Color.green);
			//if we hit sth
			if (RayCollisionInfos[i].collider != null)
			{
				isCollidingRight = true;
				Debug.DrawRay(origin, Vector2.right, Color.red);
			}
		}
	}

	void HandleCollisionLeft()
	{
		//init de la délimitation des points entre la gauche et la droite de la box
		boxStartPoint = new Vector2(box.xMin + offset, box.yMin + offset);
		boxEndPoint = new Vector2(box.xMin + offset, box.yMax - offset);

		//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
		RayCollisionInfos = new RaycastHit2D[verticalRays];

		//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
		float distance = offset / 2 + Mathf.Abs(velocity.x * Time.fixedDeltaTime);

		for (int i = 0; i < verticalRays; i++)
		{
			//on place les traits proportionnellement
			float lerpAmount = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);
			//On tire les traits vers la droite
			RayCollisionInfos[i] = Physics2D.Raycast(origin, Vector2.left, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.left, Color.green);
			//if we hit sth
			if (RayCollisionInfos[i].collider != null)
			{
				isCollidingLeft = true;
				Debug.DrawRay(origin, Vector2.left, Color.red);
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

	void HandleMovement(float inputX, float inputY, bool jump)
	{
		if (isCollidingUp)
		{
			if(velocity.y > 0)
			{
				velocity.y = 0f;
			}
		}
		if (isCollidingDown)
		{
			if(inputY > 0 || jump)
			{
				frameNumber++;
				isCollidingDown = false;
			}
			if (velocity.y < 0)
			{
				velocity.y = 0f;
			}
		}
		if (isCollidingRight)
		{
			if (velocity.x > 0)
			{
				velocity.x = 0f;
			}
		}
		if (isCollidingLeft)
		{
			if (velocity.x < 0)
			{
				velocity.x = 0f;
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
}
