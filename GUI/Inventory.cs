using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : InventoryObject
{
	void OnEnable()
	{
		InventoryManager.inventory = this;
	}

	//Add additional functionality here
	protected override void Update()
	{
		base.Update ();
	}
}
