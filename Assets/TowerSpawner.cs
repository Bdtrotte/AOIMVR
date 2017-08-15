using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour 
{
	public GameObject tower;
	public Transform parent;
	public int howMany;
	public float maxWidth;
	public float maxHeight;
	public float exclusionRadius;

	void Start () 
	{
		for (int i = 0; i < howMany; ++i) {
			Vector3 pos = new Vector3 ((Random.value - 0.5f) * maxWidth, 0, (Random.value - 0.5f) * maxHeight);
			if (Vector3.Distance(transform.position, pos) < exclusionRadius) {
				--i;
				continue;
			}
			Instantiate (tower, pos, Quaternion.identity, parent);
		}
	}
}
