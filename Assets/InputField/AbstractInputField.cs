using Qiyi.InputMethod.Keyboard;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Qiyi.UI.InputField {
    public abstract class AbstractInputField : IVrInputField
    {
		public static IKeyboard Keyboard{ set; get; }

        private ICaret _caret;
        private readonly IInputEventProcessor _inputEventProcessor;
        private readonly ITextComponentWrapper _editableText;
        private readonly IInputFieldController _controller;
        private Color _selectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);
        private Color _defaultColor = Color.black;

        protected virtual ITextComponentWrapper EditableText { get { return _editableText; } }
        protected virtual IInputFieldController InputFieldController { get { return _controller; } }

        public virtual Color SelectionColor
        {
            get { return _selectionColor; }
            set { _selectionColor = value; }
        }

        public virtual Color DefaultColor
        {
            get { return _defaultColor; }
            set { _defaultColor = value; }
        }

        private bool _interactive = false;

        public virtual string TextValue
        {
            get { return _inputEventProcessor.TextValue; }
            set
            {
                _inputEventProcessor.TextValue = value;
                _inputEventProcessor.SelectAll();
                UpdateText();
            }
        }

        public string text {
            get {
                return TextValue;
            }
            set {
                TextValue = value;
            }
        }

        protected virtual ICaret Caret
        {
            get { return _caret; }
            set
            {
                if (_caret == value)
                {
                    return;
                }

                _caret.DestroyCaret();
                _caret = value;
            }
        }

        protected AbstractInputField(ICaret caret,
            IInputEventProcessor inputEventProcessor,
            IInputFieldController controller,
            ITextComponentWrapper text)
        {
            _editableText = text;
            _caret = caret;
            _inputEventProcessor = inputEventProcessor;
            _controller = controller;
        }

        public virtual void DrawCaretOrSelection(ITextComponentWrapper text)
        {
            if (Caret.GetIndex() != Caret.GetSelectionIndex())
            {
                DrawSelection(Caret, SelectionColor, text, text.CaretOffset());
            }
            else
            {
                DrawCaret(Caret, DefaultColor, text, text.CaretOffset());
            }
        }

        protected abstract void DrawSelection(ICaret caret, Color color, ITextComponentWrapper text, Vector2 offset);

        protected abstract void DrawCaret(ICaret caret, Color color, ITextComponentWrapper text, Vector2 offset);

        public virtual void ProcessEvent(Event evt)
        {
            if (!IsInteractive())
            {
                return;
            }

            bool shouldContinue = _inputEventProcessor.ProcessEvent(evt, Caret.GetIndex(), Caret.GetSelectionIndex());

            if (shouldContinue)
            {
                //FIXME: update text twice with a single keydown in editor mode.
                UpdateText();
            }
            else
            {
                DeactivateInputField();
            }
        }

        public virtual void ActivateInputField()
        {
            UpdateText();
            _interactive = true;
            Caret.ActivateCaret();
        }

        public virtual void DeactivateInputField()
        {
            _interactive = false;
            Caret.DeactivateCaret();
        }

        public virtual void FinishInput()
        {
            _controller.OnEndInput(TextValue);
        }

        protected abstract string ProcessText(ITextComponentWrapper text, ICaret caret);

        public virtual void UpdateText()
        {
            InputFieldController.PopulateText(TextValue);
            InputFieldController.UpdateDisplayText(ProcessText(EditableText, Caret));
        }

        public virtual bool IsInteractive()
        {
            return _interactive;
        }

        public abstract void OnPointerDown(PointerEventData eventData);

        public abstract void OnDrag(PointerEventData eventData);
        
    }
}
