using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using System;

[Serializable]
public class ElementAction : ICooldown
{
	public bool foldout;
	public GameObject activationObject;
	public string activationObjectName;
	public string activationMethodName;
	public bool repeatingInvoke;
	public bool currentlyRepeating;
	public int selectedComponent;
	public int selectedMethod;
	public int selectedField;
	public string fieldValue;
	public bool sendThisItem;
	public Color tooltipColor = Color.white;
	public Font tooltipFont;
	public FontStyle tooltipFontStyle;
	public bool hasCooldown;
	public bool OnCooldown { get; set; }
	public List<CooldownSettings> cooldownSettings = new List<CooldownSettings>();
	public int selectedElement;
	public bool cooldownsFoldout;
	public bool elementsFoldout;
	public int selectedType;
	public bool hasDuration;
	public float durationTime;
	public bool parameterFoldout;
	public MethodInfo cachedMethod;
	public FieldInfo cachedField;
	public int selectedOption;
	public string[] options = new string[2]{"Method", "Field"};
	public MonoBehaviour selectedMonoBehaviour;
	public string selectedMonoBehaviourName;
	public string selectedFieldName;
	public string selectedComponentName;
	public GameObject itemActionGO;
	public UnityEngine.UI.Text itemActionText;
	public bool destroyAfterUse;
	public bool useOnActivation;
	public bool useOnDeactivation;
	public bool activateOnEquip;
	public bool activateOnUnEquip;
	public bool useOnClick;
	public bool clickedOnByMouse1;
	public bool respondToMouse0;
	public bool respondToMouse1;
	public bool tooltipFoldout;
	public bool displayInTooltip;
	public bool customTooltip;
	public string tooltipText;
	public bool onHotkey;
	public CooldownManager cooldownGO {get;set;}

	public IEnumerator StartCooldown(float time)
	{
		OnCooldown = true;
		
		yield return new WaitForSeconds (time);

		OnCooldown = false;
	}
}
