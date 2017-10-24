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
    public float angularSpeed = 0f;
	public Vector3 translate;
	private float timer = 0f;
    public bool isLooping = true;
    //public float speedXTest = 0f;
	// Use this for initialization
	void Start () {
        //speedXTest = speedX * Time.fixedDeltaTime;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if(timer>= delay && active)
		{
			if (Mathf.Abs(translateX) < maxTranslateX)
			{
				translateX += speedX * Time.fixedDeltaTime;
			}
			else
			{
                if (isLooping)
                {
                    translateX = 0;
                    speedX *= -1;
                }
                else
                {
                    speedX = 0;
                }
				
                //speedXTest *= -1;
            }
			if (Mathf.Abs(translateY) < maxTranslateY)
			{
				translateY += speedY * Time.fixedDeltaTime;
			}
			else
			{
                if (isLooping)
                {
                    translateY = 0;
                    speedY *= -1;
                }
                else
                {
                    speedY = 0;
                }
				
			}
            transform.Rotate(new Vector3(0, 0, angularSpeed));
			translate = new Vector3(speedX * Time.fixedDeltaTime, speedY * Time.fixedDeltaTime, 0);
			transform.Translate(translate);
		}
		else if(active)
		{
			timer += Time.fixedDeltaTime;
		}
        //Debug.Log(Time.fixedDeltaTime);
    }
}
