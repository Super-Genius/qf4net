using System.Reflection;

namespace qf4net
{
    public delegate void GQHSMDelegate();
    public delegate object GQHSMDelegateOO(object Params);
 
    public class GQHSMHandler
    {
        private object _sourceObject;
        private GQHSMDelegateOO _classDelegateOO = null;
        private GQHSMDelegate _classDelegate = null;

        public GQHSMHandler(object sourceObject, GQHSMDelegateOO sourceDelegate)
        {
            _sourceObject = sourceObject;
            _classDelegateOO = sourceDelegate;
        }

        public GQHSMHandler(object sourceObject, GQHSMDelegate sourceDelegate)
        {
            _sourceObject = sourceObject;
            _classDelegate = sourceDelegate;
        }

        public object Invoke(object Params)
        {
            if (_classDelegate != null)
                return _classDelegate.Method.Invoke(_sourceObject, null);
            if (_classDelegateOO != null)
                return _classDelegateOO.Method.Invoke(_sourceObject, new object[] { Params });

            return null;
        }

    }

    
}
