using NetworkService.Model;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Undo
{
    public class MoveEntityUndoCommand : IUndoableCommand
    {
        private readonly CanvasSlot _oldSlot;
        private readonly CanvasSlot _newSlot;
        private readonly Entity _entity;
        private readonly DisplayViewModel _vm;

        public MoveEntityUndoCommand(CanvasSlot oldSlot, CanvasSlot newSlot, Entity entity, DisplayViewModel vm)
        {
            _oldSlot = oldSlot;
            _newSlot = newSlot;
            _entity = entity;
            _vm = vm;
        }

        public void Undo()
        {
            if (_newSlot != null) _newSlot.CanvasEntity = null;

            if (_oldSlot != null) _oldSlot.CanvasEntity = _entity;

            var updateMethod = typeof(DisplayViewModel).GetMethod("UpdateLinesForEntity",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            updateMethod.Invoke(_vm, new object[] { _entity });

            typeof(DisplayViewModel).GetMethod("RefreshGroups",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_vm, null);
        }
    }
}
