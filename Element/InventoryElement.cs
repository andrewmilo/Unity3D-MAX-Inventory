using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using System;
using System.Linq;

[Serializable]
public class InventoryElement : ICooldown
{
	public Slot slot;
	public int id = -1;
	public string name = "";
	public string description;
	public Texture icon;
	public Color nameColor = Color.white;
	public Color descriptionColor = Color.white;
	public GameObject gameObject;
	public bool isStackable;
	public int stack = 1;
	public int maxStack;
	public bool foldout;
	public bool windowFoldout;
	public bool windowActionFoldout = true;
	public int selectedType;
	public int typeID = -1;
	public ElementType type
	{
		get
		{
			return InventoryDatabase.GetElementType(typeID);
		}
		set
		{
			typeID = value.ID;
		}
	}
	private static InventoryElement empty;
	public static InventoryElement Empty
	{
		get
		{
			if(empty == null)
				empty = new InventoryElement();

			return empty;
		}
	}
	private bool onCooldown;
	public bool OnCooldown
	{
		get
		{
			//If this element is on cooldown OR this element's type is on cooldown
			if(onCooldown || type.OnCooldown || prototype.onCooldown)
				return true;

			return false;
		}
		set
		{
			prototype.onCooldown = value;
			onCooldown = value;
		}
	}
	public InventoryElement prototype;
	public List<ElementAction> actions = new List<ElementAction>();
	public CooldownManager cooldownGO {get;set;}

	//Misc
	public bool deleteFoldout;
	public bool areYouSure;
	public bool areYouSure2;

	public InventoryElement(){}
	public InventoryElement (InventoryElement inventoryElement)
	{
		if(inventoryElement != null)
		{
			id = inventoryElement.id;
			name = inventoryElement.name;
			gameObject = inventoryElement.gameObject;
			description = inventoryElement.description;
			type = inventoryElement.type;
			selectedType = inventoryElement.selectedType;
			icon = inventoryElement.icon;
			stack = inventoryElement.stack;
			isStackable = inventoryElement.isStackable;
			maxStack = inventoryElement.maxStack;
			actions = inventoryElement.actions;

			//This means it is the original
			if(inventoryElement.prototype == null)
				prototype = inventoryElement;
			//This means that inventoryElement is an instance
			else
				prototype = inventoryElement.prototype;
		}
	}

	public void Use(ElementAction action)
	{
		if(!this.OnCooldown && !action.OnCooldown)
		{
			MonoBehaviour[] scripts = action.activationObject.GetComponents<MonoBehaviour>();
			MonoBehaviour script = null;

			foreach(MonoBehaviour mb in scripts)
			{
				if(mb.GetType ().ToString () == action.selectedComponentName)
				{
					script = (MonoBehaviour)action.activationObject.GetComponent(action.selectedComponentName);
					break;
				}
			}
			
			if(script != null)
			{
				//Methods
				if(action.selectedOption == 0)
				{
					action.cachedMethod = script.GetType ().GetMethod(action.activationMethodName);
					
					//Manage Parameters
					if(action.sendThisItem)
						action.cachedMethod.Invoke (script, new object[]{this});
					else
						action.cachedMethod.Invoke (script, null);
				}
				//Fields
				else if(action.selectedOption == 1)
				{
					action.cachedField = script.GetType ().GetField(action.selectedFieldName);
					
					if(action.cachedField != null)
					{
						if(action.cachedField.GetValue (script) is int)
						{
							int intVal = int.Parse (action.fieldValue);
							int oldVal = (int) action.cachedField.GetValue (script);
							action.cachedField.SetValue (script, oldVal + intVal);
							
							if(action.hasDuration)
								DurationManager.Add (action);
						}
						else if(action.cachedField.GetValue (script) is float)
						{
							float intVal = float.Parse (action.fieldValue);
							float oldVal = (float) action.cachedField.GetValue (script);
							action.cachedField.SetValue (script, oldVal + intVal);
							
							if(action.hasDuration)
								DurationManager.Add (action);
						}
						else if(action.cachedField.GetValue (script) is double)
						{
							double intVal = double.Parse (action.fieldValue);
							double oldVal = (double) action.cachedField.GetValue (script);
							action.cachedField.SetValue (script, oldVal + intVal);
							
							if(action.hasDuration)
								DurationManager.Add (action);
						}
					}
				}

				//Order cooldowntimes 
				List<CooldownSettings> sortedList = action.cooldownSettings.OrderByDescending(o=>o.cooldownTime).ToList ();

				foreach(CooldownSettings cds in sortedList)
				{
					//This action
					if(cds.options[cds.selOption] == "This Action")
					{
						if(action.cooldownGO != null)
							MonoBehaviour.Destroy(action.cooldownGO);

						GameObject go = new GameObject(name + " Cooldown");	
						action.cooldownGO = go.AddComponent<CooldownManager>();
						action.cooldownGO.Init (action, cds);

						if(sortedList.IndexOf (cds) > 0)
							action.cooldownGO.exclusions.Add (this);
					}
					else if(cds.options[cds.selOption] == "Type")
					{
						if(InventoryDatabase.Instance != null)
						{
							ElementType e = InventoryDatabase.FindElementType(cds.selectedType);

							if(e != null)
							{
								if(e.cooldownGO != null)
									MonoBehaviour.Destroy (e.cooldownGO);

								GameObject go = new GameObject(name + " Cooldown");	
								e.cooldownGO = go.AddComponent<CooldownManager>();
								e.cooldownGO.Init (e, cds);

								if(sortedList.IndexOf (cds) > 0)
									e.cooldownGO.exclusions.Add (this);
							}
						}
					}
					else if(cds.options[cds.selOption] == "This Element")
					{
						if(InventoryDatabase.Instance != null)
						{
							if(this.cooldownGO != null)
								MonoBehaviour.Destroy (this.cooldownGO);

							GameObject go = new GameObject(name + " Cooldown");	
							this.cooldownGO = go.AddComponent<CooldownManager>();
							this.cooldownGO.Init (this, cds);

							if(sortedList.IndexOf (cds) > 0)
								this.cooldownGO.exclusions.Add (this);
						}
					}
				}

				if(action.destroyAfterUse)
				{
					if(stack > 1)
						stack--;
					else
					{
						//Destroy this item
						slot.inventoryElement = Empty;
					}
				}
			}
		}
		else
		{
			Debug.Log (this.name + " is on cooldown.");
		}
	}

	public void UnEquip()
	{
		foreach(ElementAction action in actions)
		{
			if(action.activateOnEquip)
			{
				if(action.selectedOption == 1)
				{
					MonoBehaviour component = (MonoBehaviour) action.activationObject.GetComponent(action.selectedComponentName);
					System.Reflection.FieldInfo fieldInfo = component.GetType ().GetField (action.selectedFieldName);
					
					if(fieldInfo.GetValue(component) is int)
					{
						int intVal = int.Parse (action.fieldValue);
						int oldVal = (int) fieldInfo.GetValue(component);
						fieldInfo.SetValue (component, oldVal - intVal);
					}
					else if(fieldInfo.GetValue(component) is float)
					{
						float floatVal = float.Parse (action.fieldValue);
						float oldVal = (float) fieldInfo.GetValue(component);
						fieldInfo.SetValue (component, oldVal - floatVal);
					}
					else if(action.cachedField.GetValue (component) is double)
					{
						double intVal = double.Parse (action.fieldValue);
						double oldVal = (double) fieldInfo.GetValue (component);
						action.cachedField.SetValue (component, oldVal - intVal);
					}
				}
			}
		}
	}

	public IEnumerator StartCooldown(float time)
	{
		OnCooldown = true;
		
		yield return new WaitForSeconds (time);
		
		OnCooldown = false;
	}
}
