namespace JorisHoef.GenericUIItems.CoreState
{
    /// <summary>
    /// Optional contract for UI item components that can display selected state.
    /// </summary>
    public interface ISelectableUIItem
    {
        void SetSelected(bool selected);
    }
}
