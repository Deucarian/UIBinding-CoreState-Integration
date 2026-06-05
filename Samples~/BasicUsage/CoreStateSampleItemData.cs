using JorisHoef.Core.State;

namespace JorisHoef.GenericUIItems.CoreState.Samples.BasicUsage
{
    public sealed class CoreStateSampleItemData : IIdentifiable<string>
    {
        public CoreStateSampleItemData(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public string Id { get; }
        public string Label { get; }
    }
}
