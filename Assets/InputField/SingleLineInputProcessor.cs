using System.Text;

namespace Qiyi.UI.InputField {
    public class SingleLineInputProcessor : BaseTextProcessor
    {
        public SingleLineInputProcessor(StringBuilder textToProcess, ICaretNavigator receiver) : base(textToProcess, receiver)
        {
        }

        protected override bool HandleInputChar(StringBuilder text, int index, int selectionIndex, char c)
        {
            // Don't allow return chars or tabulator key to be entered into single line fields.
            if (c == '\t' || c == '\r' || c == 10)
            {
                return true;
            }

            return base.HandleInputChar(text, index, selectionIndex, c);
        }
    }
}
