using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionBar : InventoryObject {

	void OnEnable()
	{
		InventoryManager.actionBar = this;
	}

	protected override void Update ()
	{
		base.Update ();
	}
}
