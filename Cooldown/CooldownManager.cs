using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class CooldownManager : MonoBehaviour
{
	public static List<object> objectsOnCooldown = new List<object>();
	public List<InventoryElement> exclusions = new List<InventoryElement> ();

	public float remainingTime = -1;
	private ICooldown pItemCooldown;
	private CooldownSettings pCDS;
	private List<GameObject> cooldownGameObjects = new List<GameObject>();

	public void Init(ICooldown itemCooldown, CooldownSettings CDS)
	{
		pCDS = CDS;
		pItemCooldown = itemCooldown;
		StartCoroutine (StartCooldown(itemCooldown, CDS));
	}

	private void Update()
	{
		if(remainingTime == -1)
			remainingTime = pCDS.cooldownTime;
		remainingTime -= Time.deltaTime;

		foreach(Slot s in Slot.allSlots)
		{
			if(s != null)
			{
				if(s.inventoryElement != null)
				{
					if(s.inventoryElement.id > -1)
					{
						InventoryElement temp = pItemCooldown as InventoryElement;

						if(temp != null)
						{
							if(s.inventoryElement.id == temp.id)
								DrawCooldown (s);
						}
					}
					else if(s.transform.FindChild ("Cooldown") != null)
						DestroyImmediate (s.transform.FindChild ("Cooldown").gameObject);

					ElementType e = pItemCooldown as ElementType;

					if(e != null)
					{
						if(s.inventoryElement.type == e || e.isAncestorOf (s.inventoryElement.type))
							DrawCooldown (s);
					}

					if(s.inventoryElement.actions.Contains (pItemCooldown as ElementAction))
						DrawCooldown (s);
				}
				else if(s.transform.FindChild ("Cooldown") != null)
					DestroyImmediate (s.transform.FindChild ("Cooldown").gameObject);

				if(remainingTime <= 0 && s.transform.FindChild ("Cooldown") != null)
					DestroyImmediate (s.transform.FindChild ("Cooldown").gameObject);
			}
		}
	}

	private void DrawCooldown(Slot s)
	{
		if (exclusions.Contains (s.inventoryElement.prototype))
			return;

		//If an animation is to be drawn
		if(pCDS.drawCooldownAnimation)
		{
			//Create Cooldown Image & Text
			if(s.cooldownGameObject == null)
			{
				s.cooldownGameObject = new GameObject("Cooldown");
				
				if(!cooldownGameObjects.Contains (s.cooldownGameObject))
					cooldownGameObjects.Add (s.cooldownGameObject);
				
				s.cooldownGameObject.hideFlags = HideFlags.HideInHierarchy;
				s.cooldownImage = s.cooldownGameObject.AddComponent <Image>();
				s.cooldownImage.fillAmount = 1 * (remainingTime/pCDS.cooldownTime);
				s.cooldownImage.type = UnityEngine.UI.Image.Type.Filled;
				s.cooldownImage.fillClockwise = false;
				s.cooldownImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
				s.cooldownImage.fillOrigin = 2;
				
				s.cooldownImage.rectTransform.position = s.rectTransform.position;
				s.cooldownImage.rectTransform.sizeDelta = s.rectTransform.sizeDelta;
				
				if(pCDS.drawCooldownTimer)
				{
					s.cooldownTextGameObject = new GameObject("Counter");
					s.cooldownText = s.cooldownTextGameObject.AddComponent <Text>();
					if(s.cooldownText.font == null)
						s.cooldownText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
					s.cooldownText.alignment = TextAnchor.MiddleCenter;
					s.cooldownText.transform.SetParent (s.cooldownGameObject.transform);
					s.cooldownText.rectTransform.position = s.cooldownImage.rectTransform.position;
					s.cooldownText.rectTransform.sizeDelta = s.cooldownImage.rectTransform.sizeDelta;
				}
				
				s.cooldownGameObject.transform.SetParent (s.transform);
				s.cooldownGameObject.transform.SetAsLastSibling ();
				
				s.cooldownImage.sprite = Resources.Load<Sprite>("Cooldown");
			}
			
			s.cooldownImage.rectTransform.position = s.rectTransform.position;
			
			if(s.cooldownText != null)
			{
				s.cooldownText.rectTransform.position = s.cooldownImage.rectTransform.position;
				
				decimal tempDecimal = decimal.Parse (remainingTime.ToString ());
				if(tempDecimal != null)
					s.cooldownText.text = decimal.Round (tempDecimal, 2).ToString ();
			}
			
			if(s.cooldownImage != null)
				s.cooldownImage.fillAmount -= (1f/pCDS.cooldownTime) * Time.deltaTime;
			
			if(s.cooldownGameObject != null)
			{
				if(s != null)
				{
					if(!s.cooldownGameObject.activeSelf)
						s.cooldownGameObject.SetActive (true);
					
					if(s.cooldownGameObject.transform.parent != s.transform)
						s.cooldownGameObject.transform.SetParent (s.transform, false);
				}
				else
					s.cooldownGameObject.SetActive (false);
			}

			if(remainingTime < 0f)
				Destroy (this.gameObject);
		}
	}

	private IEnumerator StartCooldown(ICooldown cd, CooldownSettings cds)
	{
		cd.OnCooldown = true;

		yield return new WaitForSeconds (cds.cooldownTime);

		cd.OnCooldown = false;
	}
}
