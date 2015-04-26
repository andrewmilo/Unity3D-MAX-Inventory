using UnityEngine;
using System.Collections;

public class CharacterActions : MonoBehaviour {

	private GameObject wieldedGameObject;
	public float distance = 10;
	public float goDepth = 4;

	// Update is called once per frame
	void Update () {
		if(wieldedGameObject != null)
		{
			wieldedGameObject.transform.position = transform.position + Camera.main.transform.forward * 1.5f + Camera.main.transform.right;
			wieldedGameObject.transform.rotation = transform.rotation;
		}
	}

	public void Wield(InventoryElement element)
	{
		Debug.Log ("Wielded");
		if(element.gameObject != null)
		{
			wieldedGameObject = (GameObject) Instantiate (element.gameObject, transform.position + Camera.main.transform.forward, Camera.main.transform.rotation);
			wieldedGameObject.transform.localScale = new Vector3(.20f,.10f,.10f);
		}
	}

	public void UnWield()
	{
		Debug.Log ("UnWielded");
		Destroy (wieldedGameObject);
	}

	public void Attack()
	{
		Debug.Log ("Attack!");
	}

	public void Block()
	{
		Debug.Log ("Block!");
	}
}
