using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ElementType : ICooldown
{
	public string name;
	public int ID = -1;
	public int parentID = -1;
	public List<int> childrenIDs = new List<int>();
	public List<int> elementIDs = new List<int>();
	private bool onCooldown;
	public bool OnCooldown
	{
		get
		{
			return onCooldown;
		}
		set
		{
			//Set this cooldown
			onCooldown = value;
			//Set cooldowns of all sub types
			GetSubTypes ().ForEach (x => x.onCooldown = value);
		}
	}
	public CooldownManager cooldownGO {get;set;}

	public List<ElementType> GetSubTypes()
	{
		List<ElementType> temp = new List<ElementType>();

		foreach(int i in childrenIDs)
		{
			ElementType e = InventoryDatabase.GetElementType (i);

			if(e != null)
			{
				temp.Add (e);
				temp.AddRange (e.GetSubTypes ());
			}
		}

		return temp;
	}
	
	public List<InventoryElement> GetElements()
	{
		List<InventoryElement> temp = new List<InventoryElement>();

		elementIDs.ForEach (x => temp.Add (InventoryDatabase.GetElement (x)));

		GetSubTypes ().ForEach (x => x.elementIDs.ForEach (e => temp.Add (InventoryDatabase.GetElement (e))));

		return temp;
	}

	public bool isAncestorOf(ElementType eType)
	{
		if(eType != null)
			return GetSubTypes().Exists(x => x.ID == eType.ID);

		return false;
	}
	
	public bool isAncestorOf(int eTypeID)
	{
		return GetSubTypes().Exists(x => x.ID == eTypeID);
	}

	public bool isSubTypeOf(ElementType other)
	{
		if(other.isAncestorOf (this))
			return true;

		return false;
	}

	public IEnumerator StartCooldown(float time)
	{
		OnCooldown = true;
		
		yield return new WaitForSeconds (time);
		
		OnCooldown = false;
	}

	//Misc
	public Color tooltipColor = Color.white;
	[HideInInspector]
	public bool areYouSure;
	[HideInInspector]
	public bool areYouSure2;
	[HideInInspector]
	public bool deleteFoldout;
}