using MVVM1;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Undo
{
    public class ChangeViewUndoCommand : IUndoableCommand
    {
        private readonly MainWindowViewModel _mainVm;
        private readonly BindableBase _previousVm;
        private readonly BindableBase _newVm;

        public ChangeViewUndoCommand(MainWindowViewModel mainVm, BindableBase prev, BindableBase next)
        {
            _mainVm = mainVm;
            _previousVm = prev;
            _newVm = next;
        }

        public void Undo()
        {
            _mainVm.CurrentViewModel = _previousVm;
            // UndoManager.Register(new ChangeViewUndoCommand(_mainVm, _newVm, _previousVm));
        }
    }
}
