using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeSpanAttribute : PropertyAttribute
    {
        public TimeSpanAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TimeSpanAttribute))]
    public class TimeSpanDrawer : PropertyDrawer
    {
        const float XPadding = 30f;
        const float YPadding = 5f;
        const float Height = 25f;

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            float helpSize = 110f;
            var attr = attribute as TimeSpanAttribute;
            var value = TimeSpan.FromSeconds(property.intValue);
            var text = FormatText(attr.Format, value);
            Rect pos = new Rect(position.x, position.y, position.width - helpSize - 2f, position.height);
            EditorGUI.PropertyField(pos, property, label, true);
            pos = new Rect(position.xMax - helpSize, position.y, helpSize, position.height);
            EditorGUI.HelpBox(pos, text, MessageType.Info);
        }

        private string FormatText(string format, TimeSpan value)
        {
            format = format
                .Replace(":", @"\:")
                .Replace(".", @"\")
                .Replace(@"0\:", "0:");

            return string.Format(format, value);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}