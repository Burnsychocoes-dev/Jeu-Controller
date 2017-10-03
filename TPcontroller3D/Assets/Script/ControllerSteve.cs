using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSteve : MonoBehaviour {
	// Use this for initialization
	public float speed = 0.1f;

	// Variables physiques
	private float gravity = 1f; 
	private float maxGravity = 3f;
	private bool grounded = false; // Collision avec le sol
	private Vector2 velocity = Vector2.zero; // vitesse

	// Constant donnant le nbr de trait à créer pour la collision vertical
	private static int verticalRays = 4;

	// Box collder
	private BoxCollider2D mBoxCollider;

	// Partie box
	private Rect box;
	private Vector2 boxStartPoint = Vector2.zero;
	private Vector2 boxEndPoint = Vector2.zero;

	// Conteneur d'info des collisions 
	private RaycastHit2D[] RayCollisionInfo;

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
		HandleDeplacement();

		HandleChangeColor();

		//On regarde si il y a des collisions à venir
		HandleCollision();

		//Gestion de la gravité
		HandleGravity();
	}

	void LateUpdate()
	{
		transform.Translate(velocity * Time.deltaTime);
	}

	void HandleDeplacement()
	{
		float inputXStickGauche = Input.GetAxis("HorizontalStickGauche");
		float inputYStickGauche = Input.GetAxis("VerticalStickGauche");
		float inputXCroix = Input.GetAxis("HorizontalCroix");
		float inputYCroix = Input.GetAxis("VerticalCroix");
		float inputX = Input.GetAxis("Horizontal");
		float inputY = Input.GetAxis("Vertical");

		if (inputXStickGauche != 0 || inputYStickGauche != 0)
			transform.Translate(new Vector2(inputXStickGauche * speed, inputYStickGauche * speed));

		else if (inputXCroix != 0 || inputYCroix != 0)
			transform.Translate(new Vector2(inputXCroix * speed, inputYCroix * speed));

		else if (inputX != 0 || inputY != 0)
			transform.Translate(new Vector2(inputX * speed, inputY * speed));
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

	void HandleCollision()
	{
		//Chaque frame on initialise la boîte
		box = new Rect(
			transform.position.x - mBoxCollider.size.x / 2,
			transform.position.y - mBoxCollider.size.y / 2,
			mBoxCollider.size.x,
			mBoxCollider.size.y
		);
		DrawRect (box, Color.white, Time.deltaTime);

		//On initialise startPoint à gauche de la box et le milieu de la box pour le y
		boxStartPoint = new Vector2(box.xMin, box.center.y);
		//On initialise endPoint à droite de la box et le milieu de la box pour le y
		boxEndPoint = new Vector2(box.xMax, box.center.y);

		//On initialise le tableau des infos de collision au nombre de traits souhaités vers le bas
		RayCollisionInfo = new RaycastHit2D[verticalRays];

		//On prends notre distance à parcourir. En l'occurence la motié de la box + la distance parcourue avant la dernière frame
		float distance = box.height / 2 + Mathf.Abs(velocity.y * Time.deltaTime);

		for (int i = 0; i < verticalRays; i++)
		{
			//Ces petits calculs nous permettent de placer de manière proportionnelle tout les traits
			float lerpAmount = (float)i / (float)(verticalRays - 1);
			Vector2 origin = Vector2.Lerp(boxStartPoint, boxEndPoint, lerpAmount);

			//Ensuite on tire notre trait vers le bas
			RayCollisionInfo[i] = Physics2D.Raycast(origin, Vector2.down, distance, 1 << LayerMask.NameToLayer("Default"));
			Debug.DrawRay(origin, Vector2.down, Color.green);

			//Gestion de collision d'un rayon
			if (RayCollisionInfo[i].collider != null)
			{
				grounded = true;
				Debug.DrawRay(origin, Vector2.down, Color.red);
			}		
		}
	}

	void HandleGravity()
	{
		if (grounded)
		{
			//On enlève la gravité
			velocity = new Vector2(velocity.x, 0);
			grounded = false;
		}
		else
		{
			//On ajoute la gravité à notre vélocité
			velocity = new Vector2(velocity.x, Mathf.Max(velocity.y - gravity, -maxGravity));
		}
	}

	public static void DrawRect(Rect box, Color color, float duration)
	{
		Debug.DrawLine(new Vector2(box.min.x, box.max.y), new Vector2(box.max.x, box.max.y), color, duration);
		Debug.DrawLine(new Vector2(box.min.x, box.max.y), new Vector2(box.min.x, box.min.y), color, duration);
		Debug.DrawLine(new Vector2(box.min.x, box.min.y), new Vector2(box.max.x, box.min.y), color, duration);
		Debug.DrawLine(new Vector2(box.max.x, box.min.y), new Vector2(box.max.x, box.max.y), color, duration);
	}
}
