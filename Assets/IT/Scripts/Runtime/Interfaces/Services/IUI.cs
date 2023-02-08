using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IUI : IService
    {
        public void RegisterController(Controller controller);
        public void UnregisterController(Controller controller);

        public void ShowUI(Controller controller, bool hideRest = false);
        public void HideUI(Controller controller);
        public void HideAllUI();
    }
}
