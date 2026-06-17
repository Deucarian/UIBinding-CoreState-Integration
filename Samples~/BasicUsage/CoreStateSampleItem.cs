using Deucarian.UIBinding;
using UnityEngine;
using UnityEngine.UI;

namespace Deucarian.UIBinding.CoreStateIntegration.Samples.BasicUsage
{
    public sealed class CoreStateSampleItem : GenericItem<CoreStateSampleItemData>, ISelectableUIItem
    {
        [SerializeField] private CoreStateUIBindingSample owner;
        [SerializeField] private Text label;
        [SerializeField] private Image background;
        [SerializeField] private Button button;

        private bool _isSelected;

        private void Awake()
        {
            EnsureReferences();

            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
                button.onClick.AddListener(OnClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
        }

        public void Configure(
            CoreStateUIBindingSample sampleOwner,
            Text itemLabel,
            Image itemBackground,
            Button itemButton)
        {
            owner = sampleOwner;
            if (itemLabel != null)
            {
                label = itemLabel;
            }

            if (itemBackground != null)
            {
                background = itemBackground;
            }

            if (itemButton != null)
            {
                button = itemButton;
            }

            ApplyVisualState();
        }

        public override void SetData(CoreStateSampleItemData data)
        {
            base.SetData(data);
            EnsureReferences();

            if (label != null)
            {
                label.text = data != null ? data.Label : string.Empty;
            }
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            ApplyVisualState();
        }

        private void OnClicked()
        {
            if (owner != null && Data != null)
            {
                owner.Select(Data.Id);
            }
        }

        private void EnsureReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (label == null)
            {
                label = GetComponentInChildren<Text>();
            }
        }

        private void ApplyVisualState()
        {
            if (background != null)
            {
                background.color = _isSelected
                    ? new Color(0.16f, 0.42f, 0.95f, 1f)
                    : new Color(0.18f, 0.18f, 0.18f, 1f);
            }

            if (label != null)
            {
                label.color = Color.white;
            }
        }
    }
}
