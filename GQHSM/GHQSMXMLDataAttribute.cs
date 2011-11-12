

using System;
using System.IO;
using System.Reflection;
using System.Xml;

[AttributeUsage(AttributeTargets.Field)]
public class GHQSMXMLDataAttribute : Attribute
{
	private string m_XMLTag;
	
	public GHQSMXMLDataAttribute(string XMLTag)
	{
		m_XMLTag = XMLTag;
	}
	
	public virtual object ReadField(XMLReader xmlReader)
	{
		return xmlReader.ReadElementContentAsObject();
	}
	
	public string GetXMLTag()
	{
		return m_XMLTag;
	}
	
}


