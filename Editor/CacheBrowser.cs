using System;
using Cache.Core;
using Cache.Data;
using UnityEditor;
using UnityEngine;

namespace Cache
{
    public abstract class CacheBrowser : EditorWindow
    {
        private Vector2 scrollPos;

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                var cache = GetCurrentCache();
                var elements = cache.CachedData;

                EditorGUILayout.LabelField($"Elements: {elements.Count}",
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter },
                    GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var elem in elements)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(elem.Key, GUILayout.ExpandWidth(true));
                    ShowObject(elem.Value);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                Repaint();
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Play to see cache elements!",
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndVertical();
                Repaint();
            }
        }

        private void ShowObject(Cachable value)
        {
            if (value == null) return;

            var data = value.GetObject();
            if (data == null) return;

            var style = new GUIStyle(GUI.skin.GetStyle("label"))
            {
                wordWrap = true
            };

            if (data is UnityEngine.Object engineObj)
            {
                var name = engineObj.name;
                var diff = value.Date - DateTime.Now;
                if (diff.Value.TotalSeconds > 0)
                    EditorGUILayout.LabelField($"Left time: {diff}", style);

                EditorGUILayout.LabelField(name, style);
                EditorGUILayout.ObjectField(engineObj, data.GetType(), false);
                if (data is Sprite spr)
                {
                    TextureField(spr.texture);
                }
            }

            if (value is ICountable count)
            {
                var refCount = count.ReferenceCount;
                EditorGUILayout.LabelField($"Reference Count: {refCount}", GUILayout.ExpandWidth(true));
            }
        }

        private void TextureField(Texture2D texture)
        {
            GUILayout.BeginVertical();
            EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndVertical();
        }

        public abstract ObjectCache GetCurrentCache();
    }
}
