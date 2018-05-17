using UnityEngine;
using UnityEngine.UI;

namespace Qiyi.UI.InputField
{
    public interface ICaret
    {
        IInputFieldController InputFieldController { get; set; }

        void ActivateCaret();

        void DeactivateCaret();

        void Draw(Rect drawRect, Color color, VertexHelper helper);

        int GetIndex();

        int GetSelectionIndex();

        void DestroyCaret();

        bool IsVisible();

        void MoveTo(int index, bool withSelection);
    }
}
