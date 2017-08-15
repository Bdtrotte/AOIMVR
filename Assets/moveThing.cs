using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveThing : MonoBehaviour {
	public float angleSpeed;
	public float radius;

	float angle;
	Rigidbody rig;

	void Awake()
	{
		rig = GetComponent<Rigidbody> ();
	}

	void FixedUpdate()
	{
		rig.velocity = new Vector3 (-radius * angleSpeed * Mathf.Sin (angle), 0, radius * angleSpeed * Mathf.Cos (angle));

		angle += Time.fixedDeltaTime;
	}
}
