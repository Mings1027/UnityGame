using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ElementNameAttribute))]
public class ElementNameAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect i_Position, SerializedProperty i_Property, GUIContent i_Label)
	{
		ElementNameAttribute elementName = (attribute as ElementNameAttribute);
		EditorGUI.BeginProperty(i_Position, i_Label, i_Property);
		int spaceIndex = i_Label.text.LastIndexOf(" ");
		string numberText = i_Label.text.Remove(0, spaceIndex);
		if (elementName.m_StartAtOne)
		{
			if (int.TryParse(numberText, out spaceIndex))
			{
				numberText = (spaceIndex+1).ToString();
			}
		}
		if(elementName.m_UseAsPrefix)
		{
			i_Label.text = (numberText + ") " + elementName.m_ElementName).TrimEnd();
		}
		else
		{
			i_Label.text = elementName.m_ElementName + numberText;
		}
		EditorGUI.PropertyField(i_Position, i_Property, i_Label, true);
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

}
