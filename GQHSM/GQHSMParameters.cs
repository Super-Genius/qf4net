using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace qf4net
{
    public class GQHSMParameters : IEnumerable<GQHSMVariable>
    {
        private GQHSMVariable[] _params = null;
        GQHSMVariables _globals = GQHSMManager.Instance.Globals;
        public struct ParseType
        {
            public Type pType;
            public Regex TypeExpMatch;
			public bool bIsParseable;

            public ParseType(Type inType, Regex inRegex, bool inParseable)
            {
                pType = inType;
                TypeExpMatch = inRegex;
				bIsParseable = inParseable;
            }

			public ParseType(Type inType, Regex inRegex)
			{
				pType = inType;
				TypeExpMatch = inRegex;
				bIsParseable = true;
			}
		};

        static ParseType[] _parseTypes = {
                                    // int type parsing
                                    new ParseType(typeof(int), new Regex(@"^([\+\-]*\d+)$", RegexOptions.Compiled)),
                                    // float type parsing
                                    new ParseType(typeof(float),  new Regex(@"^([\+\-]*\d*\.\d+)[fF]+$", RegexOptions.Compiled)),
                                    // double type parsing
                                    new ParseType(typeof(double),  new Regex(@"^([\+\-]*\d*\.\d+)[dD]*$", RegexOptions.Compiled)),
                                    // char type parsing
                                    new ParseType(typeof(char),  new Regex(@"^\'(.)\'$", RegexOptions.Compiled)),
                                    // string type parsing
                                    new ParseType(typeof(string),  new Regex(@"^""(.*)""$", RegexOptions.Compiled), false),
                                    // bool type parsing
                                    new ParseType(typeof(bool),  new Regex(@"^((?i)true|false)", RegexOptions.Compiled)),
                                    // default = GQHSMParameters type.
                                    new ParseType(typeof(GQHSMVariables),  new Regex(@"^(.*)", RegexOptions.Compiled))
                                    };

        public GQHSMParameters()
        {
        }

        public GQHSMParameters(object Param)
        {
            _params = new GQHSMVariable[] { new GQHSMVariable(Param)};
        }
        public GQHSMParameters(GQHSMVariable[] Params)
        {
            _params = Params;
        }

        public GQHSMParameters(string paramStr)
        {
            if (paramStr != null)
                ParseParms(paramStr);
        }

        public void ParseParms(string inStr)
        {
            // split var1, var2, var3, var4, ... into array of variable names.
            string[] varStrings = inStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            _params = null;
            if (varStrings.Length > 0)
            {
                // make same number of parameter objects
                _params = new GQHSMVariable[varStrings.Length];

                // parse index to save into object array
                int paramIndex = 0;
                foreach (string varStr in varStrings)
                {
                    // remove whitespace from string
                    string compressedStr = varStr.Trim();
                    Type foundType = null;
					bool bParseable = false; ;
                    foreach (ParseType pt in _parseTypes)
                    {
                        Match match = pt.TypeExpMatch.Match(compressedStr);
                        if (match.Success)
                        {
                            foundType = pt.pType;
							bParseable = pt.bIsParseable;
                            compressedStr = match.Groups[1].Value;
                            break;
                        }
                    }

					GQHSMVariable parsedObject = compressedStr;

                    if (bParseable)
                    {
						object parseObject = foundType.InvokeMember("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, foundType, new object[] { compressedStr });
						if ((parseObject != null) && (parseObject.GetType() != typeof(GQHSMVariable)))
						{
							// wrap the real objects
							parsedObject = new GQHSMVariable(parseObject);
						}
						else
						{
							parsedObject = (GQHSMVariable)parseObject;
						}
					}
					
					_params[paramIndex] = parsedObject;
                    // if a variable is not found or null, set the name to the variable
                    if (_params[paramIndex].Value == null)
                    {
                        _params[paramIndex].Name = compressedStr;
                    }

                    paramIndex++;
                }
            }
        }

        public int Count
        {
            get
            {
                if (_params != null)
                    return _params.Length;
                return 0;
            }
        }

        public object[] ToArray()
        {
            object[] retObjects = null;
            if (_params != null)
            { 
                retObjects = new object[_params.Length];
                int index = 0;
                foreach (GQHSMVariable var in _params)
                {
					retObjects[index++] = var.Value;
                }
            }

            return retObjects;
        }

        public string GetParametersString()
        {
            string retString = "";
            bool first = true;

            if (_params != null)
            {
                foreach (GQHSMVariable gVar in _params)
                {
                    if (!first)
                    {
                        retString += ",";
                    }
                    first = false;
                    if (gVar.Value != null)
                    {
                        retString += gVar.Value.GetType();
                    }
                    else
                    {
                        retString += gVar.Name;
                    }
                }
            }

            return retString;
        }

        public GQHSMVariable this[int index]
        {
            get
            {
                if ((_params != null) && (index < _params.Length))
                {
                    return _params[index];
                }

                return null;
            }

            set
            {
                if ((_params != null) && (_params.Length < index))
                {
                    _params[index].Value = (object)value;
                }

            }
        }

        public IEnumerator<GQHSMVariable> GetEnumerator()
        {
            foreach (GQHSMVariable gVar in _params)
            {
                yield return gVar;
            }
            //return (IEnumerator<GQHSMVariable>)_params.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _params.GetEnumerator();
        }
    }
}
