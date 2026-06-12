using System.Collections.Generic;
using Deucarian.CoreState;
using Deucarian.UIBinding;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.UIBinding.CoreStateBridge.Tests
{
    public sealed class SelectionUIBindingTests
    {
        private RectTransform _parent;
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            _parent = new GameObject("Parent", typeof(RectTransform)).GetComponent<RectTransform>();
            _prefab = new GameObject("ItemPrefab", typeof(RectTransform), typeof(TestItem));
        }

        [TearDown]
        public void TearDown()
        {
            if (_parent != null)
            {
                Object.DestroyImmediate(_parent.gameObject);
            }

            if (_prefab != null)
            {
                Object.DestroyImmediate(_prefab);
            }
        }

        [Test]
        public void SelectionChange_UpdatesSelectedItemState()
        {
            BindingContext context = CreateBoundContext();

            context.Selection.Select("one");

            Assert.That(GetItem(context.Container, "one").IsSelected, Is.True);
            Assert.That(GetItem(context.Container, "two").IsSelected, Is.False);
        }

        [Test]
        public void SelectionChange_UpdatesContainerVisualSelectionWhenVisualIsConfigured()
        {
            RecordingVisual visual = new RecordingVisual();
            BindingContext context = CreateBoundContext(visual);

            visual.Clear();
            context.Selection.Select("two");

            Assert.That(context.Container.HasSelectedKey, Is.True);
            Assert.That(context.Container.SelectedKey, Is.EqualTo("two"));
            Assert.That(visual.Calls, Is.EqualTo(new[] { "selected:two" }));
            Assert.That(GetItem(context.Container, "two").IsSelected, Is.False);
        }

        [Test]
        public void SelectingAnotherKey_ClearsPreviousItem()
        {
            BindingContext context = CreateBoundContext();

            context.Selection.Select("one");
            context.Selection.Select("two");

            Assert.That(GetItem(context.Container, "one").IsSelected, Is.False);
            Assert.That(GetItem(context.Container, "two").IsSelected, Is.True);
        }

        [Test]
        public void ClearSelection_ClearsSelectedItemState()
        {
            BindingContext context = CreateBoundContext();

            context.Selection.Select("one");
            context.Selection.Clear(SelectionChangeMode.Programmatic);

            Assert.That(GetItem(context.Container, "one").IsSelected, Is.False);
            Assert.That(GetItem(context.Container, "two").IsSelected, Is.False);
        }

        [Test]
        public void Bind_AppliesExistingSelection()
        {
            Repository<string, TestData> repository = CreateRepository(
                new TestData("one", "First"),
                new TestData("two", "Second"));
            SelectionService<string, TestData> selection = new SelectionService<string, TestData>(repository);
            selection.Select("two");
            UIBindingContainer<TestData, string> container = CreateContainer();
            var repositoryBinding = new RepositoryUIBinding<string, TestData>(repository, container);
            var selectionBinding = new SelectionUIBinding<string, TestData>(selection, container);

            repositoryBinding.Bind();
            selectionBinding.Bind();

            Assert.That(GetItem(container, "one").IsSelected, Is.False);
            Assert.That(GetItem(container, "two").IsSelected, Is.True);
        }

        [Test]
        public void RemovedSelectedItem_ClearsSelectionAndRemainingItemState()
        {
            BindingContext context = CreateBoundContext();
            context.Selection.Select("one");

            context.Repository.Remove("one");

            Assert.That(context.Selection.HasSelection, Is.False);
            Assert.That(context.Container.TryGetItem("one", out _), Is.False);
            Assert.That(GetItem(context.Container, "two").IsSelected, Is.False);
        }

        [Test]
        public void Unbind_StopsSelectionUpdates()
        {
            BindingContext context = CreateBoundContext();

            context.SelectionBinding.Unbind();
            context.Selection.Select("one");

            Assert.That(GetItem(context.Container, "one").IsSelected, Is.False);
            Assert.That(context.SelectionBinding.IsBound, Is.False);
        }

        private BindingContext CreateBoundContext(RecordingVisual visual = null)
        {
            Repository<string, TestData> repository = CreateRepository(
                new TestData("one", "First"),
                new TestData("two", "Second"));
            SelectionService<string, TestData> selection = new SelectionService<string, TestData>(repository);
            UIBindingContainer<TestData, string> container = CreateContainer(visual);
            var repositoryBinding = new RepositoryUIBinding<string, TestData>(repository, container);
            var selectionBinding = new SelectionUIBinding<string, TestData>(selection, container);

            repositoryBinding.Bind();
            selectionBinding.Bind();

            return new BindingContext(repository, selection, container, repositoryBinding, selectionBinding);
        }

        private UIBindingContainer<TestData, string> CreateContainer(RecordingVisual visual = null)
        {
            return new UIBindingContainer<TestData, string>(_parent, _prefab, data => data.Id, visual);
        }

        private static Repository<string, TestData> CreateRepository(params TestData[] items)
        {
            var repository = new Repository<string, TestData>();
            repository.AddOrUpdateMany(items);
            return repository;
        }

        private static TestItem GetItem(UIBindingContainer<TestData, string> container, string key)
        {
            Assert.That(container.TryGetItem(key, out ISettableItem<TestData> item), Is.True);
            return (TestItem)item;
        }

        private sealed class BindingContext
        {
            public BindingContext(
                Repository<string, TestData> repository,
                SelectionService<string, TestData> selection,
                UIBindingContainer<TestData, string> container,
                RepositoryUIBinding<string, TestData> repositoryBinding,
                SelectionUIBinding<string, TestData> selectionBinding)
            {
                Repository = repository;
                Selection = selection;
                Container = container;
                RepositoryBinding = repositoryBinding;
                SelectionBinding = selectionBinding;
            }

            public Repository<string, TestData> Repository { get; }
            public SelectionService<string, TestData> Selection { get; }
            public UIBindingContainer<TestData, string> Container { get; }
            public RepositoryUIBinding<string, TestData> RepositoryBinding { get; }
            public SelectionUIBinding<string, TestData> SelectionBinding { get; }
        }

        private sealed class TestData : IIdentifiable<string>
        {
            public TestData(string id, string label)
            {
                Id = id;
                Label = label;
            }

            public string Id { get; }
            public string Label { get; }
        }

        private sealed class TestItem : MonoBehaviour, ISettableItem<TestData>, ISelectableUIItem
        {
            public TestData Data { get; private set; }
            public bool IsSelected { get; private set; }

            public void SetData(TestData data)
            {
                Data = data;
            }

            public void SetSelected(bool selected)
            {
                IsSelected = selected;
            }
        }

        private sealed class RecordingVisual : IUIBindingItemVisual<string, TestData>
        {
            public readonly List<string> Calls = new List<string>();

            public void ApplyNormal(string key, TestData item, object view)
            {
                Calls.Add("normal:" + key);
            }

            public void ApplySelected(string key, TestData item, object view)
            {
                Calls.Add("selected:" + key);
            }

            public void ApplyHovered(string key, TestData item, object view)
            {
                Calls.Add("hovered:" + key);
            }

            public void Clear()
            {
                Calls.Clear();
            }
        }
    }
}
