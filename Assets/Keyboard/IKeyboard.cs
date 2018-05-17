using Qiyi.UI.InputField;
using UnityEngine;

namespace Qiyi.InputMethod.Keyboard
{
    public enum FinishAction{
        Done,
        Search,
    }

	public interface IKeyboard
	{
		bool IsActive ();

		void SetActive (bool active);

		bool IsDone ();

		bool WasCanceled ();

        void FinishInput (FinishAction action);

		void ProcessKeyDown (KeyCode code, char c, EventModifiers modifiers);

		void SetInputField (IVrInputField inputField);

		IVrInputField GetInputField ();

		bool IsCapsLock ();

		void ClearInput ();

		SupportedInputMethod GetCurrentInputMethod ();
		 
		void InputSequence (string input);
	}
}
