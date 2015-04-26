using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;

[RequireComponent(typeof(Text))]
[ExecuteInEditMode]
public class TextObject : MonoBehaviour
{
	public Text textComponent;
	public string text = "";
	public GameObject selectedGameObject;
	public int selectedField;
	public string selectedFieldName;
	public int selectedMonoBehaviourIndex;
	public MonoBehaviour selectedMonoBehaviour;
	public string selectedMonoBehaviourName;
	public string format = ": ";
	public bool foldout;
	private FieldInfo pFieldInfo;

	void Reset()
	{
		textComponent = GetComponent<Text>();
		textComponent.rectTransform.position = new Vector2(0,0);
	}

	void Update()
	{
		textComponent.rectTransform.sizeDelta = new Vector2(textComponent.preferredWidth, textComponent.preferredHeight);

		if(selectedGameObject != null)
		{
			selectedMonoBehaviour = (MonoBehaviour) selectedGameObject.GetComponent(selectedMonoBehaviourName);

			if(selectedMonoBehaviour != null)
			{
				if(pFieldInfo == null)
					pFieldInfo = selectedMonoBehaviour.GetType ().GetField (selectedFieldName);

				if(selectedField > 0)
					textComponent.text = char.ToUpper (selectedFieldName[0]) + selectedFieldName.Substring (1) + format + pFieldInfo.GetValue (selectedMonoBehaviour).ToString ();
			}
		}
	}
}
