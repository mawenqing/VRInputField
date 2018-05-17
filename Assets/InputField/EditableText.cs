using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Qiyi.UI.InputField
{
    public class EditableText : ITextComponentWrapper
    {
        private Text _textComponent;
        private TextGenerator _rawTextGenerator;
        private TextGenerator DisplayedTextGenerator { get { return _textComponent.cachedTextGenerator; } }
        private TextGenerator RawTextGenerator { get { return _rawTextGenerator ?? (_rawTextGenerator = new TextGenerator()); } }

        public EditableText(Text text, TextGenerator generator)
        {
            _textComponent = text;
            _rawTextGenerator = generator;
        }

        public Rect DisplayRect
        {
            get
            {
                return _textComponent.rectTransform.rect;
            }
        }

        public int DisplayedTextLength { get { return _textComponent.text.Length; } }

        public int LineCount
        {
            get { return RawTextGenerator.lineCount; }
        }

        public Vector2 MousePositionInTextRect(PointerEventData eventData)
        {
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_textComponent.rectTransform,
                eventData.position, eventData.pressEventCamera, out localMousePos);
            return localMousePos;
        }

        public void Populate(string text, GameObject context)
        {
            TextGenerationSettings settings = _textComponent.GetGenerationSettings(_textComponent.rectTransform.rect.size);
            settings.generateOutOfBounds = true;
            RawTextGenerator.PopulateWithErrors(text, settings, context);
        }

        public Vector2 CursorPositionAt(int relativeIndex)
        {
            if (DisplayedTextGenerator.characterCount <= 0) {
                return Vector2.zero;
            }

            relativeIndex = Mathf.Clamp(relativeIndex, 0, DisplayedTextGenerator.characterCount - 1);
            UICharInfo cursorChar = DisplayedTextGenerator.characters[relativeIndex];
            return cursorChar.cursorPos;
        }

        public float CharWidth(int index)
        {
            Assert.IsTrue(index >= 0 && index < RawTextGenerator.characters.Count);
            return RawTextGenerator.characters[index].charWidth;
        }

        private Vector2 RoundedTextPivotLocalPosition(Text textComponent)
        {
            Rect inputRect = textComponent.rectTransform.rect;
            Vector2 textAnchorPivot = Text.GetTextAnchorPivot(textComponent.alignment);
            Vector2 refPoint = Vector2.zero;
            refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
            refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

            Vector2 pixelPerfectRefPoint = textComponent.PixelAdjustPoint(refPoint);

            Vector2 rounddedRefPoint = pixelPerfectRefPoint - refPoint + Vector2.Scale(inputRect.size, textAnchorPivot);
            rounddedRefPoint.x = rounddedRefPoint.x - Mathf.Floor(0.5f + rounddedRefPoint.x);
            rounddedRefPoint.y = rounddedRefPoint.y - Mathf.Floor(0.5f + rounddedRefPoint.y);

            return rounddedRefPoint;
        }

        private int LineFromPosition(Vector2 pos, TextGenerator generator)
        {
            float y = pos.y * _textComponent.pixelsPerUnit;
            float lastBottomY = 0.0f;

            for (int i = 0; i < generator.lineCount; ++i)
            {
                float topY = generator.lines[i].topY;
                float bottomY = topY - generator.lines[i].height;

                if (y > topY)
                {
                    float leading = topY - lastBottomY;
                    if (y > topY - 0.5f * leading)
                        return i - 1;
                    else
                        return i;
                }

                if (y > bottomY)
                    return i;

                lastBottomY = bottomY;
            }

            return generator.lineCount;
        }

        public int RelativeIndexFromPosition(Vector2 pos)
        {
            if (DisplayedTextGenerator.lineCount == 0)
            {
                return 0;
            }

            int line = LineFromPosition(pos, DisplayedTextGenerator);

            if (line < 0)
                return 0;
            if (line >= DisplayedTextGenerator.lineCount)
                return DisplayedTextGenerator.characterCountVisible;

            int startCharIndex = DisplayedTextGenerator.lines[line].startCharIdx;
            int endCharIndex = DisplayedTextGenerator.characterCountVisible;

            for (int i = startCharIndex; i < endCharIndex; i++)
            {
                if (i >= DisplayedTextGenerator.characterCountVisible)
                    break;

                UICharInfo charInfo = DisplayedTextGenerator.characters[i];
                Vector2 charPos = charInfo.cursorPos / _textComponent.pixelsPerUnit;

                float distToCharStart = pos.x - charPos.x;
                float distToCharEnd = charPos.x + (charInfo.charWidth / _textComponent.pixelsPerUnit) - pos.x;
                if (distToCharStart < distToCharEnd)
                    return i;
            }

            return endCharIndex;
        }

        public int LineEndIndex(int line)
        {
            line = Mathf.Max(line, 0);
            if (line + 1 < RawTextGenerator.lines.Count)
                return RawTextGenerator.lines[line + 1].startCharIdx - 1;
            return RawTextGenerator.characterCountVisible;
        }

        public int LineStartIndex(int line)
        {
            line = Mathf.Clamp(line, 0, RawTextGenerator.lines.Count - 1);
            return RawTextGenerator.lines[line].startCharIdx;
        }

        public float LineHeight(int line)
        {
            return RawTextGenerator.lineCount > line ?
            RawTextGenerator.lines[line].height : 0;
        }

        public float LineTop(int line)
        {
            return RawTextGenerator.lines[line].topY;
        }

        public Vector2 CaretOffset()
        {
            return RoundedTextPivotLocalPosition(_textComponent);
        }

        public int GetLineByCharIndex(int caretIndex)
        {
            for (int i = 0; i < RawTextGenerator.lineCount - 1; ++i)
            {
                if (RawTextGenerator.lines[i + 1].startCharIdx > caretIndex)
                    return i;
            }

            return RawTextGenerator.lineCount - 1;
        }

        public int LineDownIndex(bool goToLastChar, int index)
        {
            Assert.IsTrue(index >= 0 && index <= RawTextGenerator.characterCountVisible);

            int currentLine = GetLineByCharIndex(index);
            if (currentLine == RawTextGenerator.lineCount - 1)
            {
                return goToLastChar ? RawTextGenerator.characterCount - 1 : index;
            }

            int lineEndChar = LineEndIndex(currentLine + 1);
            for (int i = RawTextGenerator.lines[currentLine + 1].startCharIdx; i < lineEndChar; i++)
            {
                if (RawTextGenerator.characters[i].cursorPos.x >= RawTextGenerator.characters[index].cursorPos.x)
                {
                    return i;
                }
            }

            return lineEndChar;
        }

        public int LineUpIndex(bool goToFirstChar, int index)
        {
            Assert.IsTrue(index >= 0 && index <= RawTextGenerator.characterCountVisible);

            int currentLine = GetLineByCharIndex(index);
            if (currentLine == 0)
            {
                return goToFirstChar ? 0 : index;
            }

            int lineBeginChar = LineStartIndex(currentLine - 1);
            for (int i = LineEndIndex(currentLine - 1); i > lineBeginChar; i--)
            {
                if (RawTextGenerator.characters[i].cursorPos.x <= RawTextGenerator.characters[index].cursorPos.x)
                {
                    return i;
                }
            }

            return lineBeginChar;
        }

        public void UpdateDisplayText(string text)
        {
            _textComponent.text = text;
        }
    }
}
