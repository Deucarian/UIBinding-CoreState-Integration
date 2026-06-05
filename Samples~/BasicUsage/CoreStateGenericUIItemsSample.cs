using System;
using JorisHoef.Core.State;
using JorisHoef.GenericUIItems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JorisHoef.GenericUIItems.CoreState.Samples.BasicUsage
{
    public sealed class CoreStateGenericUIItemsSample : MonoBehaviour
    {
        [SerializeField] private RectTransform parent;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Text statusText;
        [SerializeField] private Button addButton;
        [SerializeField] private Button updateSelectedButton;
        [SerializeField] private Button removeSelectedButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button resetButton;

        private Repository<string, CoreStateSampleItemData> _repository;
        private SelectionService<string, CoreStateSampleItemData> _selection;
        private GenericUIContainer<CoreStateSampleItemData, string> _container;
        private RepositoryUIBinding<string, CoreStateSampleItemData> _repositoryBinding;
        private SelectionUIBinding<string, CoreStateSampleItemData> _selectionBinding;
        private int _nextId = 3;

        private void Awake()
        {
            EnsureLayout();
            CreateState();
            WireButtons();
            ResetData();
        }

        private void OnDestroy()
        {
            if (_selection != null)
            {
                _selection.SelectionChanged -= OnSelectionChanged;
            }

            if (_selectionBinding != null)
            {
                _selectionBinding.Dispose();
            }

            if (_repositoryBinding != null)
            {
                _repositoryBinding.Dispose();
            }
        }

        public void Select(string id)
        {
            if (_selection != null)
            {
                _selection.TrySelect(id);
                UpdateStatus();
            }
        }

        public void Add()
        {
            string id = _nextId.ToString();
            _nextId++;
            _repository.AddOrUpdate(new CoreStateSampleItemData(id, "Repository item " + id));
            UpdateStatus();
        }

        public void UpdateSelected()
        {
            if (!_selection.HasSelection)
            {
                UpdateStatus();
                return;
            }

            string id = _selection.SelectedKey;
            _repository.AddOrUpdate(new CoreStateSampleItemData(id, "Updated item " + id));
            UpdateStatus();
        }

        public void RemoveSelected()
        {
            if (_selection.HasSelection)
            {
                _repository.Remove(_selection.SelectedKey);
            }

            UpdateStatus();
        }

        public void Clear()
        {
            _repository.Clear();
            UpdateStatus();
        }

        public void ResetData()
        {
            _repository.Clear();
            _repository.AddOrUpdate(new CoreStateSampleItemData("1", "Repository item 1"));
            _repository.AddOrUpdate(new CoreStateSampleItemData("2", "Repository item 2"));
            _nextId = 3;
            UpdateStatus();
        }

        private void CreateState()
        {
            _repository = new Repository<string, CoreStateSampleItemData>();
            _selection = new SelectionService<string, CoreStateSampleItemData>(_repository);
            _selection.SelectionChanged += OnSelectionChanged;

            CoreStateSampleItem sampleItem = itemPrefab.GetComponent<CoreStateSampleItem>();
            if (sampleItem == null)
            {
                throw new InvalidOperationException("Item prefab must have a CoreStateSampleItem component on its root GameObject.");
            }

            sampleItem.Configure(this, null, null, null);

            _container = new GenericUIContainer<CoreStateSampleItemData, string>(
                parent,
                itemPrefab,
                item => item.Id);

            _repositoryBinding = new RepositoryUIBinding<string, CoreStateSampleItemData>(
                _repository,
                _container);

            _selectionBinding = new SelectionUIBinding<string, CoreStateSampleItemData>(
                _selection,
                _container);

            _repositoryBinding.Bind();
            _selectionBinding.Bind();
        }

        private void WireButtons()
        {
            AddListener(addButton, Add);
            AddListener(updateSelectedButton, UpdateSelected);
            AddListener(removeSelectedButton, RemoveSelected);
            AddListener(clearButton, Clear);
            AddListener(resetButton, ResetData);
        }

        private void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs<string, CoreStateSampleItemData> args)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (statusText == null || _repository == null || _selection == null)
            {
                return;
            }

            statusText.text = _selection.HasSelection
                ? "Items: " + _repository.Count + " | Selected: " + _selection.SelectedKey
                : "Items: " + _repository.Count + " | Selected: none";
        }

        private void EnsureLayout()
        {
            EnsureEventSystem();

            if (parent != null && itemPrefab != null)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Core State Generic UI Sample", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }

            RectTransform root = CreatePanel("Root", canvas.transform, new Color(0.08f, 0.09f, 0.1f, 1f));
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(520f, 420f);
            root.anchoredPosition = Vector2.zero;

            var rootLayout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(16, 16, 16, 16);
            rootLayout.spacing = 10f;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childForceExpandWidth = true;

            statusText = CreateText("Status", root, "Items: 0 | Selected: none", 16, TextAnchor.MiddleLeft);

            RectTransform controls = CreatePanel("Controls", root, new Color(0.12f, 0.13f, 0.15f, 1f));
            controls.sizeDelta = new Vector2(0f, 44f);
            var controlsLayout = controls.gameObject.AddComponent<HorizontalLayoutGroup>();
            controlsLayout.spacing = 8f;
            controlsLayout.childForceExpandWidth = true;
            controlsLayout.childForceExpandHeight = true;

            addButton = CreateButton("Add", controls, "Add");
            updateSelectedButton = CreateButton("Update Selected", controls, "Update");
            removeSelectedButton = CreateButton("Remove Selected", controls, "Remove");
            clearButton = CreateButton("Clear", controls, "Clear");
            resetButton = CreateButton("Reset", controls, "Reset");

            parent = CreatePanel("Items", root, new Color(0.1f, 0.11f, 0.12f, 1f));
            var itemsLayout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            itemsLayout.padding = new RectOffset(8, 8, 8, 8);
            itemsLayout.spacing = 6f;
            itemsLayout.childForceExpandHeight = false;
            itemsLayout.childForceExpandWidth = true;

            GameObject prefabHost = new GameObject("Runtime Prefab Source");
            prefabHost.transform.SetParent(transform, false);
            prefabHost.SetActive(false);
            itemPrefab = CreateItemPrefab(prefabHost.transform);
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject CreateItemPrefab(Transform prefabHost)
        {
            RectTransform item = CreatePanel("CoreStateSampleItem", prefabHost, new Color(0.18f, 0.18f, 0.18f, 1f));
            item.sizeDelta = new Vector2(0f, 38f);
            item.gameObject.AddComponent<Button>();

            Text label = CreateText("Label", item, "Repository item", 15, TextAnchor.MiddleLeft);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(12f, 0f);
            label.rectTransform.offsetMax = new Vector2(-12f, 0f);

            CoreStateSampleItem sampleItem = item.gameObject.AddComponent<CoreStateSampleItem>();
            sampleItem.Configure(
                null,
                label,
                item.GetComponent<Image>(),
                item.GetComponent<Button>());

            return item.gameObject;
        }

        private static Button CreateButton(string name, Transform parentTransform, string text)
        {
            RectTransform buttonTransform = CreatePanel(name, parentTransform, new Color(0.22f, 0.24f, 0.27f, 1f));
            buttonTransform.sizeDelta = new Vector2(0f, 36f);
            Button button = buttonTransform.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.28f, 0.31f, 0.35f, 1f);
            colors.pressedColor = new Color(0.16f, 0.42f, 0.95f, 1f);
            button.colors = colors;

            Text buttonText = CreateText("Text", buttonTransform, text, 14, TextAnchor.MiddleCenter);
            buttonText.rectTransform.anchorMin = Vector2.zero;
            buttonText.rectTransform.anchorMax = Vector2.one;
            buttonText.rectTransform.offsetMin = Vector2.zero;
            buttonText.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private static RectTransform CreatePanel(string name, Transform parentTransform, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parentTransform, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel.GetComponent<RectTransform>();
        }

        private static Text CreateText(string name, Transform parentTransform, string text, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parentTransform, false);
            Text label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.rectTransform.sizeDelta = new Vector2(0f, 28f);
            return label;
        }
    }
}
