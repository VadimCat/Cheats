using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ji2.Cheats.Panel
{
    /// <summary>Code-built uGUI overlay behind <see cref="CheatPanel"/>. Uses the legacy built-in font so it needs no game assets.</summary>
    internal sealed class CheatPanelRoot : MonoBehaviour
    {
        private sealed class Row
        {
            public string Provider, Command;
            public Button Button;
            public Text Action, Reason;
        }

        private static readonly Color Card = new Color(0.11f, 0.12f, 0.17f, 0.97f);
        private static readonly Color RowColor = new Color(1f, 1f, 1f, 0.06f);
        private static readonly Color Ink = new Color(0.95f, 0.96f, 1f);
        private static readonly Color Muted = new Color(0.65f, 0.68f, 0.78f);
        private static readonly Color Accent = new Color(0.45f, 0.78f, 1f);
        private static readonly Color Go = new Color(0.18f, 0.72f, 0.42f);
        private static readonly Color Off = new Color(0.3f, 0.32f, 0.38f);
        private static readonly Color Error = new Color(1f, 0.45f, 0.45f);

        private readonly List<Row> _rows = new List<Row>();
        private Font _font;
        private GameObject _launcher;
        private GameObject _panel;
        private RectTransform _content;
        private Text _feedback;
        private float _nextRefresh;
        private bool _dirty;

        public bool IsOpen => _panel != null && _panel.activeSelf;

        public void Build(Vector2 launcherAnchor)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            var launcher = Box("Launcher", transform, new Color(0.08f, 0.09f, 0.13f, 0.6f));
            launcher.anchorMin = launcher.anchorMax = launcher.pivot = launcherAnchor;
            launcher.anchoredPosition = new Vector2(launcherAnchor.x < 0.5f ? 12 : -12, 0);
            launcher.sizeDelta = new Vector2(120, 68);
            Label(launcher, "DEV", 30, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, true);
            launcher.gameObject.AddComponent<Button>().onClick.AddListener(() => SetOpen(true));
            _launcher = launcher.gameObject;

            var dim = Box("Panel", transform, new Color(0, 0, 0, 0.55f));
            Stretch(dim, Vector2.zero, Vector2.zero);
            _panel = dim.gameObject;
            var card = Box("Card", dim, Card);
            Stretch(card, new Vector2(36, 120), new Vector2(-36, -140));
            var header = Node("Header", card);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1);
            header.sizeDelta = new Vector2(0, 120);
            header.anchoredPosition = Vector2.zero;
            var title = Label(header, "Cheats", 48, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, true);
            Stretch(title.rectTransform, new Vector2(36, 0), new Vector2(-260, 0));
            var close = Box("Close", header, Off);
            close.anchorMin = close.anchorMax = close.pivot = new Vector2(1, 0.5f);
            close.anchoredPosition = new Vector2(-28, 0);
            close.sizeDelta = new Vector2(200, 84);
            Label(close, "Close", 34, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, true);
            close.gameObject.AddComponent<Button>().onClick.AddListener(() => SetOpen(false));

            var feedback = Label(card, string.Empty, 28, Muted, FontStyle.Normal, TextAnchor.MiddleLeft, true);
            var fr = feedback.rectTransform;
            fr.anchorMin = new Vector2(0, 0);
            fr.anchorMax = new Vector2(1, 0);
            fr.pivot = new Vector2(0.5f, 0);
            fr.sizeDelta = new Vector2(-72, 80);
            fr.anchoredPosition = new Vector2(0, 12);
            _feedback = feedback;

            var view = Node("Scroll", card);
            Stretch(view, new Vector2(24, 100), new Vector2(-24, -124));
            view.gameObject.AddComponent<RectMask2D>();
            var hit = view.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
            _content = Node("Content", view);
            _content.anchorMin = new Vector2(0, 1);
            _content.anchorMax = new Vector2(1, 1);
            _content.pivot = new Vector2(0.5f, 1);
            _content.sizeDelta = Vector2.zero;
            var layout = _content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12;
            layout.padding = new RectOffset(8, 8, 8, 24);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            _content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = view;
            scroll.content = _content;

            _panel.SetActive(false);
            CheatRegistry.Shared.Changed += MarkDirty;
        }

        public void SetOpen(bool open)
        {
            if (_panel == null || _panel.activeSelf == open) return;
            if (open && EventSystem.current == null)
                Debug.LogWarning("[Cheats] No EventSystem in the scene; the cheat panel cannot receive clicks.");
            _panel.SetActive(open);
            _launcher.SetActive(!open);
            if (open)
            {
                _feedback.text = string.Empty;
                Rebuild();
            }
            CheatPanel.NotifyOpenChanged(open);
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (_dirty)
            {
                _dirty = false;
                Rebuild();
                return;
            }
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.4f;
            RefreshStates();
        }

        private void OnDestroy()
        {
            CheatRegistry.Shared.Changed -= MarkDirty;
            CheatPanel.Forget(this);
        }

        private void MarkDirty() => _dirty = true;

        private void Rebuild()
        {
            _dirty = false;
            _rows.Clear();
            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                var child = _content.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            var providers = CheatRegistry.Shared.Providers;
            if (providers.Count == 0)
            {
                Label(_content, "No cheat context is available.", 32, Muted, FontStyle.Normal, TextAnchor.MiddleLeft, false);
                return;
            }
            foreach (var provider in providers)
            {
                Label(_content, provider.DisplayName, 40, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, false);
                if (!string.IsNullOrEmpty(provider.Context))
                    Label(_content, provider.Context, 28, Muted, FontStyle.Normal, TextAnchor.MiddleLeft, false);
                foreach (var group in provider.Commands.GroupBy(c => c.Category))
                {
                    Label(_content, group.Key.ToUpperInvariant(), 26, Accent, FontStyle.Bold, TextAnchor.MiddleLeft, false);
                    foreach (var command in group) AddRow(provider.Id, command);
                }
            }
            RefreshStates();
        }

        private void AddRow(string providerId, CheatDefinition command)
        {
            var row = Box(command.Id, _content, RowColor);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(24, 20, 16, 16);
            h.spacing = 20;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            var texts = Node("Texts", row);
            var v = texts.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 4;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            texts.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            Label(texts, command.Title, 34, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            var reason = Label(texts, command.Description, 24, Muted, FontStyle.Normal, TextAnchor.MiddleLeft, false);

            var buttonRect = Box("Run", row, Go);
            var element = buttonRect.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 210;
            element.preferredHeight = 88;
            element.flexibleWidth = 0;
            var action = Label(buttonRect, "RUN", 30, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, true);
            var button = buttonRect.gameObject.AddComponent<Button>();
            string commandId = command.Id;
            button.onClick.AddListener(() => Run(providerId, commandId));
            _rows.Add(new Row { Provider = providerId, Command = commandId, Button = button, Action = action, Reason = reason });
        }

        private void RefreshStates()
        {
            foreach (var row in _rows)
            {
                var state = CheatRegistry.Shared.GetState(row.Provider, row.Command);
                row.Button.interactable = state.Enabled;
                row.Button.GetComponent<Image>().color = state.Enabled ? Go : Off;
                row.Action.text = state.ActionLabel;
                var provider = CheatRegistry.Shared.Find(row.Provider);
                var definition = provider?.Commands.FirstOrDefault(c => c.Id == row.Command);
                row.Reason.text = state.Enabled ? definition?.Description ?? string.Empty : state.DisabledReason;
                row.Reason.color = state.Enabled ? Muted : Error;
            }
        }

        private void Run(string providerId, string commandId)
        {
            if (CheatPanel.CloseOnExecute) SetOpen(false);
            var result = CheatRegistry.Shared.Execute(providerId, commandId);
            _feedback.text = result.Message;
            _feedback.color = result.Success ? Muted : Error;
            if (!result.Success) Debug.LogWarning("[Cheats] " + commandId + ": " + result.Message);
        }

        private static RectTransform Node(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static RectTransform Box(string name, Transform parent, Color color)
        {
            var rt = Node(name, parent);
            rt.gameObject.AddComponent<Image>().color = color;
            return rt;
        }

        private static void Stretch(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = min;
            rt.offsetMax = max;
        }

        private Text Label(Transform parent, string value, int size, Color color, FontStyle style, TextAnchor anchor, bool fill)
        {
            var rt = Node("Text", parent);
            var text = rt.gameObject.AddComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            if (fill) Stretch(rt, Vector2.zero, Vector2.zero);
            return text;
        }
    }
}
