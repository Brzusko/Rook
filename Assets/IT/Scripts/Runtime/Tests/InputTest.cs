using System;
using System.Collections;
using System.Collections.Generic;
using IT.Input;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _inputReader;

    private void Start()
    {
        _inputReader.EnableGameplayInput();
        _inputReader.EnableCameraInput();
    }

    private void Update()
    {

    }

    private void OnDestroy()
    {
        _inputReader.DisableGameplayInput();
        _inputReader.DisableCameraInput();
    }
}
