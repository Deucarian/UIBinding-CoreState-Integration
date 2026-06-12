using System;
using Deucarian.CoreState;
using Deucarian.UIBinding;

namespace Deucarian.UIBinding.CoreStateBridge
{
    /// <summary>
    /// Synchronizes a Core State repository into a UI Binding container.
    /// </summary>
    public sealed class RepositoryUIBinding<TKey, T> : IDisposable
    {
        private readonly IReadOnlyRepository<TKey, T> _repository;
        private readonly IUIBindingContainer<T, TKey> _container;
        private bool _isBound;

        public RepositoryUIBinding(
            IReadOnlyRepository<TKey, T> repository,
            IUIBindingContainer<T, TKey> container)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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

            _repository.ItemAdded += OnItemAdded;
            _repository.ItemUpdated += OnItemUpdated;
            _repository.ItemRemoved += OnItemRemoved;
            _repository.Cleared += OnRepositoryCleared;
            _isBound = true;

            try
            {
                Refresh();
            }
            catch
            {
                Unbind();
                throw;
            }
        }

        public void Refresh()
        {
            _container.ReplaceAll(_repository.Items);
        }

        public void Unbind()
        {
            if (!_isBound)
            {
                return;
            }

            _repository.ItemAdded -= OnItemAdded;
            _repository.ItemUpdated -= OnItemUpdated;
            _repository.ItemRemoved -= OnItemRemoved;
            _repository.Cleared -= OnRepositoryCleared;
            _isBound = false;
        }

        public void Dispose()
        {
            Unbind();
        }

        private void OnItemAdded(TKey key, T item)
        {
            if (_container.TryGetItem(key, out _))
            {
                _container.Update(item);
                return;
            }

            _container.Add(item);
        }

        private void OnItemUpdated(TKey key, T item)
        {
            if (!_container.Update(item))
            {
                _container.Add(item);
            }
        }

        private void OnItemRemoved(TKey key, T item)
        {
            _container.Remove(key);
        }

        private void OnRepositoryCleared()
        {
            _container.Clear();
        }
    }
}
