using UnityEngine;
using System.Collections;

[System.Serializable]
public class SerializableItem {

	public int itemID;
	public int stack;

	public SerializableItem(int _itemID, int _stack)
	{
		itemID = _itemID;
		stack = _stack;
	}
}
