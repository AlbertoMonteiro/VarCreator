using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace VariableCreator.Commands
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(OnTabPressCreateVarCommand))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class OnTabPressCreateVarCommand : ICommandHandler<TabKeyCommandArgs>
    {
        private const string VARIABLE_NAME = "yourVariable";
        private const string LINE_START_TEXT = "var " + VARIABLE_NAME + " = ";
        private const string LINE_END_TEXT = ".var";

        public string DisplayName => nameof(OnTabPressCreateVarCommand);

        public bool ExecuteCommand(TabKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            var editor = args.TextView;

            var currentPosition = editor.Caret.Position.BufferPosition;
            var currentLine = currentPosition.GetContainingLine();
            var currentText = currentLine.GetText();
            if (currentText.EndsWith(LINE_END_TEXT))
            {
                var index = currentText.Length - currentText.TrimStart().Length;
                var lineStart = currentLine.Start + index;

                using var edit = editor.TextBuffer.CreateEdit();
                edit.Replace(currentPosition.Position - LINE_END_TEXT.Length, LINE_END_TEXT.Length, ";");
                edit.Insert(lineStart, LINE_START_TEXT);
                var textSnapshot = edit.Apply();
                var variableNameStartIndex = lineStart.Position + LINE_END_TEXT.Length;
                var selectionSpan = new SnapshotSpan(textSnapshot, Span.FromBounds(variableNameStartIndex, variableNameStartIndex + VARIABLE_NAME.Length));
                editor.Selection.Select(selectionSpan, false);
                editor.Caret.MoveTo(selectionSpan.Start);
                return true;
            }
            return false;
        }

        public CommandState GetCommandState(TabKeyCommandArgs args)
        {
            return args.TextView.Caret.Position.BufferPosition.GetContainingLine().GetText().EndsWith(LINE_END_TEXT)
                ? CommandState.Available
                : CommandState.Unavailable;
        }
    }
}