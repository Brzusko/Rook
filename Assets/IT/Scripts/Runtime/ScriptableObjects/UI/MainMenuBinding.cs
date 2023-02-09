using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.ScriptableObjects.UI
{
    [CreateAssetMenu(fileName = "Main Menu Binding", menuName = "UI/Bindings/Main Menu Binding")]
    public class MainMenuBinding : ScriptableObject
    {
        public event Action PlayButtonClicked;
        public event Action ExitButtonClicked;

        public void InvokePlayButton() => PlayButtonClicked?.Invoke();
        public void InvokeExitButton() => ExitButtonClicked?.Invoke();
    }
}
