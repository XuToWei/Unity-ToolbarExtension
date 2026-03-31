using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_6000_3_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.Toolbars;
#endif

namespace ToolbarExtension
{
    [InitializeOnLoad]
    internal static class ToolbarHelper
    {
        private static readonly List<(int, Action)> s_LeftToolbarGUI = new List<(int, Action)>();
        private static readonly List<(int, Action)> s_RightToolbarGUI = new List<(int, Action)>();

#if UNITY_6000_3_OR_NEWER
        private static MainToolbarElement CreateCustomElement(Func<VisualElement> createElement)
        {
            var customType = typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom");
            return (MainToolbarElement)Activator.CreateInstance(customType, new object[] { createElement });
        }

        [MainToolbarElement("ToolbarExtension/Left", defaultDockPosition = MainToolbarDockPosition.Left)]
        static MainToolbarElement CreateLeftElement()
        {
            return CreateCustomElement(() =>
            {
                var container = new IMGUIContainer(GUILeft);
                container.style.flexGrow = 1;
                container.style.flexDirection = FlexDirection.RowReverse;
                return container;
            });
        }

        [MainToolbarElement("ToolbarExtension/Right", defaultDockPosition = MainToolbarDockPosition.Right)]
        static MainToolbarElement CreateRightElement()
        {
            return CreateCustomElement(() =>
            {
                var container = new IMGUIContainer(GUIRight);
                container.style.flexGrow = 1;
                container.style.flexDirection = FlexDirection.RowReverse;
                return container;
            });
        }
#endif

        static ToolbarHelper()
        {
#if !UNITY_6000_3_OR_NEWER
            ToolbarCallback.OnToolbarGUILeft = GUILeft;
            ToolbarCallback.OnToolbarGUIRight = GUIRight;
#endif
            Type attributeType = typeof(ToolbarAttribute);

            foreach (var methodInfo in TypeCache.GetMethodsWithAttribute<ToolbarAttribute>())
            {
                var attributes = methodInfo.GetCustomAttributes(attributeType, false);
                if (attributes.Length > 0)
                {
                    ToolbarAttribute attribute = (ToolbarAttribute)attributes[0];
                    if (attribute.Side == OnGUISide.Left)
                    {
                        s_LeftToolbarGUI.Add((attribute.Priority, delegate
                        {
                            methodInfo.Invoke(null, null);
                        }));
                        continue;
                    }
                    if (attribute.Side == OnGUISide.Right)
                    {
                        s_RightToolbarGUI.Add((attribute.Priority, delegate
                        {
                            methodInfo.Invoke(null, null);
                        }));
                        continue;
                    }
                }
            }
            s_LeftToolbarGUI.Sort((tuple1, tuple2) => tuple1.Item1 - tuple2.Item1);
            s_RightToolbarGUI.Sort((tuple1, tuple2) => tuple2.Item1 - tuple1.Item1);
        }

        static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (var handler in s_LeftToolbarGUI)
            {
                handler.Item2();
            }

            GUILayout.EndHorizontal();
        }

        static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in s_RightToolbarGUI)
            {
                handler.Item2();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
