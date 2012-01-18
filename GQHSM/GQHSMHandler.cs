using System.Reflection;

namespace qf4net
{
    public delegate object GQHSMDelegate();
    public delegate object GQHSMDelegateOO(GQHSMVariables Params);
 
    public class GQHSMHandler
    {
        private object _sourceObject;
        private GQHSMVariables _globals = GQHSMManager.Instance.Globals;
        private GQHSMDelegate _classDelegate = null;
        private GQHSMDelegateOO _classDelegateOO = null;

        public GQHSMHandler(object sourceObject, GQHSMDelegate sourceDelegate)
        {
            _sourceObject = sourceObject;
            _classDelegate = sourceDelegate;
        }

        public GQHSMHandler(object sourceObject, GQHSMDelegateOO sourceDelegate)
        {
            _sourceObject = sourceObject;
            _classDelegateOO = sourceDelegate;
        }

        public object Invoke(GQHSMParameters Params)
        {
            GQHSMParameters _params = Params;
            object retValue = null;
            if (_params == null)
                _params = new GQHSMParameters();

            if (_classDelegateOO != null)
            {
                retValue = _classDelegateOO.Method.Invoke(_sourceObject, new object[] { _params });
            }

            if (_classDelegate != null)
            {
                _classDelegate.Method.Invoke(_sourceObject, new object[] { null });
            }

            return retValue;
        }

    }

    
}
