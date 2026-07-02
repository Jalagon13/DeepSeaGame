using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UntitledDeepSeaGame
{
    [Serializable]
    public class MenuTabEntry
    {
        [Tooltip("Optional name for the tab. Can be used by code or for organization.")]
        public string TabName;

        [Tooltip("Button that activates this tab when clicked.")]
        public Button TabButton;

        [Tooltip("Menu content to show when this tab is active.")]
        public GameObject TabContent;
    }

    public class MenuTabSystemUI : MonoBehaviour
    {
        [Tooltip("List of tabs in this menu system. Assign as many as you need.")]
        [SerializeField]
        private List<MenuTabEntry> _tabs = new List<MenuTabEntry>();

        [Tooltip("Start with this tab index active on Start. If out of range, the first tab is used.")]
        [SerializeField]
        private int _startIndex = 0;

        public int ActiveTabIndex { get; private set; } = -1;

        private void Awake()
        {
            SetupTabButtons();
        }

        private void Start()
        {
            if (_tabs.Count > 0)
            {
                SetTab(_startIndex);
            }
            
            RefreshTabs();
        }

        private void SetupTabButtons()
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i;
                if (_tabs[i].TabButton != null)
                {
                    _tabs[i].TabButton.onClick.AddListener(() => SetTab(index));
                }
            }
        }

        public void SetTab(int index)
        {
            if (_tabs == null || _tabs.Count == 0)
            {
                return;
            }

            if (index < 0 || index >= _tabs.Count)
            {
                index = 0;
            }

            ActiveTabIndex = index;

            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].TabContent != null)
                {
                    _tabs[i].TabContent.SetActive(i == index);
                }
            }
        }

        public void SetTab(string tabName)
        {
            if (string.IsNullOrEmpty(tabName) || _tabs == null)
            {
                return;
            }

            int index = _tabs.FindIndex(tab => string.Equals(tab.TabName, tabName, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                SetTab(index);
            }
        }

        public void RefreshTabs()
        {
            if (_tabs == null || _tabs.Count == 0)
            {
                return;
            }

            if (ActiveTabIndex < 0 || ActiveTabIndex >= _tabs.Count)
            {
                return;
            }

            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].TabContent != null)
                {
                    _tabs[i].TabContent.SetActive(i == ActiveTabIndex);
                }
            }
        }
    }
}
