using UnityEngine;
using UnityEngine.EventSystems;

namespace Qiyi.UI.InputField
{
    public interface ITextComponentWrapper
    {
        Vector2 MousePositionInTextRect(PointerEventData eventData);

        Rect DisplayRect { get; }

        int DisplayedTextLength { get; }

        int LineCount { get; }

        float LineHeight(int line);

        float LineTop(int line);

        int LineEndIndex(int line);

        int LineStartIndex(int line);

        void Populate(string text, GameObject context);

        float CharWidth(int index);

        Vector2 CursorPositionAt(int relativeIndex);

        Vector2 CaretOffset();

        void UpdateDisplayText(string text);

        int GetLineByCharIndex(int index);

        int RelativeIndexFromPosition(Vector2 position);

        int LineDownIndex(bool goToLastChar, int currentIndex);

        int LineUpIndex(bool goToFirstChar, int currentIndex);

    }
}
