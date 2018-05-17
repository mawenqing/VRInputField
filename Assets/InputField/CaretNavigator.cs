using UnityEngine;

namespace Qiyi.UI.InputField
{
    public class CaretNavigator : ICaretNavigator
    {
        ICaret _caret;
        ITextComponentWrapper _textWrapper;
        IVrInputField _inputField;

        public CaretNavigator(ICaret caret, ITextComponentWrapper text, IVrInputField inputField) {
            _caret = caret;
            _textWrapper = text;
            _inputField = inputField;
        }

        public void MoveCaretTo(int index, bool withSelection)
        {
            index = Mathf.Clamp(index, 0, _inputField.TextValue.Length);
            _caret.MoveTo(index, withSelection);
        }

        public void MoveDown(bool goToLastChar, bool withSelection)
        {
            MoveCaretTo(_textWrapper.LineDownIndex(goToLastChar, _caret.GetIndex()), withSelection);
        }

        public void MoveLeft(bool withSelection)
        {
            MoveCaretTo(_caret.GetIndex() - 1, withSelection);
        }

        public void MoveRight(bool withSelection)
        {
            MoveCaretTo(_caret.GetIndex() + 1, withSelection);
        }

        public void MoveUp(bool goToFirstChar, bool withSelection)
        {
            MoveCaretTo(_textWrapper.LineUpIndex(goToFirstChar, _caret.GetIndex()), withSelection);
        }
    }
}
