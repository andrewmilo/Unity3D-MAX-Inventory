using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirstPersonLooting : MonoBehaviour 
{
	public Camera cameraComponent;
	public float distance = 10f;
	public KeyCode lootKey = KeyCode.F;
	public LayerMask layerMask;
	public List<InventoryObject> priority = new List<InventoryObject>();
	private RaycastHit hit;

	LootableObject lootableObject;
	InventoryElement temp;

	// Use this for initialization
	void Start () {

		if(cameraComponent == null)
			cameraComponent = InventoryManager.Instance.cameraComponent;

		layerMask = LayerMask.NameToLayer("Everything");

		if(priority.Count == 0)
		{
			priority.Add (FindObjectOfType<Inventory>());
			priority.Add (FindObjectOfType<ActionBar>());
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if(cameraComponent != null)
		{
			if(Physics.Raycast (transform.position, cameraComponent.transform.forward, out hit, distance, layerMask))
			{
				if(hit.transform.gameObject != null)
				{
					GameObject possibleItem = hit.transform.gameObject;

					if(possibleItem.GetComponent<LootableObject>() != null)
					{
						if(Input.GetKeyDown (lootKey))
						{
							if(lootableObject != possibleItem.GetComponent<LootableObject>())
							{
								lootableObject = possibleItem.GetComponent<LootableObject>();

								InventoryElement invElem = InventoryDatabase.GetElement (lootableObject.elementID);

								temp = new InventoryElement(invElem);
							}
							else
								lootableObject.stack = temp.stack;

							if(temp != null)
							{
								temp.stack = lootableObject.stack;

								if(priority.Count > 0)
								{
									foreach(InventoryObject invOb in priority)
									{
										if(invOb != null)
										{
											if(invOb.AddItem (ref temp, false))
												Destroy (lootableObject.gameObject);

											break;
										}
									}
								}
								else
									Debug.Log ("Set up the priority system in FirstPersonLooting!");
							}
						}
					}
				}
			}
		}
	}
}
