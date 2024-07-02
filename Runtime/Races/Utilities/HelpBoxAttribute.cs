
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LionStudios.Suite.Leaderboards.Fake
{
    public enum HelpBoxMessageType
    {
        None,
        Info,
        Warning,
        Error
    }

    public class HelpBoxAttribute : PropertyAttribute
    {
        public string text;
        public HelpBoxMessageType messageType;

        public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None)
        {
            this.text = text;
            this.messageType = messageType;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer
    {
        private GUIStyle helpBoxStyle;
        
        public override float GetHeight()
        {
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return base.GetHeight();
            if (helpBoxStyle == null) return base.GetHeight();
            return Mathf.Max(40f, helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.text), EditorGUIUtility.currentViewWidth) + 4f);
        }

        public override void OnGUI(Rect position)
        {
            if (helpBoxStyle == null)
            {
                helpBoxStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;
                helpBoxStyle.wordWrap = true;
            }
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return;
            EditorGUI.HelpBox(position, helpBoxAttribute.text, GetMessageType(helpBoxAttribute.messageType));
        }

        private MessageType GetMessageType(HelpBoxMessageType helpBoxMessageType)
        {
            switch (helpBoxMessageType)
            {
                default:
                case HelpBoxMessageType.None: return MessageType.None;
                case HelpBoxMessageType.Info: return MessageType.Info;
                case HelpBoxMessageType.Warning: return MessageType.Warning;
                case HelpBoxMessageType.Error: return MessageType.Error;
            }
        }
    }
#endif
}