using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
	// Use this for initialization
	public float speed = 0.1f;

	void Start () {
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	void OnCollisionEnter(Collision collision)
	{
		Destroy(collision.gameObject);
	}

		private void FixedUpdate()
	{
		float inputXStickGauche = Input.GetAxis("HorizontalStickGauche");
		float inputYStickGauche = Input.GetAxis("VerticalStickGauche");
		float inputXCroix = Input.GetAxis("HorizontalCroix");
		float inputYCroix = Input.GetAxis("VerticalCroix");

		if (inputXStickGauche!=0 || inputYStickGauche!=0)
		
			HandleDeplacement(inputXStickGauche, inputYStickGauche);
		
		if (inputXCroix!=0 || inputYCroix!=0)
		
			HandleDeplacement(inputXCroix, inputYCroix);
		
		HandleChangeColor();
	}

	void HandleDeplacement(float inputX, float inputY)
	{
		transform.Translate(new Vector2(inputX*speed, inputY*speed));
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
}
