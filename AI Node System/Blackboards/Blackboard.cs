using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;
using System.Collections;
using System.Collections.Generic;

namespace NodeEditorFramework.AI
{
    public class Blackboard : MonoBehaviour
    {
        
        public Dictionary<string, object> dict;
        private Dictionary<string, List<BaseBlackboardObserver>> observers;

        void Awake()
        {
            dict = new Dictionary<string, object>();

            // Default values
            dict["Vector3.negativeInfinity"] = Vector3.negativeInfinity;
            dict["float.negativeInfinity"] = float.NegativeInfinity;
            observers = new Dictionary<string, List<BaseBlackboardObserver>>();
        }

        public void AddObserver(string key, BaseBlackboardObserver observer)
        {
            if(!observers.ContainsKey(key))
            {
                observers.Add(key, new List<BaseBlackboardObserver>());
                observers[key].Add(observer);
            } else if(!observers[key].Contains(observer))
            {
                observers[key].Add(observer);
            }
        }

        public void RemoveObserver(string key, BaseBlackboardObserver observer)
        {
            if(observers.ContainsKey(key))
                observers[key].Remove(observer);
        }

        private void Notify(BaseBlackboardObserver observer, bool condition)
        {
            observer.ReceiveNotification(condition);
        }

        public void SetValue(string key, object value)
        {
            if(dict==null)
                dict = new Dictionary<string, object>();
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
                if (observers.TryGetValue(key, out List<BaseBlackboardObserver> os))
                    for(int i = 0; i<os.Count; i++)
                    {
                        Notify(os[i], os[i].CheckCondition());
                    }
                    
            }
            else
            {
                bool shouldNotify = false;
                if (observers.TryGetValue(key, out List<BaseBlackboardObserver> os))
                {
                    for (int i = os.Count - 1; i >= 0; i--)
                    {
                        if (os[i].notify_rule == 0)
                        {
                            // Notify Rule is OnValueChange
                            object temp_value = dict[key];
                            if (!temp_value.Equals(value))
                                Notify(os[i], os[i].CheckCondition());
                        }
                        else
                        {
                            // Notify Rule is OnResultChange
                            bool temp_condition = os[i].CheckCondition();
                            dict[key] = value;
                            bool condition = os[i].CheckCondition();
                            if (temp_condition != condition)
                                Notify(os[i], condition);
                        }
                    }
                }
                dict[key] = value;
                
            }
        }

        public Type GetKeyType(string key)
        {
            try
            {
                return dict[key].GetType();
            } catch(KeyNotFoundException e)
            {
                throw (e);
            }
                
        }

        public bool HasValue(string key)
        {
            return dict.ContainsKey(key);
        }

        public bool IsTrue(string key)
        {
            if (!dict.ContainsKey(key))
                return false;
            return (bool)dict[key];
        }

        public bool IsFalse(string key)
        {
            return !IsTrue(key);
        }

        public bool IsEqual(string key, object value)
        {
            return dict[key].Equals(value);
        }

        public bool IsGreater(string key, object value)
        {
            IComparable o1 = dict[key] as IComparable;
            IComparable o2 = value as IComparable;
            return o1.CompareTo(o2) == 1;
        }

        public bool IsLesser(string key, object value)
        {
            IComparable o1 = dict[key] as IComparable;
            IComparable o2 = value as IComparable;
            return o1.CompareTo(o2) == -1;
        }

        public bool IsGreaterEq(string key, object value)
        {
            IComparable o1 = dict[key] as IComparable;
            IComparable o2 = value as IComparable;
            int c = o1.CompareTo(o2);
            return c == 0 || c == 1;
        }

        public bool IsLesserEq(string key, object value)
        {
            IComparable o1 = dict[key] as IComparable;
            IComparable o2 = value as IComparable;
            int c = o1.CompareTo(o2);
            return c == 0 || c == -1;
        }

        public T GetValue<T>(string key)
        {
            if (dict.ContainsKey(key))
                return (T)(dict[key]);
            return default(T);
        }

        public bool RemoveKey(string key)
        {
            return dict.Remove(key);
        }
    }
}
