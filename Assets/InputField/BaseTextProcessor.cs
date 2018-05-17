using UnityEngine;
using System.Text;
using UnityEngine.Assertions;

namespace Qiyi.UI.InputField
{
    public class BaseTextProcessor : IInputEventProcessor
    {
        private StringBuilder _processedText;
        private ICaretNavigator _caretNavigator;

        static string _clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        public string TextValue
        {
            get
            {
                return _processedText.ToString();
            }

            set
            {
                _processedText = new StringBuilder(value);
            }
        }

        public BaseTextProcessor(StringBuilder textToProcess, ICaretNavigator receiver)
        {
            _processedText = textToProcess;
            _caretNavigator = receiver;
        }

        public bool ProcessEvent(Event keyEvent, int caretIndex, int selectionIndex)
        {
            bool ctrlOnly = Ctrl(keyEvent) && !Alt(keyEvent) && !Shift(keyEvent);

            switch (keyEvent.keyCode)
            {
                case KeyCode.Backspace:
                    Backspace(_processedText, caretIndex, selectionIndex);
                    return true;

                case KeyCode.Delete:
                    ForwardSpace(_processedText, caretIndex, selectionIndex);
                    return true;

                case KeyCode.Home:
                    MoveTextStart(Shift(keyEvent));
                    return true;

                case KeyCode.End:
                    MoveTextEnd(Shift(keyEvent));
                    return true;

                case KeyCode.A:
                    if (ctrlOnly)
                    {
                        SelectAll();
                    }
                    break;

                case KeyCode.X:
                    if (ctrlOnly)
                    {
                        Cut(_processedText, caretIndex, selectionIndex);
                    }
                    break;

                case KeyCode.C:
                    if (ctrlOnly)
                    {
                        Copy(_processedText, caretIndex, selectionIndex);
                    }
                    break;

                case KeyCode.V:
                    if (ctrlOnly)
                    {
                        Paste(_processedText, caretIndex, selectionIndex);
                    }
                    break;

                case KeyCode.LeftArrow:
                    MoveLeft(caretIndex, selectionIndex, Shift(keyEvent));
                    return true;

                case KeyCode.RightArrow:
                    MoveRight(caretIndex, selectionIndex, Shift(keyEvent));
                    return true;

                case KeyCode.UpArrow:
                    MoveUp(caretIndex, selectionIndex, Shift(keyEvent));
                    return true;

                case KeyCode.DownArrow:
                    MoveDown(caretIndex, selectionIndex, Shift(keyEvent));
                    return true;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    return true;
                case KeyCode.Escape:
                    // was canceled.
                    return false;

            }

            char c = keyEvent.character;

            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int)c == 3)
                c = '\n';

            if (c.Equals('\0'))
            {
                return true;
            }

            return HandleInputChar(_processedText, caretIndex, selectionIndex, c);
        }

        private bool HasSelection(int index, int selectionIndex)
        {
            return index != selectionIndex;
        }

        protected virtual bool HandleInputChar(StringBuilder text, int index, int selectionIndex, char c)
        {
            if (HasSelection(index, selectionIndex))
            {
                Delete(text, index, selectionIndex);
            }
            Insert(text, c, index < selectionIndex ? index : selectionIndex);
            return true;
        }

        private bool Ctrl(Event evt)
        {
            return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ?
                (evt.modifiers & EventModifiers.Command) != 0 : (evt.modifiers & EventModifiers.Control) != 0;
        }

        private bool Shift(Event evt)
        {
            return (evt.modifiers & EventModifiers.Shift) != 0;
        }

        private bool Alt(Event evt)
        {
            return (evt.modifiers & EventModifiers.Alt) != 0;
        }

        public void SelectAll()
        {
            _caretNavigator.MoveCaretTo(0, false);
            _caretNavigator.MoveCaretTo(_processedText.Length, true);
        }

        private void Backspace(StringBuilder text, int caretIndex, int selectionIndex)
        {
            if (HasSelection(caretIndex, selectionIndex))
            {
                Delete(text, caretIndex, selectionIndex);
            }
            else if (caretIndex > 0)
            {
                Delete(text, caretIndex - 1, caretIndex);
            }
        }

        private void Delete(StringBuilder text, int start, int end)
        {
            Assert.IsFalse(start == end);
            text.Remove(start < end ? start : end, Mathf.Abs(end - start));
            _caretNavigator.MoveCaretTo(start < end ? start : end, false);
        }

        private void ForwardSpace(StringBuilder text, int caretIndex, int selectionIndex)
        {
            if (HasSelection(caretIndex, selectionIndex))
            {
                Delete(text, caretIndex, selectionIndex);
            }
            else if (caretIndex < text.Length)
            {
                Delete(text, caretIndex, caretIndex + 1);
            }
        }

        private void Append(char c, int index)
        {
            _processedText.Append(c);
            _caretNavigator.MoveCaretTo(index + 1, false);
        }

        private void MoveTextStart(bool shift)
        {
            _caretNavigator.MoveCaretTo(0, shift);
        }

        private void MoveTextEnd(bool shift)
        {
            _caretNavigator.MoveCaretTo(_processedText.Length, shift);
        }

        private string Copy(StringBuilder text, int start, int end)
        {
            _clipboard = text.ToString().Substring(start < end ? start : end, Mathf.Abs(start - end));
            return _clipboard;
        }

        private void Cut(StringBuilder text, int start, int end)
        {
            _clipboard = Copy(text, start, end);
            Delete(text, start, end);
        }

        private void Paste(StringBuilder text, int start, int end)
        {
            foreach (var c in _clipboard)
            {
                HandleInputChar(text, start, end, c);
            }
        }

        private void Insert(StringBuilder text, char insert, int start)
        {
            text.Insert(start, insert);
            _caretNavigator.MoveCaretTo(start + 1, false);
        }

        private void MoveRight(int index, int selectionIndex, bool shift)
        {
            if (HasSelection(index, selectionIndex) && !shift)
            {
                _caretNavigator.MoveCaretTo(Mathf.Max(index, selectionIndex), false);
            }
            else
            {
                _caretNavigator.MoveCaretTo(index + 1, shift);
            }
        }

        private void MoveLeft(int index, int selectionIndex, bool shift)
        {
            if (HasSelection(index, selectionIndex) && !shift)
            {
                _caretNavigator.MoveCaretTo(Mathf.Min(index, selectionIndex), false);
            }
            else
            {
                _caretNavigator.MoveLeft(shift);
            }
        }

        private void MoveUp(int index, int selectionIndex, bool shift)
        {
            if (HasSelection(index, selectionIndex) && !shift)
            {
                _caretNavigator.MoveCaretTo(Mathf.Min(index, selectionIndex), false);
            }
            else
            {
                _caretNavigator.MoveUp(false, shift);
            }
        }

        private void MoveDown(int index, int selectionIndex, bool shift)
        {
            if (HasSelection(index, selectionIndex) && !shift)
            {
                _caretNavigator.MoveCaretTo(Mathf.Max(index, selectionIndex), false);
            }
            else
            {
                _caretNavigator.MoveDown(false, shift);
            }
        }

    }
}
