using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Undo
{
    public static class UndoManager
    {
        private static readonly Stack<IUndoableCommand> undoStack = new Stack<IUndoableCommand>();

        public static void Register(IUndoableCommand cmd)
        {
            undoStack.Push(cmd);
        }

        public static void Undo()
        {
            if (undoStack.Count > 0)
            {
                var cmd = undoStack.Pop();
                cmd.Undo();
            }
        }
    }

    public interface IUndoableCommand
    {
        void Undo();
    }
}
