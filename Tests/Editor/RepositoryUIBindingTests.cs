using System.Linq;
using JorisHoef.Core.State;
using JorisHoef.GenericUIItems;
using NUnit.Framework;
using UnityEngine;

namespace JorisHoef.GenericUIItems.CoreState.Tests
{
    public sealed class RepositoryUIBindingTests
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
        public void Bind_PopulatesContainerFromExistingRepository()
        {
            Repository<string, TestData> repository = CreateRepository(
                new TestData("one", "First"),
                new TestData("two", "Second"));
            GenericUIContainer<TestData, string> container = CreateContainer();
            var binding = new RepositoryUIBinding<string, TestData>(repository, container);

            binding.Bind();

            Assert.That(container.Count, Is.EqualTo(2));
            Assert.That(_parent.childCount, Is.EqualTo(2));
            Assert.That(container.GetItems().Cast<TestItem>().Select(item => item.Data.Label), Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void RepositoryAdd_CreatesUIItem()
        {
            Repository<string, TestData> repository = CreateRepository();
            GenericUIContainer<TestData, string> container = CreateContainer();
            var binding = new RepositoryUIBinding<string, TestData>(repository, container);
            binding.Bind();

            repository.AddOrUpdate(new TestData("one", "First"));

            Assert.That(container.Count, Is.EqualTo(1));
            Assert.That(GetItem(container, "one").Data.Label, Is.EqualTo("First"));
        }

        [Test]
        public void RepositoryUpdate_UpdatesUIItem()
        {
            Repository<string, TestData> repository = CreateRepository(new TestData("one", "First"));
            GenericUIContainer<TestData, string> container = CreateContainer();
            var binding = new RepositoryUIBinding<string, TestData>(repository, container);
            binding.Bind();

            repository.AddOrUpdate(new TestData("one", "First updated"));

            Assert.That(container.Count, Is.EqualTo(1));
            Assert.That(GetItem(container, "one").Data.Label, Is.EqualTo("First updated"));
        }

        [Test]
        public void RepositoryRemove_RemovesUIItem()
        {
            Repository<string, TestData> repository = CreateRepository(
                new TestData("one", "First"),
                new TestData("two", "Second"));
            GenericUIContainer<TestData, string> container = CreateContainer();
            var binding = new RepositoryUIBinding<string, TestData>(repository, container);
            binding.Bind();

            bool removed = repository.Remove("one");

            Assert.That(removed, Is.True);
            Assert.That(container.Count, Is.EqualTo(1));
            Assert.That(container.TryGetItem("one", out _), Is.False);
            Assert.That(GetItem(container, "two").Data.Label, Is.EqualTo("Second"));
        }

        [Test]
        public void RepositoryClear_ClearsUIItems()
        {
            Repository<string, TestData> repository = CreateRepository(
                new TestData("one", "First"),
                new TestData("two", "Second"));
            GenericUIContainer<TestData, string> container = CreateContainer();
            var binding = new RepositoryUIBinding<string, TestData>(repository, container);
            binding.Bind();

            repository.Clear();

            Assert.That(container.Count, Is.EqualTo(0));
            Assert.That(_parent.childCount, Is.EqualTo(0));
        }

        [Test]
        public void Unbind_StopsRepositoryUpdates()
        {
            Repository<string, TestData> repository = CreateRepository(new TestData("one", "First"));
            GenericUIContainer<TestData, string> container = CreateContainer();
            var binding = new RepositoryUIBinding<string, TestData>(repository, container);
            binding.Bind();

            binding.Unbind();
            repository.AddOrUpdate(new TestData("two", "Second"));
            repository.AddOrUpdate(new TestData("one", "First updated"));
            repository.Remove("one");

            Assert.That(container.Count, Is.EqualTo(1));
            Assert.That(GetItem(container, "one").Data.Label, Is.EqualTo("First"));
            Assert.That(container.TryGetItem("two", out _), Is.False);
        }

        [Test]
        public void UnbindAndDispose_AreIdempotent()
        {
            Repository<string, TestData> repository = CreateRepository(new TestData("one", "First"));
            GenericUIContainer<TestData, string> container = CreateContainer();
            var binding = new RepositoryUIBinding<string, TestData>(repository, container);

            Assert.DoesNotThrow(() =>
            {
                binding.Bind();
                binding.Bind();
                binding.Unbind();
                binding.Unbind();
                binding.Dispose();
                binding.Dispose();
            });

            Assert.That(binding.IsBound, Is.False);
        }

        [Test]
        public void Bindings_DoNotShareStaticState()
        {
            Repository<string, TestData> firstRepository = CreateRepository(new TestData("one", "First"));
            Repository<string, TestData> secondRepository = CreateRepository(new TestData("two", "Second"));
            GenericUIContainer<TestData, string> firstContainer = CreateContainer();
            GenericUIContainer<TestData, string> secondContainer = CreateContainer();
            var firstBinding = new RepositoryUIBinding<string, TestData>(firstRepository, firstContainer);
            var secondBinding = new RepositoryUIBinding<string, TestData>(secondRepository, secondContainer);

            firstBinding.Bind();
            secondBinding.Bind();
            firstRepository.AddOrUpdate(new TestData("one", "First updated"));
            secondRepository.AddOrUpdate(new TestData("three", "Third"));

            Assert.That(firstContainer.Count, Is.EqualTo(1));
            Assert.That(secondContainer.Count, Is.EqualTo(2));
            Assert.That(firstContainer.TryGetItem("two", out _), Is.False);
            Assert.That(firstContainer.TryGetItem("three", out _), Is.False);
            Assert.That(secondContainer.TryGetItem("one", out _), Is.False);
        }

        private GenericUIContainer<TestData, string> CreateContainer()
        {
            return new GenericUIContainer<TestData, string>(_parent, _prefab, data => data.Id);
        }

        private static Repository<string, TestData> CreateRepository(params TestData[] items)
        {
            var repository = new Repository<string, TestData>();
            repository.AddOrUpdateMany(items);
            return repository;
        }

        private static TestItem GetItem(GenericUIContainer<TestData, string> container, string key)
        {
            Assert.That(container.TryGetItem(key, out ISettableItem<TestData> item), Is.True);
            return (TestItem)item;
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
    }
}
