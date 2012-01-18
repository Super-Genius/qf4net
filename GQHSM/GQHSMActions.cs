using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace qf4net
{
    public class GQHSMAction
    {
        private GQHSMParameters _params = null;
        private string _name = null;
        MethodInfo _actionHandler = null;
        GQHSM _parentHSM = null;
        GQHSMVariables _globals;

        public GQHSMAction(GQHSM parent, string actionStr)
        {
            _parentHSM = parent;
            string[] NameAndParams = actionStr.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (NameAndParams.Length > 1)
            {
                _params = new GQHSMParameters(NameAndParams[1]);
            }
            else if (NameAndParams.Length == 1)
            {
                _params = new GQHSMParameters();
            }
            _name = NameAndParams[0];

            _globals = GQHSMManager.Instance.Globals;

            if (!_name.StartsWith("^"))
            {
                _actionHandler = _parentHSM.GetMethod(_name, _params);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public GQHSMParameters Params
        {
            get
            {
                return _params;
            }

            set
            {
                _params = value;
            }
        }

        public object InvokeActionHandler()
        {
            if (_actionHandler != null)
            {
                object[] sendParams = null;
                if (_params != null)
                {
                    sendParams = _params.ToArray();
                }
                return _actionHandler.Invoke(_parentHSM.HandlerClass, sendParams);
            }
            else
            {
                return _parentHSM.CallActionHandler(this);
            }

        }
    }

    public class GQHSMActions : IEnumerable<GQHSMAction>
    {
        private GQHSMAction[] _actions;
        private GQHSM _parentHSM;

        public GQHSMAction this[int index]
        {
            get
            {
                if ((_actions != null) && (index < _actions.Length))
                    return _actions[index];

                return null;
            }
        }

        public IEnumerator<GQHSMAction> GetEnumerator()
        {
            return (IEnumerator<GQHSMAction>)(_actions.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        public int Count
        {
            get
            {
                if (_actions != null)
                    return _actions.Length;

                return 0;
            }
        }

        public void Init(GQHSM parentHSM, string[] actionStrings)
        {
            _parentHSM = parentHSM;

            if (actionStrings.Length > 0)
            {
                _actions = new GQHSMAction[actionStrings.Length];
                int paramIndex = 0;
                foreach (string actionStr in actionStrings)
                {
                    if (actionStr.Length > 0)
                    {
                        _actions[paramIndex++] = new GQHSMAction(parentHSM, actionStr);
                    }
                    else
                    {
                        throw new Exception("Invalid zero length action string detected!, Discarding");
                    }
                }
            }
        }

        public GQHSMActions(GQHSM parentHSM, string[] actionsStr)
        {
            Init(parentHSM, actionsStr);
        }

        public GQHSMActions(GQHSM parentHSM, string actionsStr)
        {
            string[] actionStrings = actionsStr.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Init(parentHSM, actionStrings);
        }

        public object InvokeActionHandlers()
        {
            foreach (GQHSMAction action in _actions)
            {
                object retValue = action.InvokeActionHandler();
                if (retValue != null)
                    return retValue;
            }

            return null;
        }

    }
}
