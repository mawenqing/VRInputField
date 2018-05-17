using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Qiyi.UI.InputField
{
    public class MultiLineInputFieldImpl : AbstractInputField
    {
        private Color _selectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255);
        private int _drawStart, _drawEnd;

        public MultiLineInputFieldImpl(
            ICaret caret,
            IInputEventProcessor inputEventProcessor,
            IInputFieldController controller,
            ITextComponentWrapper text)
            : base(caret, inputEventProcessor, controller, text)
        {
        }

        protected override string ProcessText(ITextComponentWrapper text, ICaret caret)
        {
            Vector2 extents = text.DisplayRect.size;

            int caretLine = text.GetLineByCharIndex(caret.GetIndex());

            if (caret.GetIndex() > _drawEnd)
            {
                _drawEnd = text.LineEndIndex(caretLine);
                float bottomY = text.LineTop(caretLine) - text.LineHeight(caretLine);
                // TODO: Remove interline spacing on last line.
                int startLine = caretLine;
                while (startLine > 0)
                {
                    float topY = text.LineTop(startLine - 1);
                    if (topY - bottomY > extents.y)
                        break;
                    startLine--;
                }
                _drawStart = text.LineStartIndex(startLine);
            }
            else
            {
                if (caret.GetIndex() < _drawStart)
                {
                    _drawStart = text.LineStartIndex(caretLine);
                }

                int startLine = text.GetLineByCharIndex(_drawStart);
                int endLine = startLine;

                float topY = text.LineTop(startLine);
                float bottomY = text.LineTop(endLine) - text.LineHeight(endLine);

                // TODO: Remove interline spacing on last line.

                while (endLine < text.LineCount - 1)
                {
                    bottomY = text.LineTop(endLine + 1) - text.LineHeight(endLine + 1);
                    // TODO: Remove interline spacing on last line.
                    if (topY - bottomY > extents.y)
                        break;
                    ++endLine;
                }

                _drawEnd = text.LineEndIndex(endLine);

                while (startLine > 0)
                {
                    topY = text.LineTop(startLine - 1);
                    if (topY - bottomY > extents.y)
                    {
                        break;
                    }

                    startLine--;
                }

                _drawStart = text.LineStartIndex(startLine);
            }
            return TextValue.Substring(_drawStart, _drawEnd - _drawStart);
        }

        protected override void DrawSelection(ICaret caret, Color color, ITextComponentWrapper text, Vector2 offset)
        {
            var lines = CalculateSelectionRects(caret, text, offset);

            using (var helper = new VertexHelper())
            {
                foreach (var rect in lines)
                {
                    caret.Draw(rect, color, helper);
                }
            }
        }

        protected override void DrawCaret(ICaret caret, Color color, ITextComponentWrapper text, Vector2 offset)
        {
            using (var helper = new VertexHelper())
            {
                caret.Draw(CalculateCaretDrawRect(text, offset, caret.GetIndex() - _drawStart), color, helper);
            }
        }

        private List<Rect> CalculateSelectionRects(ICaret caret, ITextComponentWrapper text, Vector2 offset)
        {
            List<Rect> highlightRects = new List<Rect>();

            int start = Mathf.Min(caret.GetIndex(), caret.GetSelectionIndex()) - _drawStart;
            start = Mathf.Clamp(start, 0, text.DisplayedTextLength);
            int end = Mathf.Max(caret.GetIndex(), caret.GetSelectionIndex()) - _drawStart;
            end = Mathf.Clamp(end, 0, text.DisplayedTextLength);

            int startLine = text.GetLineByCharIndex(start);
            int endLine = text.GetLineByCharIndex(end);
            if (startLine == endLine)
            {
                highlightRects.Add(HightedLineRect(start, end, text.LineHeight(startLine), text));
            }
            else
            {
                int currentLineEndIndex = text.LineEndIndex(startLine);
                highlightRects.Add(HightedLineRect(start, currentLineEndIndex, text.LineHeight(startLine), text));

                while (startLine < endLine - 1)
                {
                    startLine++;
                    highlightRects.Add(HightedLineRect(
                        text.LineStartIndex(startLine),
                        text.LineEndIndex(startLine),
                        text.LineHeight(startLine),
                        text));
                }

                highlightRects.Add(HightedLineRect(
                    text.LineStartIndex(endLine), end, text.LineHeight(endLine), text));
            }
            return highlightRects;
        }

        private Rect HightedLineRect(int startIndex, int endIndex, float height, ITextComponentWrapper text)
        {
            Vector2 start = text.CursorPositionAt(startIndex);
            Vector2 end = text.CursorPositionAt(endIndex);
            // FIXME: add last char width according to caret moving direction.
            Rect rect = new Rect(start.x, start.y - height,
                Mathf.Abs(end.x + text.CharWidth(endIndex + _drawStart) - start.x), height);
            return rect;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            Caret.MoveTo(
                EditableText.RelativeIndexFromPosition(
                    EditableText.MousePositionInTextRect(eventData)) + _drawStart,
                false);

            eventData.Use();
        }

        public override void OnDrag(PointerEventData eventData)
        {
            Vector2 localMousePos = EditableText.MousePositionInTextRect(eventData);
            if (localMousePos.y < EditableText.DisplayRect.yMin)
            {
                Caret.MoveTo(EditableText.LineDownIndex(true, Caret.GetIndex()), true);
                UpdateText();
            }
            else if (localMousePos.y > EditableText.DisplayRect.yMax)
            {
                Caret.MoveTo(EditableText.LineUpIndex(true, Caret.GetIndex()), true);
                UpdateText();
            }
            else {
                Caret.MoveTo(EditableText.RelativeIndexFromPosition(localMousePos) + _drawStart, true);
                InputFieldController.MarkGeometryAsDirty();
            }

            eventData.Use();
        }

        private Rect CalculateCaretDrawRect(
            ITextComponentWrapper text,
            Vector2 offset,
            int relativeIndex)
        {
            Vector2 localCursorPos = text.CursorPositionAt(relativeIndex);
            int line = text.GetLineByCharIndex(relativeIndex);
            float height = text.LineHeight(line);

            return new Rect(localCursorPos.x + offset.x,
                localCursorPos.y - height + offset.y, 1,
                height);
        }
    }
}
