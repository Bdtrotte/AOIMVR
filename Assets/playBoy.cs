using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playBoy : MonoBehaviour {
	public Transform head;
	public Transform feet;

	public Transform LeftHipPoint;
	public Transform RightHipPoint;

	public float hipHeight;
	public float hipDistance;

	void Update()
	{
		CapsuleCollider c = GetComponent<CapsuleCollider> ();

		Vector3 relativePoint = head.position - transform.position;

		RaycastHit hit;
		if (Physics.Raycast(head.position, Vector3.down, out hit, 2.1f, ~(1 << 8))) {
			float height = head.position.y - hit.point.y;
			feet.position = hit.point;
			c.center = new Vector3(relativePoint.x, height / 2, relativePoint.z);
			c.height = height;
		} else {
			feet.position = head.position - new Vector3 (0, relativePoint.y, 0);
			c.center = new Vector3 (relativePoint.x, c.center.y, relativePoint.z);
		}

		updateHips();
	}

	void updateHips()
	{
		Vector3 right = new Vector3 (head.right.x, 0, head.right.z);
		right.Normalize ();

		LeftHipPoint.position = -right * hipDistance + feet.position + Vector3.up*hipHeight;
		RightHipPoint.position = right * hipDistance + feet.position + Vector3.up*hipHeight;
	}
}
