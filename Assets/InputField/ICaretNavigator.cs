namespace Qiyi.UI.InputField
{
    public interface ICaretNavigator
    {
        void MoveCaretTo(int index, bool withSelection);

        void MoveLeft(bool withSelection);

        void MoveRight(bool withSelection);

        void MoveDown(bool goToLastChar, bool withSelection);

        void MoveUp(bool goToFistChar, bool withSelection);
    }
}
