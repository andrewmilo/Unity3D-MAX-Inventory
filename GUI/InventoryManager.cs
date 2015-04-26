using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
	private static InventoryManager instance;
	public static InventoryManager Instance
	{
		get
		{
			if(Application.isPlaying)
				return FindObjectOfType<InventoryManager>();
			else
			{
				if(instance != null)
					return instance;
				else
					return FindObjectOfType<InventoryManager>();
			}
		}
	}

	public Canvas canvas;
	public static ActionBar actionBar;
	public static Inventory inventory;
	public static CharacterMenu characterMenu;
	public EventSystem eventSystem;
	public List<InventoryObject> allInventoryObjects
	{
		get
		{
			List<InventoryObject> temp = new List<InventoryObject>();

			for(int i = 0; i < transform.childCount; i++)
			{
				InventoryObject invOb = transform.GetChild (i).GetComponent<InventoryObject>();

				temp.Add (invOb);
			}

			return temp;
		}
	}

	//Important
	public float dropOffset = 4;
	public GameObject character;
	public Transform dropTransform;
	public Camera cameraComponent;
	public bool showCursor = true;
	public bool lockCursor;
	public GUISkin inventorySkin;
	private bool forceCursor;
	public bool stackingActive = true;

	//Resolution
	public bool scalingOn;
	public bool useCurrentResolution;
	public Vector2 currentResolution;
	public Vector2 nativeResolution;
	public Vector2 scaleFactor;
	public static CanvasScaler canvasScaler;

	//Slot Settings
	public TextAnchor itemStackAnchor;
	public TextAnchor slotNumberAnchor;

	//Tooltip
	public GameObject tooltipGO;
	public GameObject tooltipNameGO;
	public GameObject tooltipDescriptionGO;
	public GameObject tooltipItemTypeGO;
	public RawImage tooltipRawImage;
	public Text tooltipItemTypeText;
	public Text tooltipNameText;
	public Text tooltipDescriptionText;
	public RectTransform tooltipNameRectTransform;
	public RectTransform tooltipDescriptionRectTransform;
	public RectTransform tooltipRect;
	public RectTransform tooltipItemTypeRect;
	public List<Text> tooltipTexts = new List<Text>();
	public List<RawImage> tooltipRawImages = new List<RawImage>();
	public RawImage tooltipItemIconRawImage;
	public RawImage tooltipBackground;

	//Slot and Item Management
	public static InventoryElement draggedItem{get; set;}
	private RawImage draggedIcon;
	public static  GameObject draggedGameObject{get; private set;}
	private GameObject draggedGameObjectTextGO;
	private Text draggedGameObjectStack;

	//Misc
	public static float screenWidth{get; private set;}
	public static float screenHeight{get; private set;}
	private Slot clickedSlot;
	private List<bool> prevObStates = new List<bool>();
	private bool prevObInit;
	private bool style1Activated;
	public RectTransform rectTransform;
	public bool cursorFoldout = true;
	public bool tooltipFoldout = true;
	public bool importantFoldout = true;
	public bool DisableScriptsFoldout = true;
	public bool slotSettingsFoldout = true;
	public bool scalingFoldout = true;
	private List<bool> savedCursorStates;
	private int savedStyle;
	private Slot droppedItemSlot;
	private bool splitActive = false;

	void OnApplicationQuit()
	{
		for(int i = 0; i < transform.childCount; i++)
		{
			InventoryObject invOb = transform.GetChild (i).GetComponent<InventoryObject>();
			
			if(invOb != null)
				invOb.Save(invOb.GetType ().ToString ());
		}
	}

	void Awake()
	{
		instance = this;
		eventSystem = FindObjectOfType<EventSystem>();

		canvas = transform.GetComponentInChildren<Canvas>();
		
		if(canvas == null)
			canvas = transform.GetComponentInParent<Canvas>();
		
		if(character == null)
			Debug.LogError ("Please set the Character field in the Inventory Manager GameObject");
		
		if(dropTransform == null)
			Debug.LogError ("Please set the Drop Transform field in the Inventory Manager GameObject");
		
		if(cameraComponent == null)
			Debug.LogError ("Please set the Camera Component field in the Inventory Manager GameObject");

		//Load items when not in editor
#if UNITY_EDITOR
#else
		for(int i = 0; i < allInventoryObjects.Count; i++)
		{
			InventoryObject invOb = allInventoryObjects[i];
			
			if(invOb != null)
			{
				//Load Items outside of editor
				invOb.Load (invOb.GetType ().ToString ());
			}
		}
#endif
	}

	void Update ()
	{
		if(Application.isPlaying)
		{
			Slot slot = Slot.slotWithCursor;

			bool mouse0Pressed = false;
			bool mouse1Pressed = false;
			bool mouse0Up = false;

			screenWidth = Screen.width;
			screenHeight = Screen.height;

			if(Input.GetMouseButtonDown (0))
			{
				clickedSlot = slot;
				mouse0Pressed = true;

				if(Input.GetKey (KeyCode.LeftShift))
				{
					if(slot.inventoryElement.name != "" && draggedItem == null)
					{
						if(slot.inventoryElement.stack > 1)
						{
							splitActive = true;
							SplitStack (slot.inventoryElement);
						}
					}
				}
			}
			if(Input.GetMouseButtonDown (1))
				mouse1Pressed = true;
			if(Input.GetMouseButtonUp (0))
				mouse0Up = true;

			if(eventSystem == null)
			{
				eventSystem = new GameObject("Event System").AddComponent<EventSystem>();
				eventSystem.gameObject.AddComponent<StandaloneInputModule>();
			}

			if(character != null)
			{
				if(!character.activeSelf)
				{
					if(prevObInit == false)
					{
						prevObStates.Clear ();
						
						foreach(InventoryObject invOb in allInventoryObjects)
						{
							prevObStates.Add(invOb.gameObject.activeSelf);
							invOb.gameObject.SetActive (false);
						}

						prevObInit = true;
					}
				}
				else
				{
					if(prevObInit == true)
					{
						int count = 0;
						foreach(InventoryObject invOb in allInventoryObjects)
						{
							invOb.gameObject.SetActive (prevObStates[count]);
							count++;
						}

						prevObInit = false;
					}
				}
			}

			style1Activated = false;
			forceCursor = false;

			//If the cursor is inside a slot
			if(slot != null)
			{
				if(draggedItem == null)
				{
					if(slot.inventoryElement != null)
					{
						if(slot.inventoryElement.name != "")
						{
							if(tooltipGO == null)
							{
								tooltipGO = new GameObject("Tooltip");
								tooltipGO.SetActive (false);
								tooltipGO.AddComponent<CanvasGroup>().blocksRaycasts = false;
								tooltipRawImage = tooltipGO.AddComponent<RawImage>();
								tooltipRect = tooltipRawImage.rectTransform;
								tooltipGO.transform.SetParent (this.transform, false);
							}
							
							if(tooltipNameGO == null)
							{
								tooltipNameGO = new GameObject("Name");
								tooltipNameGO.AddComponent<CanvasGroup>().blocksRaycasts = false;
								tooltipNameText = tooltipNameGO.AddComponent<Text>();
								tooltipNameRectTransform = tooltipNameText.rectTransform;
								tooltipNameText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
								if(TooltipSettings.Instance != null)
								{
									tooltipNameText.color = slot.inventoryElement.nameColor;
									tooltipNameText.fontStyle = TooltipSettings.Instance.tooltipNameFontStyle;
								}
								tooltipNameGO.transform.SetParent(tooltipGO.transform, false);
							}
							
							if(tooltipDescriptionGO == null)
							{
								tooltipDescriptionGO = new GameObject("Description");
								tooltipDescriptionGO.AddComponent<CanvasGroup>().blocksRaycasts = false;
								tooltipDescriptionText = tooltipDescriptionGO.AddComponent<Text>();
								tooltipDescriptionRectTransform = tooltipDescriptionText.rectTransform;
								tooltipDescriptionText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
								tooltipDescriptionText.alignment = TextAnchor.UpperLeft;
								if(TooltipSettings.Instance != null)
								{
									tooltipDescriptionText.color = slot.inventoryElement.descriptionColor;
									tooltipDescriptionText.fontStyle = TooltipSettings.Instance.tooltipDescriptionFontStyle;
								}
								tooltipDescriptionGO.transform.SetParent(tooltipGO.transform, false);
							}
							
							if(tooltipItemTypeGO == null)
							{
								tooltipItemTypeGO = new GameObject("Item Type");
								tooltipItemTypeGO.AddComponent<CanvasGroup>().blocksRaycasts = false;
								tooltipItemTypeText = tooltipItemTypeGO.AddComponent<Text>();
								tooltipItemTypeRect = tooltipItemTypeText.rectTransform;
								tooltipItemTypeText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

								if(TooltipSettings.Instance != null)
								{
									tooltipItemTypeText.color = slot.inventoryElement.type.tooltipColor;
									tooltipItemTypeText.fontStyle = TooltipSettings.Instance.tooltipTypeFontStyle;
								}
								tooltipItemTypeGO.transform.SetParent(tooltipGO.transform, false);
							}

							//Get name, description, and text
							tooltipNameText.text = slot.inventoryElement.name;
							tooltipDescriptionText.text = slot.inventoryElement.description;
							tooltipItemTypeText.text = slot.inventoryElement.type.name;

							//Set size of parts
							tooltipNameRectTransform.sizeDelta = new Vector2(tooltipNameText.preferredWidth, tooltipNameText.preferredHeight);
							tooltipItemTypeRect.sizeDelta = new Vector2(tooltipItemTypeText.preferredWidth, tooltipItemTypeText.preferredHeight);
							tooltipDescriptionRectTransform.sizeDelta = new Vector2(tooltipNameText.preferredWidth + tooltipItemTypeText.preferredWidth + TooltipSettings.Instance.minTooltipWidth, tooltipDescriptionText.preferredHeight);

							//Set size of Stats
							int fieldCount = 0;
							float totalStatSpace = 0;
							for(int m = 0; m < slot.inventoryElement.actions.Count; m++)
							{
								ElementAction itemAction = slot.inventoryElement.actions[m];

								if(itemAction != null)
								{
									if(itemAction.displayInTooltip && itemAction.tooltipText != "")
									{
										if(itemAction.itemActionGO == null)
										{
											itemAction.itemActionGO = new GameObject(itemAction.selectedFieldName);
											itemAction.itemActionGO.hideFlags = HideFlags.HideInHierarchy;
											itemAction.itemActionGO.transform.SetParent (tooltipGO.transform, false);
											itemAction.itemActionText = itemAction.itemActionGO.AddComponent<Text>();
											itemAction.itemActionText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
											//itemAction.itemActionText.fontStyle = FontStyle.BoldAndItalic;
											itemAction.itemActionText.rectTransform.sizeDelta = new Vector2(itemAction.itemActionText.preferredWidth, itemAction.itemActionText.preferredHeight);
										}

										totalStatSpace += itemAction.itemActionText.rectTransform.sizeDelta.y;
										
										fieldCount++;
									}
								}
							}

							//Determine the tallest between name and type
							float lineOneSpace = 0;
							if(tooltipNameText.preferredHeight >= tooltipItemTypeText.preferredHeight)
								lineOneSpace = tooltipNameText.preferredHeight;
							else
								lineOneSpace = tooltipItemTypeText.preferredHeight;

							//Min XY Dimensions
							float minY = lineOneSpace + totalStatSpace + tooltipDescriptionText.preferredHeight;
							float minX = tooltipNameText.preferredWidth + tooltipItemTypeText.preferredWidth;

							if(TooltipSettings.Instance != null)
							{
								tooltipRect.sizeDelta = new Vector2(minX + TooltipSettings.Instance.minTooltipWidth, minY + TooltipSettings.Instance.minTooltipHeight);
								
								tooltipNameRectTransform.localPosition = new Vector3(-tooltipRect.sizeDelta.x/2 + tooltipNameRectTransform.sizeDelta.x/2 + TooltipSettings.Instance.nameIndentation, tooltipRect.sizeDelta.y/2 - tooltipNameText.preferredHeight/2 - TooltipSettings.Instance.nameSpacing);
								tooltipItemTypeRect.localPosition = new Vector3(tooltipRect.sizeDelta.x/2 - tooltipItemTypeText.preferredWidth/2 + TooltipSettings.Instance.typeIndentation, tooltipRect.sizeDelta.y/2 - tooltipItemTypeText.preferredHeight/2 - TooltipSettings.Instance.typeSpacing);

								if(tooltipDescriptionText.cachedTextGenerator.lineCount > 1)
									tooltipDescriptionRectTransform.localPosition = new Vector3(TooltipSettings.Instance.descriptionIndentation, (tooltipNameRectTransform.localPosition.y - tooltipNameText.preferredHeight/2) - TooltipSettings.Instance.descriptionSpacing - tooltipDescriptionText.preferredHeight/2);
								else
									tooltipDescriptionRectTransform.localPosition = new Vector3(TooltipSettings.Instance.descriptionIndentation, (tooltipNameRectTransform.localPosition.y - tooltipNameText.preferredHeight/2) - TooltipSettings.Instance.descriptionSpacing);
								
								int spaceCount = 0;
								foreach(ElementAction itemAction in slot.inventoryElement.actions)
								{
									if(itemAction.displayInTooltip)
									{
										if(itemAction.itemActionText != null)
										{
											itemAction.itemActionText.text = itemAction.tooltipText;
											itemAction.itemActionText.rectTransform.sizeDelta = new Vector2(tooltipRect.sizeDelta.x - TooltipSettings.Instance.actionIndentation, itemAction.itemActionText.preferredHeight);
											
											if(spaceCount == 0)
												itemAction.itemActionText.rectTransform.localPosition = new Vector2(TooltipSettings.Instance.actionIndentation, tooltipDescriptionRectTransform.localPosition.y - tooltipDescriptionText.preferredHeight/2 - TooltipSettings.Instance.actionSpacing);
											else
												itemAction.itemActionText.rectTransform.localPosition = new Vector2(TooltipSettings.Instance.actionIndentation, slot.inventoryElement.actions[spaceCount - 1].itemActionText.rectTransform.localPosition.y - slot.inventoryElement.actions[spaceCount - 1].itemActionText.preferredHeight/2 - TooltipSettings.Instance.actionSpacing);
										}
										
										spaceCount++;
									}
								}

								//Set Texture Background
								if(TooltipSettings.Instance.tooltipBackgroundTexture != null)
								{
									tooltipRawImage.enabled = true;
									tooltipRawImage.texture = TooltipSettings.Instance.tooltipBackgroundTexture;
								}
								else
									tooltipRawImage.enabled = false;
							}

							Vector3 adjustment = new Vector3(tooltipRect.sizeDelta.x/2, tooltipRect.sizeDelta.y/2);

							//right
							if(Input.mousePosition.x + tooltipRect.sizeDelta.x > Screen.width)
								adjustment.x -= tooltipRect.sizeDelta.x;
							//upper
							if(Input.mousePosition.y + tooltipRect.sizeDelta.y > screenHeight)
								adjustment.y -= tooltipRect.sizeDelta.y;

							tooltipRect.position = Input.mousePosition + adjustment + new Vector3(20,0,0);

							tooltipGO.SetActive (true);
						}
					}
				}
				else
				{
					if(tooltipGO != null)
						Destroy (tooltipGO);
				}
			}
			else
			{
				if(tooltipGO != null)
					Destroy (tooltipGO);
			}

			if(!Input.GetKey (KeyCode.LeftShift))
			{
				if(mouse0Pressed)
				{
					if(draggedItem == null)
					{
						if(slot != null)
						{
							if(slot.inventoryElement.name != "" && !slot.lockItem)
							{
								//Start Drag
								draggedItem = slot.PickupElement();
								CreateDraggedGameObject (draggedItem);
							}
						}
					}
					//End Drag
					else
						DropHandler ();
				}
			}

			//Drag Element
			if(draggedIcon != null)
			{
				if(draggedGameObjectStack != null && draggedItem != null)
					draggedGameObjectStack.text = draggedItem.stack.ToString ();

				draggedIcon.rectTransform.position = Input.mousePosition;
				draggedIcon.enabled = true;
			}

			for(int i = 0; i < allInventoryObjects.Count; i++)
			{
				InventoryObject invOb = allInventoryObjects[i];

				//Operate on each individual InventoryObject
				if(invOb != null)
				{
					//Toggling
					if (Input.GetKeyDown(invOb.ToggleKey))
						invOb.gameObject.SetActive(!invOb.gameObject.activeSelf);

					//Check for required cursor states
					if(invOb.scriptsToDisable.Count > 0 && invOb.gameObject.activeSelf)
						forceCursor = true;

					//If Inventory Object is active
					if(invOb.gameObject.activeSelf)
					{
						//Single Pass
						if(invOb.prevStates == null)
						{
							invOb.prevStates = new List<bool>();

							//For each script
							foreach(Behaviour scriptToDisable in invOb.scriptsToDisable)
							{
								//If disabled, then another Inventory Object may be disabling it
								if(scriptToDisable.enabled == false)
								{
									for(int k = 0; k < allInventoryObjects.Count; k++)
									{
										InventoryObject invObject = allInventoryObjects[k];

										if(invObject.gameObject.activeSelf)
										{
											if(invObject.scriptsToDisable.Contains (scriptToDisable) && invObject != invOb)
											{
												//Has the Inventory Object disabled this script
												//If not, then either another Inventory Object disabled it,
												//Or it was simply disabled without any intervening
												if(invObject.prevStates != null)
												{
													//Save Previous script State
													invOb.prevStates.Add (invObject.prevStates[invObject.scriptsToDisable.IndexOf (scriptToDisable)]);
													break;
												}
											}
										}

										//If checked all inventory objects
										if(k == allInventoryObjects.Count - 1)
										{
											invOb.prevStates.Add (scriptToDisable.enabled);

											//Disable it
											scriptToDisable.enabled = false;
										}
									}
								}
								//If enabled, then we can safely disable it
								else
								{
									invOb.prevStates.Add (scriptToDisable.enabled);
									
									//Disable it
									scriptToDisable.enabled = false;
								}
							}
						}
					}
					else
					{
						//Single Pass
						if(invOb.prevStates != null)
						{
							for(int k = 0; k < invOb.scriptsToDisable.Count; k++)
							{
								bool contained = false;

								for(int p = 0; p < allInventoryObjects.Count; p++)
								{
									InventoryObject invObject = allInventoryObjects[p];

									//If no other Inventory Objects are disabling this script and are enabled
									if(invObject.gameObject.activeSelf)
									{
										//This means that it is being handled already
										if(invObject.scriptsToDisable.Contains (invOb.scriptsToDisable[k]) && invObject != invOb)
											contained = true;
									}
								}

								//If nobody else is in need of the script's state, then revert it
								if(!contained)
									invOb.scriptsToDisable[k].enabled = invOb.prevStates[k];
							}

							invOb.prevStates = null;
						}
					}
				}
			}

			if(forceCursor)
			{
				if(savedCursorStates == null)
				{
					savedCursorStates = new List<bool>();
					savedCursorStates.Add (Screen.lockCursor);
					savedCursorStates.Add (Cursor.visible);
				}

				//Forced states when an Inventory Object is opened
				Screen.lockCursor = false;
				Cursor.visible = true;
			}
			else
			{
				if(savedCursorStates != null)
				{
					Screen.lockCursor = savedCursorStates[0];
					Cursor.visible = savedCursorStates[1];

					savedCursorStates = null;
				}
			}
		}
	}

	public void CreateDraggedGameObject(InventoryElement element)
	{
		if(draggedGameObjectStack != null)
			draggedGameObjectStack.text = element.stack.ToString ();

		if(draggedGameObject == null)
		{
			draggedGameObject = new GameObject("Dragged Item");
			draggedGameObject.transform.SetParent(canvas.transform, false);
			draggedIcon = draggedGameObject.AddComponent<RawImage>();
			draggedIcon.texture = element.icon;
			draggedIcon.rectTransform.sizeDelta = new Vector2(45,45);
			draggedGameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
			draggedIcon.enabled = false;
			
			if(element.stack > 1)
			{
				draggedGameObjectTextGO = new GameObject("Item Stack");
				draggedGameObjectTextGO.transform.SetParent(draggedGameObject.transform, false);
				draggedGameObjectStack = draggedGameObjectTextGO.AddComponent<Text>();
				draggedGameObjectStack.rectTransform.sizeDelta = new Vector2(45,45);
				if(draggedGameObjectStack.font == null)
					draggedGameObjectStack.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
				draggedGameObjectStack.alignment = TextAnchor.LowerRight;

			}
		}
	}

	public void SplitStack(InventoryElement element)
	{
		InventoryElement temp = new InventoryElement(element);

		CreateDraggedGameObject (element);
		
		//Even
		if(element.stack % 2 == 0)
		{
			temp.stack = element.stack/2;
			element.stack /= 2;
		}
		//Odd
		else
		{
			temp.stack = Mathf.CeilToInt (element.stack/2);
			element.stack = temp.stack + 1;
		}
		
		if(temp.stack > 1)
		{
			if(draggedGameObjectStack != null)
				draggedGameObjectStack.text = temp.stack.ToString ();
		}

		//temp.originSlot = element.slot;
		draggedItem = temp;
		draggedItem.slot = null;
	}

	public void DropHandler()
	{
		//Dropping over GUI
		if(EventSystem.current.IsPointerOverGameObject ())
		{
			//If inside slot
			if(Slot.slotWithCursor != null)
			{
				Slot slot = Slot.slotWithCursor;

				slot.DropElement (draggedItem);
			}
			else
				Debug.Log ("Dropping over other GUI, add code here to process the item");
		}
		else
			DropIntoWorld (draggedItem);
	}

	public void DropIntoWorld(InventoryElement item)
	{
		Vector3 startTransform = dropTransform.transform.position;
		Vector3 forwardVector = cameraComponent.transform.forward;

		if(item.gameObject != null)
		{
			GameObject ins = (GameObject)Instantiate(item.gameObject, startTransform + forwardVector, item.gameObject.transform.rotation);
			ins.name = item.name;

			if(ins.GetComponent<Rigidbody>() == null)
				ins.AddComponent<Rigidbody>();
			
			if(ins.GetComponent<LootableObject>() == null)
				ins.AddComponent<LootableObject>();
			
			ins.GetComponent<LootableObject>().stack = item.stack;
			ins.GetComponent<LootableObject>().elementID = item.id;

			Vector3 endTransform = startTransform + (forwardVector * dropOffset);
			
			if (endTransform.y > 0f)
				ins.GetComponent<Rigidbody>().velocity = (forwardVector) * dropOffset;
			else
			{
				endTransform.y = .6f;
				ins.GetComponent<Rigidbody>().velocity = (forwardVector) * dropOffset;
			}

			Destroy (draggedGameObject);
			draggedItem = null;
		}
	}
}
