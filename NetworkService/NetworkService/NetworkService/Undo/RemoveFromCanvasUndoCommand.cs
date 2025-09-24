using NetworkService.Model;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Undo
{
    public class RemoveFromCanvasUndoCommand : IUndoableCommand
    {
        private readonly CanvasSlot _slot;
        private readonly Entity _entity;
        private readonly List<ConnectionLine> _removedLines;
        private readonly DisplayViewModel _vm;

        public RemoveFromCanvasUndoCommand(CanvasSlot slot, Entity entity, List<ConnectionLine> removedLines, DisplayViewModel vm)
        {
            _slot = slot;
            _entity = entity;
            _removedLines = removedLines;
            _vm = vm;
        }

        public void Undo()
        {
            _slot.CanvasEntity = _entity;

            foreach (var line in _removedLines)
                _vm.Linije.Add(line);

            typeof(DisplayViewModel).GetMethod("RefreshGroups",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_vm, null);
        }
    }
}
