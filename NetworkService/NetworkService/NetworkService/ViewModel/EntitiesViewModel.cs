using MVVM1;
using NetworkService.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class EntitiesViewModel : BindableBase
    {
        private Entity _selectedEntity;
        private Entity _currentEntity = new Entity();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        private bool _searchByName = true;
        public bool SearchByName
        {
            get => _searchByName;
            set { _searchByName = value; OnPropertyChanged(nameof(SearchByName)); }
        }

        private bool _searchByType;
        public bool SearchByType
        {
            get => _searchByType;
            set { _searchByType = value; OnPropertyChanged(nameof(SearchByType)); }
        }

        public ObservableCollection<Entity> Entities { get; set; }
        public ObservableCollection<Entity> FilteredEntities { get; set; }
        public ObservableCollection<EntityType> AvailableTypes { get; set; }

        public Entity CurrentEntity
        {
            get => _currentEntity;
            set { _currentEntity = value; OnPropertyChanged(nameof(CurrentEntity)); }
        }

        public Entity SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                _selectedEntity = value;
                OnPropertyChanged(nameof(SelectedEntity));
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public MyICommand AddCommand { get; private set; }
        public MyICommand DeleteCommand { get; private set; }

        public MyICommand ApplySearchCommand { get; private set; }
        public MyICommand ClearSearchCommand { get; private set; }

        public EntitiesViewModel(ObservableCollection<Entity> sharedEntities)
        {
            Entities = sharedEntities;
            FilteredEntities = new ObservableCollection<Entity>(Entities);

            AvailableTypes = new ObservableCollection<EntityType>
            {
                new EntityType { Name = "Solar Panel", ImagePath = "/Images/solar.png" },
                new EntityType { Name = "Wind Generator", ImagePath = "/Images/wind.png" }
            };

            AddCommand = new MyICommand(OnAdd);
            DeleteCommand = new MyICommand(OnDelete, CanDelete);
            ApplySearchCommand = new MyICommand(OnApplySearch);
            ClearSearchCommand = new MyICommand(OnClearSearch);
        }

        private void OnAdd()
        {
            CurrentEntity.Validate();
            if (CurrentEntity.IsValid)
            {
                if (Entities.Any(e => e.Id == CurrentEntity.Id))
                {
                    MessageBox.Show($"Entity with ID {CurrentEntity.Id} already exists. Please choose a unique ID.");
                    return;
                }

                Entities.Add(new Entity
                {
                    Id = CurrentEntity.Id,
                    Name = CurrentEntity.Name,
                    Type = CurrentEntity.Type,
                    Value = CurrentEntity.Value
                });

                RefreshFilter();
                CurrentEntity = new Entity();
            }
        }

        private void OnDelete()
        {
            if (SelectedEntity != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete entity '{SelectedEntity.Name}' (ID: {SelectedEntity.Id})?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Entities.Remove(SelectedEntity);
                    RefreshFilter();
                }
            }
        }

        private bool CanDelete()
        {
            return SelectedEntity != null;
        }

        private void OnApplySearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            var search = SearchText.ToLower();

            var filtered = Entities.Where(e =>
                (SearchByName && e.Name.ToLower().Contains(search)) ||
                (SearchByType && e.Type != null && e.Type.Name.ToLower().Contains(search))
            ).ToList();

            FilteredEntities.Clear();
            foreach (var e in filtered)
                FilteredEntities.Add(e);
        }

        private void OnClearSearch()
        {
            SearchText = string.Empty;
            RefreshFilter();
        }

        private void RefreshFilter()
        {
            FilteredEntities.Clear();
            foreach (var e in Entities)
                FilteredEntities.Add(e);
        }
    }
}