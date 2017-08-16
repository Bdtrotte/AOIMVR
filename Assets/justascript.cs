using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class justascript : MonoBehaviour
{
	public Rigidbody r;
	public Transform t;
	public Transform player;
	public Transform feet;
	public Transform head;
	public Transform hipPoint;
	public LineRenderer line;
	public ParticleSystem ps;

	SteamVR_Controller.Device d;

	bool hasFired = false;
	float ropeLength = 0;

	List<Transform> linePoints;
	Vector3 prePos;

	void Start ()
	{
		linePoints = new List<Transform> ();
		prePos = hipPoint.position;

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
			updateLine ();
			drawLine ();

			if (d.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
				hasFired = false;
				line.enabled = false;
			} else {
				float f = d.GetAxis (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x * 4000;
				r.AddForce ((linePoints[linePoints.Count-1].position - feet.position).normalized * f);
				Rigidbody rig = linePoints[linePoints.Count-1].parent.gameObject.GetComponent<Rigidbody> ();
				if (rig != null)
					rig.AddForce ((hipPoint.position - linePoints[linePoints.Count-1].position).normalized * f);

				float distance = TotalDistance ();

				if (distance > ropeLength) {
					Vector3 hookToHipDir = (hipPoint.position - linePoints[linePoints.Count-1].position).normalized;

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

						Vector3 crossProduct = Vector3.Cross (linePoints[linePoints.Count-1].position - rig.transform.position, hookToHipDir * (distance - ropeLength));
						rig.angularVelocity = crossProduct*50;

						Vector3 desiredPos = -hookToHipDir * ropeLength + hipPoint.position;
						Vector3 hookToOther = rig.transform.position - linePoints[linePoints.Count-1].position;

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
					if (Physics.Raycast (hipPoint.position, hit.point - hipPoint.position, out hit, Mathf.Infinity, ~(1 << 8))) {
						Transform hookPoint = new GameObject ("HookPoint").transform;
						hookPoint.position = hit.point + (hipPoint.position - hit.point).normalized*0.1f;
						hookPoint.SetParent (hit.collider.transform);

						ropeLength = Vector3.Distance (hipPoint.position, hookPoint.position);

						line.enabled = true;
						clearLine ();
						linePoints.Add (hookPoint);
						drawLine ();

						hasFired = true;
					}
				}
			}
		}

		prePos = hipPoint.position;
	}

	void drawLine()
	{
		line.positionCount = linePoints.Count + 1;

		int i = 0;
		foreach(Transform t in linePoints)
			line.SetPosition (i++, t.position);

		line.SetPosition (linePoints.Count, hipPoint.position);
	}

	void clearLine()
	{
		foreach (Transform t in linePoints)
			Destroy (t.gameObject);

		linePoints.Clear ();
	}

	void updateLine()
	{
		if (linePoints.Count > 1) {
			Vector3 aroundCornerPoint = linePoints[linePoints.Count - 2].position;
			if (!Physics.Raycast(hipPoint.position, aroundCornerPoint - hipPoint.position, Vector3.Distance(aroundCornerPoint, hipPoint.position), ~(1<<8))) {
				Destroy(linePoints[linePoints.Count - 1].gameObject);
				linePoints.RemoveAt(linePoints.Count - 1);
				return;
			}
		}

		if (linePoints [0].GetComponentInParent<Rigidbody> () == null) {
			Vector3 preHookPoint = linePoints [linePoints.Count - 1].position;
			RaycastHit hit;
			if (Physics.Raycast (hipPoint.position, preHookPoint - hipPoint.position, out hit, Vector3.Distance (hipPoint.position, preHookPoint), ~(1 << 8))) {
				Vector3 newHookPos = hit.point + prePos - hipPoint.position;
				Transform newHookPoint = new GameObject ("hook point").transform;
				newHookPoint.position = newHookPos;
				newHookPoint.SetParent (hit.collider.transform);
				linePoints.Add (newHookPoint);
			}
		}
	}

	float TotalDistance()
	{
		float d = 0;
		Vector3 prePos = linePoints [0].position;

		for (int i = 1; i < linePoints.Count; ++i) {
			d += Vector3.Distance (prePos, linePoints [i].position);
			prePos = linePoints [i].position;
		}

		return d + Vector3.Distance (prePos, hipPoint.position);
	}
}
