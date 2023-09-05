using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementNameAttribute : PropertyAttribute 
{
	public bool m_StartAtOne;
	public bool m_UseAsPrefix;
	public string m_ElementName;

	public ElementNameAttribute(string i_ElementName, bool i_UseAsPrefix = false, bool i_StartAtOne = false)
	{
		m_ElementName = i_ElementName;
		m_UseAsPrefix = i_UseAsPrefix;
		m_StartAtOne = i_StartAtOne;
	}
}
