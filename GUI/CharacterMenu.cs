using UnityEngine;
using System.Collections;

public class CharacterMenu : InventoryObject {

	void OnEnable()
	{
		InventoryManager.characterMenu = this;
	}

	protected override void onShiftClick (InventoryElement element)
	{
//		if(InventoryManager.actionBar.enabled)
//		{
//			InventoryManager.actionBar.AddItem(element, true);
//		}
	}
}
