using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TJNK.Farwander.Systems.UI
{
    public class CombatLog : MonoBehaviour
    {
        public static CombatLog Instance { get; private set; }

        [Header("Wiring")]
        public Text logText;

        [Header("Settings")]
        public int maxLines = 50;

        private readonly List<string> _lines = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Log(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            _lines.Add(msg);
            if (_lines.Count > maxLines) _lines.RemoveAt(0);
            if (logText)
            {
                logText.text = string.Join("\n", _lines);
            }
        }
    }
}