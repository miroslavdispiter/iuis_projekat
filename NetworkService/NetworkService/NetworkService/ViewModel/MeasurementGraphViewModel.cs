using MVVM1;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        public ObservableCollection<Entity> Entities { get; set; }

        private Entity _selectedEntity;
        public Entity SelectedEntity
        {
            get => _selectedEntity;
            set
            {
                if (_selectedEntity != value)
                {
                    _selectedEntity = value;
                    OnPropertyChanged(nameof(SelectedEntity));
                    StatusMessage = $"Selected entity: {SelectedEntity?.Name ?? "None"}";

                    LoadMeasurementsForEntity();
                }
            }
        }

        public ObservableCollection<GraphPoint> Measurements { get; set; }

        private PointCollection _graphPoints;
        public PointCollection GraphPoints
        {
            get => _graphPoints;
            set { _graphPoints = value; OnPropertyChanged(nameof(GraphPoints)); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public MeasurementGraphViewModel(ObservableCollection<Entity> sharedEntities)
        {
            Entities = sharedEntities;
            Measurements = new ObservableCollection<GraphPoint>();
            StatusMessage = "Measurement Graph initialized.";

            // pratimo promene u kolekciji i u entitetima
            Entities.CollectionChanged += Entities_CollectionChanged;
            foreach (var e in Entities)
                e.PropertyChanged += Entity_PropertyChanged;

            if (Entities.Any())
                SelectedEntity = Entities.First();
        }

        private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Entity entity in e.NewItems)
                    entity.PropertyChanged += Entity_PropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (Entity entity in e.OldItems)
                    entity.PropertyChanged -= Entity_PropertyChanged;
            }
        }

        private void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Entity.Value))
            {
                if (SelectedEntity != null && sender is Entity changed && changed.Id == SelectedEntity.Id)
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(100);
                        LoadMeasurementsForEntity();
                    });
                }
            }
        }

        private void LoadMeasurementsForEntity()
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(LoadMeasurementsForEntity);
                return;
            }

            Measurements.Clear();
            GraphPoints = new PointCollection();

            if (SelectedEntity == null) return;

            string logFile = "log.txt";
            if (!File.Exists(logFile))
            {
                StatusMessage = "Log file not found.";
                return;
            }

            var lines = File.ReadAllLines(logFile)
                .Where(l => l.Contains($"Entity_{SelectedEntity.Id}"))
                .Reverse().Take(5).Reverse()   // >>> poslednjih 5 merenja
                .ToList();

            if (lines.Count == 0)
            {
                StatusMessage = $"No data for entity {SelectedEntity.Name}";
                return;
            }

            var parsedLines = lines.Select(line =>
            {
                try
                {
                    var parts = line.Split(',');
                    DateTime ts = DateTime.ParseExact(parts[0].Trim(), "dd.MM.yyyy HH:mm:ss", null);
                    double val = double.Parse(parts[2].Trim());
                    return new { Timestamp = ts, Value = val };
                }
                catch { return null; }
            }).Where(x => x != null).ToList();

            if (parsedLines.Count == 0)
            {
                StatusMessage = $"No valid data for {SelectedEntity.Name}";
                return;
            }

            double maxValue = Math.Max(10, parsedLines.Max(x => x.Value));
            double xStep = parsedLines.Count > 1 ? 500.0 / (parsedLines.Count - 1) : 500.0;

            double graphWidth = 600;
            double leftMargin = 50;
            double rightMargin = 50;
            double usableWidth = graphWidth - leftMargin - rightMargin;
            int sections = parsedLines.Count;
            double sectionWidth = usableWidth / sections;

            int index = 0;
            foreach (var item in parsedLines)
            {
                bool isValid = item.Value >= 1 && item.Value <= 5;

                double x = leftMargin + (index * sectionWidth) + sectionWidth / 2;
                double y = 250 - (item.Value / maxValue) * 200;

                var point = new GraphPoint
                {
                    Timestamp = item.Timestamp,
                    Value = item.Value,
                    RelativeX = x,
                    RelativeY = y,
                    Color = isValid ? Brushes.Blue : Brushes.Red,
                    Label = item.Timestamp.ToString("HH:mm")
                };

                Measurements.Add(point);
                GraphPoints.Add(new System.Windows.Point(point.RelativeX, point.RelativeY));
                index++;
            }

            StatusMessage = $"Loaded {Measurements.Count} points for {SelectedEntity.Name}";
        }
    }
}
