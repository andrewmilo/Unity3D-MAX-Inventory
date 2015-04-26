using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DurationManager : MonoBehaviour {

	public static List<GameObject> durationGameObjects = new List<GameObject>();

	private ElementAction pItemAction;

	public static void Add(ElementAction itemAction)
	{
		GameObject go = new GameObject(itemAction + " Duration");
		durationGameObjects.Add (go);
		go.AddComponent<DurationManager>().Init (itemAction);
	}

	public void Init(ElementAction itemAction)
	{
		pItemAction = itemAction;
		StartCoroutine (StartDuration());
	}

	private IEnumerator StartDuration() 
	{
		yield return new WaitForSeconds(pItemAction.durationTime);
		
		MonoBehaviour component = (MonoBehaviour) pItemAction.activationObject.GetComponent(pItemAction.selectedComponentName);
		System.Reflection.FieldInfo fieldInfo = component.GetType ().GetField (pItemAction.selectedFieldName);

		if(fieldInfo.GetValue(component) is int)
		{
			int intVal = int.Parse (pItemAction.fieldValue);
			int oldVal = (int) fieldInfo.GetValue(component);
			fieldInfo.SetValue (component, oldVal - intVal);
		}
		else if(fieldInfo.GetValue(component) is float)
		{
			float floatVal = float.Parse (pItemAction.fieldValue);
			float oldVal = (float)fieldInfo.GetValue(component);
			fieldInfo.SetValue (component, oldVal - floatVal);
		}
		else if(fieldInfo.GetValue(component) is double)
		{
			double floatVal = double.Parse (pItemAction.fieldValue);
			double oldVal = (double)fieldInfo.GetValue(component);
			fieldInfo.SetValue (component, oldVal - floatVal);
		}

		DestroyImmediate (gameObject);
	}
}
