using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateformeMovingScript : MonoBehaviour {
	public bool active = true;
	public float delay = 0f;
	public float speedX = 0f;
	public float speedY = 0f;
	public float maxTranslateX = 0f;
	public float maxTranslateY = 0f;
	public float translateX = 0f;
	public float translateY = 0f;
	private float timer = 0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(timer>= delay && active)
		{
			if (Mathf.Abs(translateX) < maxTranslateX)
			{
				translateX += speedX * Time.deltaTime;
			}
			else
			{
				translateX = 0;
				speedX *= -1;
			}
			if (Mathf.Abs(translateY) < maxTranslateY)
			{
				translateY += speedY * Time.deltaTime;
			}
			else
			{
				translateY = 0;
				speedY *= -1;
			}

			transform.Translate(speedX * Time.deltaTime, speedY * Time.deltaTime, 0);
		}
		else
		{
			timer += Time.deltaTime;
		}
		
	}
}
