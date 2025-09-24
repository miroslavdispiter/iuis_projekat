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
    public class DeleteEntityUndoCommand : IUndoableCommand
    {
        private readonly ObservableCollection<Entity> _entities;
        private readonly Entity _deletedEntity;
        private readonly EntitiesViewModel _entitiesViewModel;

        public DeleteEntityUndoCommand(ObservableCollection<Entity> entities, Entity deleted, EntitiesViewModel vm)
        {
            _entities = entities;
            _deletedEntity = deleted;
            _entitiesViewModel = vm;
        }

        public void Undo()
        {
            _entities.Add(_deletedEntity);
            _entitiesViewModel.RefreshFilter();
        }
    }
}
