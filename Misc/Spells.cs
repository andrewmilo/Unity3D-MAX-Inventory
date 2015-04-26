using UnityEngine;
using System.Collections;

public class Spells : MonoBehaviour {

	public Transform origin;
	public bool useGravity;

	public void Fireball(InventoryElement element)
	{
		Debug.Log ("SSD");
		GameObject go = Instantiate(GameObject.CreatePrimitive (PrimitiveType.Sphere), origin.position, transform.rotation) as GameObject;
		go.AddComponent<Rigidbody>();
		ParticleSystem ps = go.AddComponent<ParticleSystem>();
		ps.startColor = Color.red;

		if(useGravity)
			go.GetComponent<Rigidbody>().useGravity = true;
		else
			go.GetComponent<Rigidbody>().useGravity = false;

		go.GetComponent<Rigidbody>().velocity = origin.transform.TransformDirection(Vector3.forward * 50);
	}
}
