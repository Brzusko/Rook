using System;
using System.Collections;
using System.Collections.Generic;
using IT;
using IT.Interfaces;
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
        
        ServiceContainer.Get<IUI>().RegisterController(this);
    }

    protected virtual void OnDestroy()
    {
        UnbindUI();
        ServiceContainer.Get<IUI>().UnregisterController(this);
    }
    
    protected virtual void BindUI(){}
    protected virtual void UnbindUI(){}

    public virtual void Show()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.display = DisplayStyle.Flex;
        root.SetEnabled(true);
        
        BindUI();
    }

    public virtual void Hide()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.style.display = DisplayStyle.None;
        root.SetEnabled(false);
        
        UnbindUI();
    }
}
