using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Qiyi.UI.InputField
{
    public class VrInputFieldImpl : AbstractInputField
    {
        private int _drawStart, _drawEnd;

        public VrInputFieldImpl(ICaret caret,
            IInputEventProcessor inputEventProcessor,
            IInputFieldController controller,
            ITextComponentWrapper editableText
            ) : base(caret, inputEventProcessor, controller, editableText)
        {
        }

        protected override string ProcessText(ITextComponentWrapper editableText, ICaret caret)
        {
            // truncate text to display within the bounds of text rect.
            Vector2 textRectExtents = editableText.DisplayRect.size;

            float width = 0;
            if (caret.GetIndex() > _drawEnd || (caret.GetIndex() == TextValue.Length && _drawStart > 0))
            {
                _drawEnd = caret.GetIndex();
                _drawStart = _drawEnd - 1;
                while (width < textRectExtents.x && _drawStart >= 0)
                {
                    width += editableText.CharWidth(_drawStart--);
                }

                if (width >= textRectExtents.x)
                {
                    _drawStart++;
                }

                _drawStart++;
            }
            else
            {
                if (caret.GetIndex() < _drawStart)
                {
                    _drawStart = caret.GetIndex();
                }

                _drawEnd = _drawStart;
                while (width < textRectExtents.x && _drawEnd < TextValue.Length)
                {
                    width += editableText.CharWidth(_drawEnd++);
                }

                if (width >= textRectExtents.x)
                {
                    _drawEnd--;
                }
            }

            _drawStart = Mathf.Clamp(_drawStart, 0, TextValue.Length);
            _drawEnd = Mathf.Clamp(_drawEnd, 0, TextValue.Length);
            return TextValue.Substring(_drawStart, _drawEnd - _drawStart);
        }

        private Rect CalculateCaretDrawRect(
            ITextComponentWrapper text,
            Vector2 offset,
            int index,
            int selectionIndex)
        {
            Vector2 localCursorPos = text.CursorPositionAt(index);
            Vector2 localSelectionPos = text.CursorPositionAt(selectionIndex);
            float height = text.LineHeight(0);

            if (index > selectionIndex)
            {
                Vector2 temp = localSelectionPos;
                localSelectionPos = localCursorPos;
                localCursorPos = temp;
            }

            return new Rect(localCursorPos.x + offset.x,
                localCursorPos.y - height + offset.y,
                index != selectionIndex ? Mathf.Abs(localSelectionPos.x - localCursorPos.x) : 1,
                height);
        }

        private int LocalIndex()
        {
            return Caret.GetIndex() - _drawStart;
        }

        private int LocalSelectionIndex(ITextComponentWrapper text)
        {
            // selection index could be out of bounds.
            return Mathf.Clamp(Caret.GetSelectionIndex() - _drawStart, 0, text.DisplayedTextLength);
        }

        protected override void DrawSelection(ICaret caret, Color color, ITextComponentWrapper text, Vector2 offset)
        {
            int index = LocalIndex();
            int selectionIndex = LocalSelectionIndex(text);
            using (var helper = new VertexHelper())
            {
                Caret.Draw(CalculateCaretDrawRect(text, offset, index, selectionIndex), color, helper);
            }
        }

        protected override void DrawCaret(ICaret caret, Color color, ITextComponentWrapper text, Vector2 offset)
        {
            int index = LocalIndex();
            int selectionIndex = LocalSelectionIndex(text);

            using (var helper = new VertexHelper())
            {
                Caret.Draw(CalculateCaretDrawRect(text, offset, index, selectionIndex), color, helper);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            MoveCaretWithinBounds(EditableText.RelativeIndexFromPosition(
                EditableText.MousePositionInTextRect(eventData)) + _drawStart, false);
            eventData.Use();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            Vector2 localMousePos = EditableText.MousePositionInTextRect(eventData);
            if (localMousePos.x < EditableText.DisplayRect.xMin)
            {
                MoveCaretWithinBounds(Caret.GetIndex() - 1, true);
                UpdateText();
            }
            else if (localMousePos.x > EditableText.DisplayRect.xMax)
            {
                MoveCaretWithinBounds(Caret.GetIndex() + 1, true);
                UpdateText();
            }
            else
            {
                MoveCaretWithinBounds(EditableText.RelativeIndexFromPosition(localMousePos) + _drawStart, true);
                InputFieldController.MarkGeometryAsDirty();
            }

            eventData.Use();
        }

        private void MoveCaretWithinBounds(int index, bool withSelection)
        {
            index = Mathf.Clamp(index, 0, TextValue.Length);
            Caret.MoveTo(index, withSelection);
        }
    }
}
