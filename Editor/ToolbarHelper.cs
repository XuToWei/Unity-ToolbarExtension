using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolbarExtension
{
    [InitializeOnLoad]
    internal static class ToolbarHelper
    {
        private static readonly List<(int, Action)> s_LeftToolbarGUI = new List<(int, Action)>();
        private static readonly List<(int, Action)> s_RightToolbarGUI = new List<(int, Action)>();

        static ToolbarHelper()
        {
#if UNITY_6000_3_OR_NEWER
            var customType = typeof(MainToolbarButton).Assembly.GetType("UnityEditor.Toolbars.MainToolbarCustom");
            Activator.CreateInstance(customType, new object[]
            {
                () =>
                {
                    var container = new IMGUIContainer(() => ToolbarHelper.GUILeft());
                    container.style.flexGrow = 1;
                    container.style.flexDirection = FlexDirection.Row;
                    return container;
                },
                () =>
                {
                    var container = new IMGUIContainer(() => ToolbarHelper.GUIRight());
                    container.style.flexGrow = 1;
                    container.style.flexDirection = FlexDirection.Row;
                    return container;
                }
            });
#else
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

        internal static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (var handler in s_LeftToolbarGUI)
            {
                handler.Item2();
            }

            GUILayout.EndHorizontal();
        }

        internal static void GUIRight()
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
