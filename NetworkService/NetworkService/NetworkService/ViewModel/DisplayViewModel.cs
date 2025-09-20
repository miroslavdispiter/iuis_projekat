using MVVM1;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class DisplayViewModel : BindableBase
    {
        private Entity _selectedEntity;
        public ObservableCollection<Entity> Entities { get; set; }
        public ObservableCollection<Entity> SolarEntities { get; set; }
        public ObservableCollection<Entity> WindEntities { get; set; }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public Entity SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (_selectedEntity != value)
                {
                    _selectedEntity = value;
                    OnPropertyChanged(nameof(SelectedEntity));
                    StatusMessage = $"Selected: {SelectedEntity?.Name ?? "None"}";
                }
            }
        }

        public DisplayViewModel(ObservableCollection<Entity> sharedEntities)
        {
            Entities = sharedEntities;

            SolarEntities = new ObservableCollection<Entity>();
            WindEntities = new ObservableCollection<Entity>();
            RefreshGroups();

            Entities.CollectionChanged += Entities_CollectionChanged;

            StatusMessage = "Display initialized.";
        }

        private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshGroups();
        }

        private void RefreshGroups()
        {
            SolarEntities.Clear();
            WindEntities.Clear();

            foreach (var e in Entities)
            {
                if (e.Type.Name == "Solar Panel")
                    SolarEntities.Add(e);
                else if (e.Type.Name == "Wind Generator")
                    WindEntities.Add(e);
            }
        }
    }
}
