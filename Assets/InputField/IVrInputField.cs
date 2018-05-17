using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Qiyi.UI.InputField
{
    public interface IVrInputField
    {
        string TextValue { get; set; }

        [ObsoleteAttribute("use TextValue property instead.")]
        string text { get; set; }

        void ProcessEvent(Event evt);

        void ActivateInputField();

        void DeactivateInputField();

        void FinishInput();

        void UpdateText();

        bool IsInteractive();
    }
}

