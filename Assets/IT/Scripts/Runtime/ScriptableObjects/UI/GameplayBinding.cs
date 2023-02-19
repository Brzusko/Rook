using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

namespace IT.ScriptableObjects.UI
{
    [CreateAssetMenu(fileName = "Gameplay Binding", menuName = "UI/Bindings/Gameplay Binding")]
    public class GameplayBinding : ScriptableObject
    {
        public event Action ClearUI;
        public event Action<int, Color> CreatePlayerIndicator;
        public event Action<int> RemovePlayerIndicator;
        public event Action<int, int> UpdatePoints;
        public event Action<int> SetMain;

        public void FireClearUI() => ClearUI?.Invoke();
        public void FireCreatePlayerIndicator(int id, Color color) => CreatePlayerIndicator?.Invoke(id, color);
        public void FireRemovePlayerIndicator(int id) => RemovePlayerIndicator?.Invoke(id);
        public void FireUpdatePoints(int id, int points) => UpdatePoints?.Invoke(id, points);
        public void FireSetMain(int id) => SetMain?.Invoke(id);
    }
}
