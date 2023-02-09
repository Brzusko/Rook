using System.Collections;
using System.Collections.Generic;
using IT.UI;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IUI : IService
    {
        public void RegisterController(Controller controller);
        public void UnregisterController(Controller controller);

        public void ShowUI(ControllerIDs id, bool hideRest = false);
        public void HideUI(ControllerIDs id);
        public void HideAllUI();
    }
}
