using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThirdPersonLooting : MonoBehaviour 
{
	public Camera cameraComponent;
	public float distance = 5f;
	public KeyCode lootKey = KeyCode.Mouse0;
	public LayerMask layerMask;
	public List<InventoryObject> priority = new List<InventoryObject>();
	private RaycastHit hit;
	private Ray ray;
	private LootableObject lootableObject;
	private InventoryElement temp;
	
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
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if(Physics.Raycast (ray, out hit, distance, layerMask))
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
