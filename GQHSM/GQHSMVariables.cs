using System;
using System.Collections.Generic;
using System.Reflection;


namespace qf4net
{

    public class GQHSMVariable
    {
        private object _value;
        private string _name;
        private object _handlerClass;
        private FieldInfo _fieldInfo;
		private PropertyInfo _propertyInfo;
		private bool _bIsInDirect;

        public object Value
        {
            get
            {
				if (_bIsInDirect)
                {
					if (_propertyInfo != null)
					{
						return (_propertyInfo.GetValue(_handlerClass, null));
					}
                    
					if (_fieldInfo != null)
                    {
                        return (_fieldInfo.GetValue(_handlerClass));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return _value;
                }
            }

            set
            {
				if (_bIsInDirect)
                {
					if (_propertyInfo != null)
					{
						_propertyInfo.SetValue(_handlerClass, value, null);
					}
					else if (_fieldInfo != null)
					{
						_fieldInfo.SetValue(_handlerClass, value);
					}
					else
					{
						throw new NullReferenceException();
					}
				}
                else
                {
                    _value = value;
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public GQHSMVariable(object src)
        {
            // copy constructor?
            if (src is GQHSMVariable)
            {
				Copy((GQHSMVariable)src);
            }
            else
            {
                _value = src;
				_bIsInDirect = false;
				_fieldInfo = null;
				_propertyInfo = null;
            }
        }

		public void Copy(GQHSMVariable src)
		{
			_value = src._value;
			_name = src._name;
			_handlerClass = src._handlerClass;
			_fieldInfo = src._fieldInfo;
			_propertyInfo = src._propertyInfo;
			_bIsInDirect = src._bIsInDirect;
		}

        public void SetReference(object handlerClass, FieldInfo fieldInfo)
        {
            if (fieldInfo != null)
            {
                _name = fieldInfo.Name;
            }
            _handlerClass = handlerClass;
            _fieldInfo = fieldInfo;
			_propertyInfo = null;
			_bIsInDirect = true;
        }

		public void SetProperty(object handlerClass, PropertyInfo propInfo)
		{
			if (propInfo != null)
			{
				_name = propInfo.Name;
			}
			_handlerClass = handlerClass;
			_propertyInfo = propInfo;
			_fieldInfo = null;
			_bIsInDirect = true;
		}

		// implicit/explicit casts to GQHSMVariable
        // bool case
        static public implicit operator bool(GQHSMVariable src)
        {
            try
            {
                return (bool)src.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
                ;
            }
        }

        static public implicit operator GQHSMVariable(bool src)
        {
            try
            {
                return new GQHSMVariable(src);
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        // int case
        static public implicit operator int(GQHSMVariable src)
        {
            try
            {
                return (int)src.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }


        // int case
        static public implicit operator GQHSMVariable(int src)
        {
            try
            {
                return new GQHSMVariable(src);
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        // float case
        static public implicit operator float(GQHSMVariable src)
        {
            try
            {
                return (float)src.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        static public implicit operator GQHSMVariable(float src)
        {
            try
            {
                return new GQHSMVariable(src);
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        // double case
        static public implicit operator double(GQHSMVariable src)
        {
            try
            {
                return (double)src.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        static public implicit operator GQHSMVariable(double src)
        {
            try
            {
                return new GQHSMVariable(src);
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        // string case
        static public implicit operator string(GQHSMVariable src)
        {
            try
            {
                return (string)src.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        static public implicit operator GQHSMVariable(string src)
        {
            try
            {
                return new GQHSMVariable(src);
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        // char case
        static public implicit operator char(GQHSMVariable src)
        {
            try
            {
                return (char)src.Value;
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

        static public implicit operator GQHSMVariable(char src)
        {
            try
            {
                return new GQHSMVariable(src);
            }
            catch (Exception ex)
            {
                throw new Exception("GQHSMVariables Exception: ", ex);
            }
        }

    }

    public class GQHSMVariables
    {
        // public variables
        private Dictionary<string, GQHSMVariable> _variables = new Dictionary<string, GQHSMVariable>();

        public static object Parse(string varName)
        {
            return GQHSMManager.Instance.Globals[varName];
        }

        public virtual GQHSMVariable this[string key]
        {
            get
            {
                if (!_variables.ContainsKey(key))
                {
                    GQHSMVariable newVar = new GQHSMVariable(null);
                    newVar.SetReference(null, null);
                    _variables.Add(key, newVar);
                }

                return _variables[key];
            }

            set
            {
                if (!_variables.ContainsKey(key))
                {
                    _variables.Add(key, value);
                }
                else
                {
                    _variables[key] = value;
                }
            }
        }
    }
}
