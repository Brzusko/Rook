using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.UI
{
    public class LobbyUIController : Controller
    {
        private static readonly string _waiterListName = "WaiterList";
        private static readonly string _messageLabelName = "Message";
        private static readonly string _readyButtonName = "Ready";
        private static readonly string _exitButtonName = "Exit";
        public override ControllerIDs ControllerID => ControllerIDs.LOBBY;
    }
}
