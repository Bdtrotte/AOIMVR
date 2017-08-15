using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class justascript : MonoBehaviour
{
	public Rigidbody r;
	public Transform t;
	public Transform player;
	public Transform feet;
	public Transform hookPoint;
	public Transform head;
	public LineRenderer line;
	public ParticleSystem ps;
	public bool isRight;

	SteamVR_Controller.Device d;

	bool hasFired = false;
	float ropeLength = 0;

	void Start ()
	{
		d = SteamVR_Controller.Input ((int)GetComponent<SteamVR_TrackedObject> ().index);
		line.enabled = false;
	}

	void FixedUpdate ()
	{
		if (d.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip)) {
			float force = 2000f;
			r.AddForce (t.up * force);
			if (ps.isStopped)
				ps.Play ();
		} else {
			if (ps.isPlaying)
				ps.Stop ();
		}

		if (hasFired) {
			line.SetPosition (0, HipPoint());
			line.SetPosition (1, hookPoint.position);

			if (d.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
				hasFired = false;
				line.enabled = false;
			} else {
				float f = d.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x * 4000;
				r.AddForce ((hookPoint.position - feet.position).normalized * f);
				Rigidbody rig = hookPoint.parent.gameObject.GetComponent<Rigidbody> ();
				if (rig != null)
					rig.AddForce ((HipPoint() - hookPoint.position).normalized * f);

				Vector3 hipPoint = HipPoint ();
				float distance = Vector3.Distance (hipPoint, hookPoint.position);

				if (distance > ropeLength) {
					Vector3 hookToHipDir = (hipPoint - hookPoint.position).normalized;

					if (rig == null) {
						Vector3 newVelocity = r.velocity - Vector3.Dot (r.velocity, hookToHipDir) * hookToHipDir - hookToHipDir * (distance - ropeLength);
						r.velocity = newVelocity;
					} else {
						//Modify the other position and velocity to swing around this.
						Vector3 newOtherVelocity = rig.velocity;
						float otherAwaySpeed = Vector3.Dot (r.velocity, -hookToHipDir);
						if (otherAwaySpeed > 0)
							newOtherVelocity += otherAwaySpeed * hookToHipDir;
						newOtherVelocity += hookToHipDir * (distance - ropeLength);

						rig.velocity = newOtherVelocity;

						Vector3 crossProduct = Vector3.Cross (hookPoint.position - rig.transform.position, hookToHipDir * (distance - ropeLength));
						rig.angularVelocity = crossProduct*50;

						Vector3 desiredPos = -hookToHipDir * ropeLength + hipPoint;
						Vector3 hookToOther = rig.transform.position - hookPoint.position;

						rig.transform.position = desiredPos + hookToOther;
					}
				} else if (f != 0) {
					ropeLength = distance;
				}
			}
		} else {
			if (d.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
				RaycastHit hit;
				if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, ~(1 << 8))) {
					if (Physics.Raycast (HipPoint(), hit.point - HipPoint(), out hit, Mathf.Infinity, ~(1 << 8))) {
						hookPoint.position = hit.point;
						hookPoint.SetParent (hit.collider.transform);

						ropeLength = Vector3.Distance (HipPoint (), hookPoint.position);

						line.enabled = true;
						line.SetPosition (0, HipPoint());
						line.SetPosition (1, hookPoint.position);

						hasFired = true;
					}
				}
			}
		}
	}

	Vector3 HipPoint()
	{
		if (isRight)
			return RightHipPoint ();
		else
			return LeftHipPoint ();
	}

	Vector3 RightHipPoint()
	{
		return feet.position
		+ new Vector3 (0, 1, 0)
		- new Vector3 (-head.forward.z, 0, head.forward.x).normalized * 0.5f;
	}

	Vector3 LeftHipPoint()
	{
		return feet.position
		+ new Vector3 (0, 1, 0)
		+ new Vector3 (-head.forward.z, 0, head.forward.x).normalized * 0.5f;
	}
}
