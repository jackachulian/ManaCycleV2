using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowJoinCode : MonoBehaviour
    {
        const string k_NoCode = "–";

        public ISession Session { get; set; }
        
        [SerializeField]
        TMP_Text m_Text;
        [SerializeField]
        Button m_CopyCodeButton;

        void Start()
        {
            if(m_Text == null)
                m_Text = GetComponentInChildren<TMP_Text>();
            if(m_CopyCodeButton == null)
                m_CopyCodeButton = GetComponentInChildren<Button>();
            
            m_CopyCodeButton.onClick.AddListener(CopySessionCodeToClipboard);
        }

        public void OnSessionJoined()
        {
            m_Text.text = Session?.Code ?? k_NoCode;
            m_CopyCodeButton.interactable = true;
        }

        public void OnSessionLeft()
        {
            m_Text.text = k_NoCode;
            m_CopyCodeButton.interactable = false;
        }

        void CopySessionCodeToClipboard()
        {
            // Deselect the button when clicked.
            EventSystem.current.SetSelectedGameObject(null);
            
            var code = m_Text.text;

            if (Session?.Code == null || string.IsNullOrEmpty(code))
            {
                return;
            }

            // Copy the text to the clipboard.
            GUIUtility.systemCopyBuffer = code;
        }
    }