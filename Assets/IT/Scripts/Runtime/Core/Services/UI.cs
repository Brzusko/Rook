using System;
using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using UnityEngine;

namespace IT.UI
{
    public class UI : MonoBehaviour, IUI
    {
        private Dictionary<ControllerIDs, Controller> _controllers;
        public Type Type => typeof(IUI);
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _controllers = new Dictionary<ControllerIDs, Controller>();
        }

        private void Start()
        {
            ServiceContainer.RegisterService<IUI>(this);
        }

        public void RegisterController(Controller controller)
        {
            if (_controllers.ContainsKey(controller.ControllerID))
            {
                Destroy(controller.gameObject);
                return;
            }
            
            _controllers.Add(controller.ControllerID, controller);
        }

        public void UnregisterController(Controller controller)
        {
            if(!_controllers.ContainsKey(controller.ControllerID))
                return;
            
            if(_controllers[controller.ControllerID] != controller)
                return;

            _controllers.Remove(controller.ControllerID);
        }

        public void ShowUI(ControllerIDs id, bool hideRest = false)
        {
            if(!_controllers.ContainsKey(id))
                return;
            
            _controllers[id].Show();
            
            if(!hideRest)
                return;

            foreach (Controller controller in _controllers.Values)
            {
                if(controller.ControllerID == id)
                    continue;
                
                controller.Hide();
            }
        }

        public void HideUI(ControllerIDs id)
        {
            if(!_controllers.ContainsKey(id))
                return;
            
            _controllers[id].Hide();
        }
        
        public void HideAllUI()
        {
            foreach (Controller controller in _controllers.Values)
            {
                controller.Hide();
            }
        }
    }
}
