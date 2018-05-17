namespace Qiyi.UI.InputField
{
    public interface IInputFieldController
    {
        void MarkGeometryAsDirty();

        void OnEndInput(string text);

        void UpdateDisplayText(string text);

        void PopulateText(string text);
    }
}
