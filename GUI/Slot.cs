using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;

/// <summary>
/// Represents a Slot that contains an Inventory Element
/// </summary>

#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
#endif
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public static List<Slot> allSlots = new List<Slot>();
	public static Slot slotWithCursor;
	public static Slot activatedSlot;
	public InventoryObject inventoryObject;
	public GameObject activationCharacterGO;
	public Text activationCharacterText;
	public bool activationCharacterFoldout;
	public Text stackText;
	public GameObject stackGO;
	public RectTransform rectTransform;
	public Transform backgroundImageTransform;
	public Transform slotIconImageTransform;
	public RawImage backgroundRawImage;
	public RawImage slotRawIconImage;
	public Transform textTransform;
	public InventoryElement inventoryElement;
	public List<ElementType> acceptedTypes = new List<ElementType>();
	public bool itemTypesFoldout;
	public string[] activationCharacters = new string[]{"None", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
	public string activationCharacter;
	public int activationInt;
	public GameObject numberGO;
	public Text numberText;
	public Text slotText;
	public GameObject slotTextGO;
	public bool slotTextFoldout;
	public bool slotActivationSettingsFoldout;
	public TextAnchor textAnchor = TextAnchor.MiddleCenter;
	public bool disableTextIfItem = true;
	public UnityEngine.Object actionMethod;
	public bool lockItem;
	public int itemSelection = 0;
	public int itemSelectionStack = 1;
	public bool itemFoldout;
	public bool itemStackChanged;
	public bool activationResponseFoldout;
	public bool changeSize = true;
	public Vector2 changeSizeVector2 = new Vector2(5,5);
	public bool changeTexture;
	public Texture changeTextureImage;
	public int selectedItemType;
	public bool itemChanged;
	public bool hasItem;
	public bool itemDeposited;
	public InventoryElement currentItem;
	public bool backgroundFoldout;
	public List<GameObject> cooldownGameObjects = new List<GameObject>();
	public GameObject cooldownGameObject;
	public Image cooldownImage;
	public GameObject cooldownTextGameObject;
	public Text cooldownText;
	private InventoryElement cachedItem;
	private Texture cachedTexture;
	public bool ifActivateOnHotkey = true;

	public void Init()
	{
		rectTransform = GetComponent<RectTransform>();


		//Create GameObject for BG
		backgroundImageTransform = new GameObject("Slot Background Image").transform;
		backgroundImageTransform.gameObject.hideFlags = HideFlags.HideInHierarchy;
		backgroundImageTransform.parent = transform;
		backgroundRawImage = backgroundImageTransform.gameObject.AddComponent<RawImage>();
		backgroundRawImage.rectTransform.sizeDelta = rectTransform.sizeDelta;

		//Create GameObject for Icon
		slotIconImageTransform = new GameObject("Slot Icon Image").transform;
		slotIconImageTransform.gameObject.hideFlags = HideFlags.HideInHierarchy;
		slotIconImageTransform.transform.parent = transform;
		slotRawIconImage = slotIconImageTransform.gameObject.AddComponent<RawImage>();
		slotRawIconImage.rectTransform.sizeDelta = rectTransform.sizeDelta - new Vector2(30,30);
		slotRawIconImage.enabled = false;
		
		//Create GameObject for Text
		stackGO = new GameObject("Slot Item Stack");
		stackGO.gameObject.hideFlags = HideFlags.HideInHierarchy;
		stackGO.transform.parent = transform;
		stackText = stackGO.AddComponent<Text>();
		if(stackText.font == null)
			stackText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		stackText.rectTransform.sizeDelta = rectTransform.sizeDelta;
		stackText.fontStyle = FontStyle.Bold;
		stackText.alignment = TextAnchor.LowerRight;
		stackText.rectTransform.position = rectTransform.position - new Vector3(2f, 0);
		stackText.gameObject.SetActive (false);

		//Create GameObject for Text
		slotTextGO = new GameObject("Slot Text");
		slotTextGO.gameObject.hideFlags = HideFlags.HideInHierarchy;
		slotTextGO.transform.parent = transform;
		slotText = slotTextGO.AddComponent<Text>();
		if(slotText.font == null)
			slotText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		slotText.rectTransform.sizeDelta = rectTransform.sizeDelta;
		slotText.alignment = TextAnchor.MiddleCenter;
		slotText.enabled = false;

		//Create GameObject for Activation Text
		activationCharacterGO = new GameObject("Activation Char");
		activationCharacterGO.hideFlags = HideFlags.HideInHierarchy;
		activationCharacterGO.transform.parent = transform;
		activationCharacterText = activationCharacterGO.AddComponent<Text>();
		activationCharacterText.fontStyle = FontStyle.Bold;
		if(activationCharacterText.font == null)
			activationCharacterText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		activationCharacterText.alignment = TextAnchor.UpperRight;
		activationCharacterText.rectTransform.sizeDelta = rectTransform.sizeDelta;
	}

	private void Update()
	{
		inventoryObject = GetComponentInParent<InventoryObject>();
		activationCharacter = activationCharacters[activationInt];

		if(!allSlots.Contains (this))
			allSlots.Add (this);

		slotRawIconImage.rectTransform.sizeDelta = rectTransform.sizeDelta - new Vector2(5,5);
		backgroundRawImage.rectTransform.sizeDelta = rectTransform.sizeDelta;
		stackText.rectTransform.sizeDelta = rectTransform.sizeDelta;
		slotText.rectTransform.sizeDelta = rectTransform.sizeDelta;
		activationCharacterText.rectTransform.sizeDelta = rectTransform.sizeDelta;

		//Find out when an element is equipped
		if(Application.isPlaying)
		{
			if(inventoryElement != null)
			{
				if(inventoryElement.id > -1)
				{
					if(cachedItem == null)
					{
						cachedItem = inventoryElement;

						if(acceptedTypes.Exists(x => x.ID == inventoryElement.type.ID || x.isAncestorOf (inventoryElement.type)))
						{
							foreach(ElementAction itemAction in inventoryElement.actions)
							{
								if(itemAction.activateOnEquip)
									inventoryElement.Use (itemAction);
							}
						}
					}
				}

				if(inventoryElement.name == "" && cachedItem != null)
				{
					if(acceptedTypes.Exists(x => x.ID == cachedItem.type.ID || x.isAncestorOf (cachedItem.type)))
					{
						//For Stats/Fields
						cachedItem.UnEquip ();

						//For Method
						foreach(ElementAction itemAction in inventoryElement.actions)
						{
							if(itemAction.activateOnUnEquip)
								inventoryElement.Use (itemAction);
						}
					}

					cachedItem = null;
				}
			}
		}

		//If slot is active
		if(gameObject.activeSelf)
		{
			//Called when 'Play' is live
			if(Application.isPlaying)
			{
				activationCharacter = activationCharacters[activationInt];
					
				//If activation char has a value
				if(activationCharacter != "None")
				{
					//If you press activation char
					if(Input.GetKeyDown (activationCharacter.ToLower ()))
					{
						//On Hotkey
						//If the slot has an item
						if(inventoryElement.name != "")
						{
							//For each action
							foreach(ElementAction itemAction in inventoryElement.actions)
							{
								if(itemAction.onHotkey)
								{
									//Reoccurring call
									if(itemAction.repeatingInvoke)
										itemAction.currentlyRepeating = true;
									else
										inventoryElement.Use (itemAction);							
								}
							}
						}

						if(ifActivateOnHotkey)
						{
							//If there is a slot activated
							if(Slot.activatedSlot != null)
							{
								//Disable any currently repeating functions
								if(Slot.activatedSlot.inventoryElement != null)
								{
									foreach(ElementAction itemAction in Slot.activatedSlot.inventoryElement.actions)
										itemAction.currentlyRepeating = false;
								}

								foreach(ElementAction itemAction in Slot.activatedSlot.inventoryElement.actions)
								{
									//On Deactivation
									if(itemAction.useOnDeactivation)
									{
										//Reoccurring call
										if(itemAction.repeatingInvoke)
											itemAction.currentlyRepeating = true;
										else
											Slot.activatedSlot.inventoryElement.Use (itemAction);
									}
								}
							}

							//If it's this slot, then disable it
							if(Slot.activatedSlot == this)
							{
								if(changeSize)
									rectTransform.sizeDelta -= changeSizeVector2;
								if(changeTexture)
								{
									backgroundRawImage.texture = cachedTexture;
									cachedTexture = null;
								}

								Slot.activatedSlot = null;
							}
							else
							{
								Slot.activatedSlot = this;

								if(changeSize)
									rectTransform.sizeDelta += changeSizeVector2;
								if(changeTexture)
								{
									cachedTexture = backgroundRawImage.texture;
									backgroundRawImage.texture = changeTextureImage;
								}

								//If the slot has an item
								if(inventoryElement.name != "")
								{
									//For each action
									foreach(ElementAction itemAction in inventoryElement.actions)
									{
										if(itemAction.useOnActivation)
										{
											//Reoccurring call
											if(itemAction.repeatingInvoke)
												itemAction.currentlyRepeating = true;
											else
												inventoryElement.Use (itemAction);							
										}
									}
								}
							}
						}
					}
				}

				//Repeating Invoke
				foreach(ElementAction itemAction in inventoryElement.actions)
				{
					if(itemAction.currentlyRepeating)
						inventoryElement.Use (itemAction);
				}
				//Process repeating Invoke
				if(Slot.activatedSlot == this)
				{
					//If there is an item
					if(inventoryElement.name != "")
					{
						//For each action
						foreach(ElementAction itemAction in inventoryElement.actions)
						{
							if(itemAction.respondToMouse0)
							{
								if(Input.GetMouseButtonDown(0))
								{
									if(itemAction.repeatingInvoke)
										itemAction.currentlyRepeating = true;
									else
										inventoryElement.Use (itemAction);
								}
							}
							
							if(Slot.activatedSlot == this)
							{
								if(itemAction.respondToMouse1)
								{
									if(Input.GetMouseButtonDown(1))
									{
										if(itemAction.repeatingInvoke)
											itemAction.currentlyRepeating = true;
										else
											inventoryElement.Use (itemAction);
									}
								}
							}
						}
					}
				}
			}

			if(activationCharacter != null && activationCharacter != "None")
			{
				activationCharacterText.enabled = true;
				activationCharacterText.text = activationCharacter;
			}
			else
			{
				if(activationCharacterText != null)
					activationCharacterText.enabled = false;
			}

			if(inventoryElement != null)
			{
				if(inventoryElement.slot != this)
					inventoryElement.slot = this;
				
				//If there IS an item
				if(inventoryElement.name != "")
				{
					//If it has an icon
					if(inventoryElement.icon != null)
					{
						slotRawIconImage.enabled = true;
						
						//Set Icon
						slotRawIconImage.texture = inventoryElement.icon;
						slotRawIconImage.rectTransform.sizeDelta = rectTransform.sizeDelta - new Vector2(8,8);
					}

					if(InventoryManager.Instance != null)
					{
						if(InventoryManager.Instance.stackingActive && inventoryElement.stack > 1)
						{
							stackGO.SetActive (true);
							if(stackText.font == null)
								stackText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
							stackText.text = inventoryElement.stack.ToString ();
						}
						else
							stackText.text = string.Empty;
					}
				}
				else
				{
					stackGO.SetActive (false);
					
					//Destroy RawImage
					if(slotRawIconImage != null)
						slotRawIconImage.enabled = false;
				}
			}
			
			//Slot text
			if(slotText != null)
			{
				if(inventoryElement != null)
				{
					if(disableTextIfItem && inventoryElement.name != "")
						slotText.enabled = false;
					else
						slotText.enabled = true;
				}
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		slotWithCursor = this;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(slotWithCursor == this)
			slotWithCursor = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if(inventoryElement.name != "")
		{
			//On right click
			if(eventData.button == PointerEventData.InputButton.Right && InventoryManager.draggedItem == null)
			{
				foreach(ElementAction itemAction in inventoryElement.actions)
				{
					if(itemAction.clickedOnByMouse1)
						inventoryElement.Use (itemAction);
				}
			}
		}
	}

	public InventoryElement PickupElement()
	{
		InventoryElement temp;
		temp = inventoryElement;

		inventoryElement = InventoryElement.Empty;

		return temp;
	}

	public void DropElement(InventoryElement draggedItem)
	{
		bool matchingType = acceptedTypes.Exists (x => x.ID == draggedItem.type.ID || x.isAncestorOf (draggedItem.type));

		//If there is NO item type OR the item types match
		if((acceptedTypes.Count == 0 || matchingType) && !lockItem)
		{
			if(inventoryElement.name != "")
			{
				//If Stacking is allowed, and items match - stack them.
				if (inventoryElement.isStackable && inventoryElement.stack < inventoryElement.maxStack && inventoryElement.id == draggedItem.id)
				{
					//Stacks fit into 1 Slot
					if (inventoryElement.stack + draggedItem.stack <= inventoryElement.maxStack)
					{
						DestroyImmediate (InventoryManager.draggedGameObject);
						inventoryElement.stack += draggedItem.stack;
						InventoryManager.draggedItem = null;
					}
					//Stack overflow
					else
					{
						int Diff = Mathf.Abs (inventoryElement.maxStack - inventoryElement.stack);
						inventoryElement.stack += Diff;
						draggedItem.stack -= Diff;
					}
				}
				//Else, just swap them.
				else
				{
					Transform cd = this.transform.FindChild ("Cooldown");
					if(cd != null)
						DestroyImmediate (cd.gameObject);
					InventoryElement temp = inventoryElement;
					inventoryElement = draggedItem;
					DestroyImmediate (InventoryManager.draggedGameObject);
					InventoryManager.draggedItem = temp;
					InventoryManager.Instance.CreateDraggedGameObject (temp);
				}
			}
			//No Item
			else
			{
				DestroyImmediate (InventoryManager.draggedGameObject);
				inventoryElement = draggedItem;
				InventoryManager.draggedItem = null;
			}
		}
	}

	private void OnDisable()
	{
		if(slotWithCursor == this)
			slotWithCursor = null;

		allSlots.Remove (this);
	}

	private void OnDestroy()
	{
		allSlots.Remove (this);

		if(slotWithCursor == this)
			slotWithCursor = null;

		if(activationCharacterGO != null)
			DestroyImmediate (activationCharacterGO);
		
		if(stackGO != null)
			DestroyImmediate (stackGO);
		
		if(numberGO != null)
			DestroyImmediate (numberGO);
		
		if(slotTextGO != null)
			DestroyImmediate (slotTextGO);
	}
}
