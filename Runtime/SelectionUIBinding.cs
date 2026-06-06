using System;
using System.Reflection;
using JorisHoef.Core.State;
using JorisHoef.GenericUIItems;

namespace JorisHoef.GenericUIItems.CoreState
{
    /// <summary>
    /// Reflects a Core State selection service into selectable Generic UI Items.
    /// </summary>
    public sealed class SelectionUIBinding<TKey, T> : IDisposable
    {
        private readonly ISelectionService<TKey, T> _selectionService;
        private readonly IGenericUIContainer<T, TKey> _container;
        private bool _isBound;

        public SelectionUIBinding(
            ISelectionService<TKey, T> selectionService,
            IGenericUIContainer<T, TKey> container)
        {
            _selectionService = selectionService ?? throw new ArgumentNullException(nameof(selectionService));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public bool IsBound
        {
            get { return _isBound; }
        }

        public void Bind()
        {
            if (_isBound)
            {
                return;
            }

            _selectionService.SelectionChanged += OnSelectionChanged;
            _isBound = true;
            Refresh();
        }

        public void Refresh()
        {
            if (ApplyContainerSelectionVisual())
            {
                return;
            }

            ClearSelectableItems();

            if (_selectionService.HasSelection)
            {
                SetItemSelected(_selectionService.SelectedKey, true);
            }
        }

        public void Unbind()
        {
            if (!_isBound)
            {
                return;
            }

            _selectionService.SelectionChanged -= OnSelectionChanged;
            _isBound = false;
        }

        public void Dispose()
        {
            Unbind();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs<TKey, T> args)
        {
            if (ApplyContainerSelectionVisual(args.HasSelection, args.SelectedKey))
            {
                return;
            }

            if (args.HadPreviousSelection)
            {
                SetItemSelected(args.PreviousKey, false);
            }

            if (args.HasSelection)
            {
                SetItemSelected(args.SelectedKey, true);
            }
        }

        private bool ApplyContainerSelectionVisual()
        {
            return ApplyContainerSelectionVisual(
                _selectionService.HasSelection,
                _selectionService.HasSelection ? _selectionService.SelectedKey : default);
        }

        private bool ApplyContainerSelectionVisual(bool hasSelection, TKey selectedKey)
        {
            Type containerType = _container.GetType();
            PropertyInfo hasItemVisualProperty = containerType.GetProperty("HasItemVisual");

            if (hasItemVisualProperty == null ||
                hasItemVisualProperty.PropertyType != typeof(bool) ||
                !(bool)hasItemVisualProperty.GetValue(_container, null))
            {
                return false;
            }

            if (hasSelection)
            {
                MethodInfo setSelectedKey = containerType.GetMethod(
                    "SetSelectedKey",
                    new[] { typeof(TKey) });

                if (setSelectedKey == null)
                {
                    return false;
                }

                setSelectedKey.Invoke(_container, new object[] { selectedKey });
            }
            else
            {
                MethodInfo clearSelectedKey = containerType.GetMethod(
                    "ClearSelectedKey",
                    Type.EmptyTypes);

                if (clearSelectedKey == null)
                {
                    return false;
                }

                clearSelectedKey.Invoke(_container, null);
            }

            return true;
        }

        private void ClearSelectableItems()
        {
            foreach (ISettableItem<T> item in _container.GetItems())
            {
                if (item is ISelectableUIItem selectableItem)
                {
                    selectableItem.SetSelected(false);
                }
            }
        }

        private bool SetItemSelected(TKey key, bool selected)
        {
            if (!_container.TryGetItem(key, out ISettableItem<T> item))
            {
                return false;
            }

            if (!(item is ISelectableUIItem selectableItem))
            {
                return false;
            }

            selectableItem.SetSelected(selected);
            return true;
        }
    }
}
