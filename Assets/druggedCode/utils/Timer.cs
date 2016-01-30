using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using druggedcode;

namespace druggedcode
{
    public class Timer : Singleton<Timer>
    {
        static int callCount;

        bool _initialize;
        List<TimerObject> _list;

        public void Init()
        {
            if (_initialize)
                return;

            callCount = 1;
            _list = new List<TimerObject>();

            _initialize = true;
        }

        TimerObject CreateTimerObject()
        {
            ++callCount;

            TimerObject obj = GameObjectUtil.Create<TimerObject>(callCount + "_", Instance.transform);
            obj.id = callCount;
            return obj;
        }

        void Add(TimerObject obj)
        {
            if (_initialize == false)
                Init();

            _list.Add(obj);

            obj.onComplete += OnTimerObjectComplete;
            obj.Run();
        }

        void OnTimerObjectComplete(TimerObject obj)
        {
            Remove(obj);
        }

        void Remove(TimerObject obj)
        {
            _list.Remove(obj);
            obj.onComplete -= OnTimerObjectComplete;
            Destroy(obj.gameObject);
        }

        static public TimerObject DelayedCall(GameObject go, float delay, string methodName)
        {
            TimerObject obj = Instance.CreateTimerObject();
            obj.DelaeydCall(go, delay, methodName);
            Timer.Instance.Add(obj);
            return obj;
        }

        static public TimerObject DelayedCall(float delay, UnityAction call)
        {
            TimerObject obj = Instance.CreateTimerObject();
            obj.DelayedCall(delay, call);
            Timer.Instance.Add(obj);
            return obj;
        }

        static public void CancelDelayedCall(TimerObject obj)
        {
            Timer.Instance.Remove(obj);
        }
    }

    public class TimerObject : MonoBehaviour
    {
        public enum Mode
        {
            UnityAction,
            SendMessage
        }

        GameObject _target;
        string _methodName;

        UnityAction _call;
        float _delay;

        public Mode mode;
        public int id;

        public event UnityAction<TimerObject> onComplete;

        public TimerObject()
        {

        }

        public void DelayedCall(float delay, UnityAction call)
        {
            mode = Mode.UnityAction;

            _delay = delay;
            _call = call;

        }

        public void DelaeydCall(GameObject go, float delay, string methodName)
        {
            mode = Mode.SendMessage;

            _delay = delay;
            _target = go;
            _methodName = methodName;
        }

        public void Run()
        {
            Invoke("Excute", _delay);
        }

        void Excute()
        {
            switch (mode)
            {
                case Mode.UnityAction:

                    if (_call != null)
                        _call();
                    break;

                case Mode.SendMessage:
                    _target.SendMessage(_methodName);
                    break;
            }

            if (onComplete != null)
                onComplete(this);
        }
    }
}
