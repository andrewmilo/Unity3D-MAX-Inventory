using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TooltipSettings : MonoBehaviour {

	private TooltipSettings(){}
	protected static TooltipSettings instance;
	public static TooltipSettings Instance
	{
		get
		{
			GameObject go = (GameObject) Resources.Load ("TooltipSettings");
			return go.GetComponent<TooltipSettings>();
		}
	}

	public bool tooltipEnabled = true;
	public Vector2 tooltipOffset = new Vector2(20, 20);
	public float maxTooltipWidth = 200f;
	public float minTooltipWidth = 70f;
	public float minTooltipHeight = 50f;
	public float tooltipNamePositionX;
	public float tooltipNamePositionY;
	public float tooltipDescriptionPositionX;
	public float tooltipDescriptionPositionY;
	public float tooltipItemTypePositionX;
	public float tooltipItemTypePositionY;
	public float tooltipIconPositionX;
	public float tooltipIconPositionY;
	public float lineSpacing;
	public float linePadding;
	public float nameIndentation = 3f;
	public float nameSpacing;
	public float typeIndentation = -3f;
	public float typeSpacing;
	public float descriptionIndentation = 3f;
	public float descriptionSpacing = 10f;
	public float actionIndentation = 3f;
	public float actionSpacing = 10f;
	public FontStyle tooltipNameFontStyle = FontStyle.Normal;
	public FontStyle tooltipDescriptionFontStyle = FontStyle.Normal;
	public FontStyle tooltipTypeFontStyle = FontStyle.Normal;
	public Texture tooltipBackgroundTexture;

	//Foldouts
	public bool nameFoldout = true;
	public bool typeFoldout = true;
	public bool descriptionFoldout = true;
	public bool actionFoldout = true;
}
