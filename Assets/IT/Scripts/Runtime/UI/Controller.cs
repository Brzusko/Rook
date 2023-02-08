using System;
using System.Collections;
using System.Collections.Generic;
using IT.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class Controller : MonoBehaviour
{
    [SerializeField] protected UIDocument _uiDocument;

    public virtual ControllerIDs ControllerID => ControllerIDs.NONE;
    
    protected virtual void Awake()
    {
        Hide();
    }

    protected virtual void Show()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.display = DisplayStyle.Flex;
        root.SetEnabled(true);
    }

    protected virtual void Hide()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.display = DisplayStyle.None;
        root.SetEnabled(false);
    }
}
