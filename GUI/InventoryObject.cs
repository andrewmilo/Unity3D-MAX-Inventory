using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public abstract class InventoryObject : MonoBehaviour
{
	//Display Settings
	public KeyCode ToggleKey;
	public bool TooltipEnabledLocal = true;
	public List<Behaviour> scriptsToDisable = new List<Behaviour>();
	[System.NonSerialized]
	public List<bool> prevStates;
	public bool forceCursor;

	//Transforms
	public Transform backgroundImageTransform;
	public Transform backgroundTextTransform;
	public Transform slotsTransform;
	public RawImage backgroundRawImage;
	public Text backgroundText;

	//Text
	public List<TextObject> textObjects = new List<TextObject>();
	public Vector3 textOffset;

	//Container
	public float percentageOfScreenX = .5f;
	public float percentageOfScreenY = .5f;
	public Vector3 bgOffset;
	public float bgSizeX = 500;
	public float bgSizeY = 500;
	public Texture backgroundImage;

	//Slots
	public List<Slot> Slots
	{
		get
		{
			List<Slot> temp = new List<Slot>();

			if(slotsTransform == null)
			{
				GameObject newGO = new GameObject("Slots");
				slotsRectTransform = newGO.AddComponent<RectTransform>();
				newGO.transform.parent = transform;
				newGO.transform.SetAsLastSibling();
				slotsTransform = newGO.transform;
			}

			if(slotsTransform != null)
			{
				slotsTransform = transform.FindChild ("Slots").transform;

				for(int i = 0; i < slotsTransform.childCount; i++)
				{
					Slot slot = slotsTransform.GetChild (i).GetComponent<Slot>();
					
					if(slot != null)
						temp.Add (slot);
				}
			}

			return temp;
		}
	}
	public float percentageOfContainerX = .5f;
	public float percentageOfContainerY = .5f;
	public int horizontalSlots = 5;
	public int verticalSlots = 5;
	public Vector2 SlotSpacing = new Vector2(1.15f, 1.15f);
	public Vector2 SlotSize = new Vector2(45, 45);
	public Texture2D slotTexture;
	public Color itemStackColor = new Color(255,255,255,255);

	//Menu
	protected bool splitMenu;
	protected Vector2 splitMenuCoords;
	protected string splitInput = "";
	protected bool splitActivated;
	protected Slot splitSlot;

	//Input Field
	private Transform inputFieldTransform;
	private InputField inputField;
	private RectTransform inputFieldRectTransform;

	//Transform
	public RectTransform rectTransform;
	//public float positionX;
	//public float positionY;
	public float sizeX;
	public float sizeY;

	//Misc
	public bool save;
	public bool numberEachSlot;
	public Color numberColor;
	public Vector3 numberOffset;
	private static int slotIndex = -1;
	public float screenWidth;
	public float screenHeight;
	private Canvas canvas;
	private bool loadedItems;
	public GameObject disableGO;
	public string[] disableStrings;
	public int selectedDisable;
	private RectTransform backgroundTextsRect;
	private RectTransform slotsRectTransform;

	//Editor Foldouts
	public bool displayFoldout = true;
	public bool slotFoldout = true;
	public bool elementManagementFoldout = true;
	public bool textFoldout = true;
	public bool positionFoldout = true;
	public bool disableScriptsFoldout;
	public bool backgroundFoldout;

	protected virtual void Awake()
	{
#if UNITY_EDITOR
		Load (GetType ().ToString ());
#endif
	}

	protected virtual void Update()
	{
		if(canvas == null)
			canvas = transform.GetComponentInParent<Canvas>();
		if(canvas == null)
			canvas = transform.GetComponentInChildren<Canvas>();
		if(rectTransform == null)
			rectTransform = GetComponent<RectTransform>();

		sizeX = Size ().x;
		sizeY = Size ().y;

		screenWidth = Screen.width;
		screenHeight = Screen.height;

		if(backgroundRawImage != null)
		{
			if(backgroundRawImage.enabled)
				rectTransform.sizeDelta = backgroundRawImage.rectTransform.sizeDelta;
			else
				rectTransform.sizeDelta = Size ();
		}
		else
			rectTransform.sizeDelta = Size ();

		rectTransform.position = new Vector3(percentageOfScreenX * screenWidth, screenHeight - percentageOfScreenY * screenHeight);

		//Manage Slot Position and Size
		float positionX = percentageOfScreenX * screenWidth;
		float positionY = percentageOfScreenY * screenHeight;

		if(Slots.Count != 0)
		{
			//Over the bottom of the screen
			if(positionY + rectTransform.sizeDelta.y/2 > screenHeight)
			{
				float diff = positionY + rectTransform.sizeDelta.y/2 - screenHeight + .5f;
				
				rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y + diff);
				positionY -= diff;
			}
			//Above the screen
			if(positionY - rectTransform.sizeDelta.y/2 < 0)
			{
				float diff = positionY - rectTransform.sizeDelta.y/2 - .5f;
				
				rectTransform.position = new Vector2(rectTransform.position.x, rectTransform.position.y + diff);
				positionY -= diff;
			}
			//Right of the screen
			if(positionX + rectTransform.sizeDelta.x/2 > screenWidth)
			{
				float diff = positionX + rectTransform.sizeDelta.x/2 - screenWidth - .5f;

				rectTransform.position = new Vector2(rectTransform.position.x - diff, rectTransform.position.y);
				positionX -= diff;
			}
			//Left of the screen
			if(positionX - rectTransform.sizeDelta.x/2 < 0)
			{
				float diff = positionX - rectTransform.sizeDelta.x/2 + .5f;
				
				rectTransform.position = new Vector2(rectTransform.position.x - diff, rectTransform.position.y);
				positionX -= diff;
			}
		}

		//Cache Slots, Background, and Text
		if(backgroundImageTransform == null)
		{
			backgroundImageTransform = transform.FindChild ("Background Image Main");
			
			if(backgroundImageTransform == null)
			{
				GameObject newGO = new GameObject("Main Background");
				newGO.AddComponent<CanvasGroup>().blocksRaycasts = false;
				newGO.AddComponent<RectTransform>().position = rectTransform.position;
				newGO.transform.parent = transform;
				newGO.transform.SetSiblingIndex (0);
				backgroundImageTransform = newGO.transform;
				backgroundRawImage = newGO.AddComponent<RawImage>();
				backgroundRawImage.rectTransform.position = rectTransform.position + bgOffset;
				backgroundRawImage.rectTransform.sizeDelta = rectTransform.sizeDelta;
			}
		}

		if(slotsTransform == null)
		{
			slotsTransform = transform.FindChild ("Slots");
			
			if(slotsTransform == null)
			{
				GameObject newGO = new GameObject("Slots");
				slotsRectTransform = newGO.AddComponent<RectTransform>();
				newGO.transform.parent = transform;
				newGO.transform.SetAsLastSibling();
				slotsTransform = newGO.transform;
			}
		}

		if(slotsRectTransform != null)
		{
			slotsRectTransform.transform.position = rectTransform.position;
			slotsRectTransform.sizeDelta = rectTransform.sizeDelta;
		}
	}

	private void Reset()
	{
		rectTransform = GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(500,500);
	}

	protected virtual void onShiftClick(InventoryElement element)
	{
	}

	/// <summary>
	/// Checks if this inventory object has any empty slots
	/// </summary>
	public bool isFull()
	{
		for (int i = 0; i < Slots.Count; i++)
		{
			if(Slots[i].inventoryElement == null)
				return false;
			else
			{
				if(Slots[i].inventoryElement.name == null)
					return false;

				if(Slots[i].inventoryElement.name == "")
					return false;
			}
		}
		
		return true;
	}

	/// <summary>
	/// Checks if this inventory object is full for this item
	/// </summary>
	public bool isFull(InventoryElement element) 
	{
		for (int i = 0; i < Slots.Count; i++)
		{
			InventoryElement slotItem = Slots[i].inventoryElement;
			Slot slot = Slots[i];

			if(slotItem == null)
				return false;
			else
			{
				if(slot.acceptedTypes.Count == 0 || slot.acceptedTypes.Exists(x => x.ID == element.type.ID))
				{
					if(slotItem.name == null)
						return false;

					if(slotItem.name == "")
						return false;

					if(InventoryManager.Instance != null)
					{
						if(InventoryManager.Instance.stackingActive)
						{
							if(slotItem.id == element.id && (slotItem.stack < slotItem.maxStack))
								return false;
						}
					}
				}
			}
		}
		
		return true;
	}

	/// <summary>
	/// Adds an item to this inventory object by spilling the stack across qualified slots.
	/// </summary>
	public bool AddItem(ref InventoryElement element, bool tryPreserveStack)
	{
		//If full then return entire stack back
		if(isFull (element))
		{
			//Reset
			slotIndex = -1;

			return false;
		}

		//Try to preserve stack by finding an empty slot
		//Else the item is split up
		if(tryPreserveStack)
		{
			for(int i = 0; i < Slots.Count; i++)
			{
				if(Slots[i].inventoryElement.name == "")
				{
					Slots[i].inventoryElement = element;
					element = InventoryElement.Empty;
					//Reset
					slotIndex = -1;

					return true;
				}
			}
		}

		for(int i = 0; i < Slots.Count; i++)
		{
			InventoryElement slotItem = Slots[i].inventoryElement;
			Slot slot = Slots[i];

			if(slot.acceptedTypes.Count == 0 || slot.acceptedTypes.Contains(element.type))
			{
				//If there is no Item
				if(slotItem.name == "" || slotItem.name == null)
				{
					//First empty slot
					if(slotIndex == -1)
					{
						//Save first empty slot
						slotIndex = i;

						//If stacking is disabled
						if(InventoryManager.Instance != null)
						{
							if(!InventoryManager.Instance.stackingActive)
							{
								slot.inventoryElement = element;
								element = InventoryElement.Empty;

								//Reset
								slotIndex = -1;

								return true;
							}
						}
					}
				}
				//Else there is an item
				else
				{
					//If you find the same item, add their stacks
					if(slotItem.prototype == element.prototype && InventoryManager.Instance.stackingActive)
					{
						if(slotItem.stack < slotItem.maxStack)
						{
							//If the two stacks can fit
							if(slotItem.stack + element.stack <= slotItem.maxStack)
							{
								slotItem.stack += element.stack;

								//Reset
								slotIndex = -1;

								element = InventoryElement.Empty;
								return true;
							}
							//Stack overflow
							else
							{
								//Amount that we can fill in
								int diff = slotItem.maxStack - slotItem.stack;

								if(diff > 0)
								{
									//Distribute stack
									slotItem.stack += diff;
									element.stack -= diff;
								}
								else
								{
									//Reset
									slotIndex = -1;

									return false;
								}
							}
						}
					}
				}
			}
		}

		//Add item
		if(slotIndex != -1)
		{
			Slots[slotIndex].inventoryElement = new InventoryElement (element);

			element = InventoryElement.Empty;
			
			//Reset
			slotIndex = -1;

			return true;
		}
		
		//Reset
		slotIndex = -1;

		return false;
	}

	/// <summary>
	/// Saves all slot items into memory
	/// </summary>
	public void Save(string key)
	{
		List<SerializableItem> serializedItems = new List<SerializableItem>();
		BinaryFormatter binaryFormatter = new BinaryFormatter();

		for(int i = 0; i < Slots.Count; i++)
		{
			Slot slot = Slots[i];

			if(slot.inventoryElement != null)
			{
				if(slot.inventoryElement.name != "" && slot.inventoryElement.id != -1)
					serializedItems.Add (new SerializableItem(slot.inventoryElement.id, slot.inventoryElement.stack));
				else
					serializedItems.Add (null);
			}
		}

		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize (memoryStream, serializedItems);
		string temp = System.Convert.ToBase64String(memoryStream.ToArray());

		PlayerPrefs.SetString (key, temp);
	}

	/// <summary>
	/// Returns Items if they were previously stored under 'key'
	/// </summary>
	public void Load(string key)
	{
		if(PlayerPrefs.HasKey (key))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();

			string temp = PlayerPrefs.GetString (key);
			
			if(temp == "" || temp == null)
				return;

			MemoryStream memoryStream = new MemoryStream(System.Convert.FromBase64String (temp));

			List<SerializableItem> serializedItems = binaryFormatter.Deserialize (memoryStream) as List<SerializableItem>;

			foreach(Slot slot in Slots)
			{
				slot.inventoryElement = InventoryElement.Empty;
			}

			if(serializedItems != null)
			{
				if(InventoryDatabase.Instance != null)
				{
					foreach(SerializableItem si in serializedItems)
					{
						if(si != null)
						{
							if(InventoryDatabase.Instance != null)
							{
								InventoryElement tempEl = InventoryDatabase.GetElement(si.itemID);

								if(tempEl != null)
								{
									if(Slots.Count > serializedItems.IndexOf (si))
									{
										Slots[serializedItems.IndexOf (si)].inventoryElement = new InventoryElement(tempEl);
										Slots[serializedItems.IndexOf (si)].inventoryElement.stack = si.stack;
									}
								}
								else
								{
									if(Slots.Count > serializedItems.IndexOf (si))
										Slots[serializedItems.IndexOf (si)].inventoryElement = InventoryElement.Empty;
								}
							}
							else
								Debug.LogError("There is no Inventory Database Instance!");
						}
					}

					if(Application.isPlaying)
						PlayerPrefs.DeleteKey (key);
				}
			}
		}
	}

	/// <summary>
	/// Returns the size of the inventory object
	/// </summary>
	public Vector2 Size()
	{
		Vector2 size = SlotSize + new Vector2((horizontalSlots - 1) * SlotSize.x * (SlotSpacing.x), (verticalSlots - 1) * SlotSize.y * (SlotSpacing.y));

		return size;
	}
}
