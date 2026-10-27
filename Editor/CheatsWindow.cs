using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ji2.Cheats.Editor
{
    public sealed class CheatsWindow : EditorWindow
    {
        [SerializeField] private string _search = string.Empty;
        private Vector2 _scroll;
        private string _message;
        private bool _failed;
        [MenuItem("Tools/Development/Cheats")]
        public static void Open() => GetWindow<CheatsWindow>("Cheats");
        private void OnEnable()
        {
            minSize = new Vector2(420, 480);
            CheatRegistry.Shared.Changed += Repaint;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }
        private void OnDisable()
        {
            CheatRegistry.Shared.Changed -= Repaint;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
        }
        private void PlayModeChanged(PlayModeStateChange state) => Repaint();
        private void OnInspectorUpdate() => Repaint();
        public void SetSearch(string query) { _search = query ?? string.Empty; Repaint(); }
        public int VisibleCommandCount => CheatRegistry.Shared.Providers.Sum(p => p.Commands.Count(Matches));
        private bool Matches(CheatDefinition command) => string.IsNullOrWhiteSpace(_search) ||
            (command.Title + " " + command.Description + " " + command.Category + " " + command.Id)
                .IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;

        private void OnGUI()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Cheats", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(48))) _search = string.Empty;
            }
            EditorGUILayout.LabelField(VisibleCommandCount + " commands", EditorStyles.miniLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var providers = CheatRegistry.Shared.Providers;
            if (providers.Count == 0)
                EditorGUILayout.HelpBox("No cheat context is available. Open a project with a registered cheat adapter.", MessageType.Info);
            foreach (var provider in providers)
            {
                EditorGUILayout.LabelField(provider.DisplayName, EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(provider.Context, MessageType.None);
                foreach (var group in provider.Commands.Where(Matches).GroupBy(c => c.Category))
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(group.Key, EditorStyles.miniBoldLabel);
                    foreach (var command in group)
                    {
                    var state = CheatRegistry.Shared.GetState(provider.Id, command.Id);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                EditorGUILayout.LabelField(command.Title, EditorStyles.boldLabel);
                                EditorGUILayout.LabelField(command.Description, EditorStyles.wordWrappedMiniLabel);
                            }
                            using (new EditorGUI.DisabledScope(!state.Enabled))
                                if (GUILayout.Button(new GUIContent(state.ActionLabel, state.DisabledReason), GUILayout.Width(104), GUILayout.Height(32)))
                                {
                                    var result = CheatRegistry.Shared.Execute(provider.Id, command.Id);
                                    _message = result.Message; _failed = !result.Success;
                                    Repaint();
                                }
                        }
                        if (!state.Enabled) EditorGUILayout.LabelField(state.DisabledReason, EditorStyles.wordWrappedMiniLabel);
                    }
                    }
                }
            }
            if (providers.Count > 0 && VisibleCommandCount == 0) EditorGUILayout.HelpBox("No commands match this search.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(_message)) EditorGUILayout.HelpBox(_message, _failed ? MessageType.Error : MessageType.Info);
        }
    }
}
