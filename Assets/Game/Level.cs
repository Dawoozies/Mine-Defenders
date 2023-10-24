using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public class EventArgs
    {
        public EventArgs(string text, GameObject obj)
        {
            Text = text;
            Obj = obj;
        }
        public string Text;
        public GameObject Obj;
    }
    public class Publisher
    {
        public delegate void EventHandler(object sender, EventArgs args);
        public event EventHandler SampleEvent;
        protected virtual void RaiseEvent(GameObject objectHandler)
        {
            SampleEvent?.Invoke(this, new EventArgs("Text argument", objectHandler));
        }
    }
}
