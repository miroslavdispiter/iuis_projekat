using NetworkService.Model;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Undo
{
    public class AddConnectionUndoCommand : IUndoableCommand
    {
        private readonly ConnectionLine _line;
        private readonly DisplayViewModel _vm;

        public AddConnectionUndoCommand(ConnectionLine line, DisplayViewModel vm)
        {
            _line = line;
            _vm = vm;
        }

        public void Undo()
        {
            _vm.Linije.Remove(_line);
        }
    }
}
