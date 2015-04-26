using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class InventoryDatabase : MonoBehaviour {

	[SerializeField]
	private List<InventoryElement> elementDatabase = new List<InventoryElement>();
	[SerializeField]
	private List<ElementType> typeDatabase = new List<ElementType>();
	
	private InventoryDatabase(){}
	protected static InventoryDatabase instance;
	
	public static InventoryDatabase Instance
	{
		get
		{
			if(instance == null)
				instance = FindObjectOfType<InventoryDatabase>();

			if(instance == null)
			{
				Debug.Log ("No Inventory Database found in scene. Loading Inventory Database from Resources.");
				GameObject go = (GameObject) Instantiate (Resources.Load ("InventoryDatabase"));
				go.name = "InventoryDatabase";
				instance = go.GetComponent<InventoryDatabase>();
			}

			if(instance == null)
			{
				instance = new GameObject("InventoryDatabase").AddComponent<InventoryDatabase>();
				Debug.Log ("Could not locate an Inventory Database to load. Creating new database in scene. Save it as a prefab to persist changes between scenes.");
			}

			return instance;
		}
	}

	// Use this for initialization
	void Awake () {
		instance = this;
	}
	
	// Update is called once per frame 
	void Update () {
	}

	public static void Add(InventoryElement element, ElementType type)
	{
		if(Instance != null)
		{
			if(element != null && type != null)
			{
				//Set ID of the element
				element.id = Instance.elementDatabase.Count;
				//Set the Element's type
				element.typeID = type.ID;
				//Add element to Element Type list
				Instance.typeDatabase[type.ID].elementIDs.Add (element.id);
				//Add element to the database
				Instance.elementDatabase.Add (element);
			}
		}
	}

	public static void Add(ElementType newType, ElementType parent)
	{
		if(Instance != null && newType != null)
		{
			//Set ID of the Element Type
			newType.ID = Instance.typeDatabase.Count;
			//Add to parent's list of children
			if(parent != null)
				Instance.typeDatabase[parent.ID].childrenIDs.Add (newType.ID);
			//Add Element Type to the database
			Instance.typeDatabase.Add (newType);
		}
	}

	public static void Remove(InventoryElement element)
	{
		if(Instance != null && element != null)
		{
			if(element.id > -1 && element.type.ID > -1 && element.id < Instance.elementDatabase.Count && element.type.ID < Instance.typeDatabase.Count)
			{
				Instance.typeDatabase[element.type.ID].elementIDs.Remove (element.id);
				Instance.elementDatabase [element.id] = InventoryElement.Empty;
			}
		}
	}

	/// <summary>
	/// Deletes this node as well as all the nodes underneath
	/// </summary>
	public static void Remove(ElementType type)
	{
		if(Instance != null)
		{
			if(type.ID > -1 && type.ID < Instance.typeDatabase.Count)
			{
				int index = type.ID;

				//Remove all elements from this type
				type.GetElements().ForEach (x => Remove (x));

				//Remove all sub types
				type.GetSubTypes().ForEach (x => Remove (x));

				//If not root
				if(type.parentID > -1)
					Instance.typeDatabase[type.parentID].childrenIDs.Remove (type.ID);

				Instance.typeDatabase [index] = new ElementType ();
				Instance.typeDatabase [index].ID = -2;
				Instance.typeDatabase [index].parentID = -2;
			}
		}
	}

	public static InventoryElement FindElement(string name)
	{
		if(Instance != null)
			return Instance.elementDatabase.Find (x => x.name == name);

		return null;
	}

	public static ElementType FindElementType(string name)
	{
		if(Instance != null)
			return Instance.typeDatabase.Find (x => x.name == name);

		return null;
	}

	public static InventoryElement GetElement(int id)
	{
		if(Instance != null)
		{
			if(id > -1 && id < Instance.elementDatabase.Count)
				return Instance.elementDatabase[id];
		}

		return null;
	}

	public static ElementType GetElementType(int id)
	{
		if(Instance != null)
		{
			if(id > -1 && id < Instance.typeDatabase.Count)
				return Instance.typeDatabase[id];
		}

		return null;
	}

	public static int ElementCount
	{
		get
		{
			if(Instance != null)
				return Instance.elementDatabase.Count;

			return -1;
		}
	}

	public static int ElementTypeCount
	{
		get
		{
			if(Instance != null)
				return Instance.typeDatabase.Count;

			return -1;
		}
	}

	public static List<ElementType> GetAllElementTypes()
	{
		if(Instance != null)
			return Instance.typeDatabase.FindAll (x => x.ID > -1);

		return null;
	}

	public static List<InventoryElement> GetAllElements()
	{
		if(Instance != null)
			return Instance.elementDatabase.FindAll(x => x.id > -1);

		return null;
	}
}