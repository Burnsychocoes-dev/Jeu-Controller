using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSeb : MonoBehaviour {
	// Use this for initialization

	//speed multiplicator
	public float speed = 0.1f;

	//constante de chute
	public float gravity = 3f;
	//vitesse max de chute
	public float maxGravity = 15f;

	//pour appliquer des forces au personnage
	private Vector2 velocity = Vector2.zero;

	//collision down
	//Si on touche le sol
	private bool grounded = false;
	private bool collideSide = false;
	private bool collideUp = false;
	private static int verticalRays = 4;

	private BoxCollider2D boxCollider;
	private Rect box;

	//Délimitations de box
	private Vector2 startPointDown = Vector2.zero;
	private Vector2 endPointDown = Vector2.zero;

	//retour de Raycast sur les infos de collision
	private RaycastHit2D[] hitInfosDown;


	void Start () {
		boxCollider = GetComponent<BoxCollider2D>();
		InitBox();
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}


	private void FixedUpdate()
	{
		float inputXStickGauche = Input.GetAxis("HorizontalStickGauche");
		float inputYStickGauche = Input.GetAxis("VerticalStickGauche");
		float inputXCroix = Input.GetAxis("HorizontalCroix");
		float inputYCroix = Input.GetAxis("VerticalCroix");
		float inputX = Input.GetAxis("Horizontal");
		float inputY = Input.GetAxis("Vertical");

		if (inputXStickGauche!=0 || inputYStickGauche!=0)
		
			HandleDeplacement(inputXStickGauche, inputYStickGauche);
		
		if (inputXCroix!=0 || inputYCroix!=0)
		
			HandleDeplacement(inputXCroix, inputYCroix);

		if (inputX != 0 || inputY != 0)
			HandleDeplacement(inputX, inputY);
		
		HandleChangeColor();

		//init de la box de collision
		InitBox();

		//gestion de la collision en bas
		HandleCollisionDown();
		HandleCollisionSide();

		if (grounded)
		{
			//on enlève la gravité
			velocity = new Vector2(velocity.x, 0);
			grounded = false;
		}
		else {
			// on ajoute la gravité à la velocité
			velocity = new Vector2(velocity.x, Mathf.Max(velocity.y - gravity, -maxGravity));
			
		}
		if (collideSide)
		{
			velocity = new Vector2(0, velocity.y);
			collideSide = false;

		}
		if (Input.GetButtonDown("Jump"))
		{
			velocity = new Vector2(velocity.x, speed*10);
			HandleCollisionUp();

			if (collideUp)
			{
				velocity = new Vector2(velocity.x,  - gravity);
				collideUp = false;
			}
			else
			{
				//velocity = new Vector2(velocity.x, Mathf.Max(velocity.y - gravity, -maxGravity));
			}
		}

		
		
	}

	void LateUpdate()
	{
		transform.Translate(velocity * Time.deltaTime);
	}


	void HandleDeplacement(float inputX, float inputY)
	{
		velocity = new Vector2(inputX*speed, inputY*speed);
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

	void InitBox()
	{
		box = new Rect(
			transform.position.x - boxCollider.size.x / 2,
			transform.position.y - boxCollider.size.y / 2,
			boxCollider.size.x,
			boxCollider.size.y);
	}

	void HandleCollisionDown()
	{
		//init de la délimitation des points entre la gauche et la droite de la box
		startPointDown = new Vector2(box.xMin, box.center.y);
		endPointDown = new Vector2(box.xMax, box.center.y);

		hitInfosDown = new RaycastHit2D[verticalRays];

		float distance = box.height / 2 + Mathf.Abs(velocity.y * Time.deltaTime);

		for(int i=0; i < verticalRays; i++)
		{
			//on place les traits proportionnellement
			float lerpAmount  = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(startPointDown, endPointDown, lerpAmount);

			//On tire les traits vers le bas
			hitInfosDown[i] = Physics2D.Raycast(origin, Vector2.down, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.down, Color.green);

			//if we hit sth
			if (hitInfosDown[i].fraction > 0)
			{
				grounded = true;
				Debug.DrawRay(origin, Vector2.down, Color.red);
				Debug.Log("touchéDown");
			}
		}
	}

	void HandleCollisionSide()
	{
		//init de la délimitation des points entre la gauche et la droite de la box
		startPointDown = new Vector2(box.center.x, box.yMin);
		endPointDown = new Vector2(box.center.x, box.yMax);

		hitInfosDown = new RaycastHit2D[verticalRays];

		float distance = box.width / 2 + Mathf.Abs(velocity.x * Time.deltaTime);

		for (int i = 0; i < verticalRays; i++)
		{
			//on place les traits proportionnellement
			float lerpAmount = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(startPointDown, endPointDown, lerpAmount);
			
			
			//On tire les traits vers la droite
			hitInfosDown[i] = Physics2D.Raycast(origin, Vector2.right, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.right, Color.green);
			//if we hit sth
			if (hitInfosDown[i].fraction > 0)
			{
				collideSide = true;
				Debug.DrawRay(origin, Vector2.right, Color.red);
				Debug.Log("touchéRight");
			}
			
			
			
			//On tire les traits vers la gauche
			hitInfosDown[i] = Physics2D.Raycast(origin, Vector2.left, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.left, Color.green);
			//if we hit sth
			if (hitInfosDown[i].fraction > 0)
			{
				collideSide = true;
				Debug.DrawRay(origin, Vector2.left, Color.red);
				Debug.Log("touchéLeft");
			}
			

			
		}
	}

	void HandleCollisionUp()
	{
		//init de la délimitation des points entre la gauche et la droite de la box
		startPointDown = new Vector2(box.xMin, box.center.y);
		endPointDown = new Vector2(box.xMax, box.center.y);

		hitInfosDown = new RaycastHit2D[verticalRays];

		float distance = box.height / 2 + Mathf.Abs(velocity.y * Time.deltaTime);

		for (int i = 0; i < verticalRays; i++)
		{
			//on place les traits proportionnellement
			float lerpAmount = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(startPointDown, endPointDown, lerpAmount);

			//On tire les traits vers le bas
			hitInfosDown[i] = Physics2D.Raycast(origin, Vector2.up, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.up, Color.green);

			//if we hit sth
			if (hitInfosDown[i].fraction > 0)
			{
				collideUp = true;
				Debug.DrawRay(origin, Vector2.up, Color.red);
				Debug.Log("touchéUp");
			}
		}
	}

}
