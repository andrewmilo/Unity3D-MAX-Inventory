using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class CooldownSettings
{
	public float currentCooldownTime{get; private set;}
	public string[] options = new string[3]{"This Action", "This Element", "Type"};
	public string selectedElement;
	public string selectedType;
	public int selType;
	public int selElem;
	public int selOption = 0;
	public bool cooldownFoldout;
	public float cooldownTime;
	public bool drawCooldownAnimation;
	public bool drawCooldownTimer;
}
