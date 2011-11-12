/// <summary>
/// SerializeXMLClass serializes XML data from Custom Attributes of classes or structs
/// </summary>
/// <remarks>
/// Copyright (c) 2011 Sony Ericsson
/// Author: Kenneth Hurley
/// </remarks>

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using GHQSMXMLDataAttribute;
using qf4net;


public static class SerializeXMLClass
{
	private Dictionary<string, GHQSMXMLDataAttribute> m_XMLTagReaders;
	
	static public void LoadClassDataFromXML(object theObject, XmlReader reader)
	{
		GHQSMXMLDataAttribute[] attributeData = theObject.GetType().GetCustomAttributes(typeof (GHQSMXMLDataAttribute), true);
		if (m_XMLTagReaders == null)
		{
			m_XMLTagReaders = new Dictionary<string, GHQSMXMLDataAttribute>();
		}
		else
		{
			m_XMLTagReaders.Clear();
		}
		
		foreach (GHQSMXMLDataAttribute singleData in attributeData)
		{
			m_XMLTagReaders[singleData.GetXMLTag()] = singleData;
		}
	}
}