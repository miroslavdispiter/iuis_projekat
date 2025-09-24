using NetworkService.Model;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Undo
{
    public class AddEntityUndoCommand : IUndoableCommand
    {
        private readonly ObservableCollection<Entity> _entities;
        private readonly Entity _addedEntity;
        private readonly EntitiesViewModel _entitiesViewModel;

        public AddEntityUndoCommand(ObservableCollection<Entity> entities, Entity added, EntitiesViewModel vm)
        {
            _entities = entities;
            _addedEntity = added;
            _entitiesViewModel = vm;
        }

        public void Undo()
        {
            _entities.Remove(_addedEntity);
            _entitiesViewModel.RefreshFilter();
        }
    }
}
