using MVVM1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkService.Model
{
    public class EntityConnection : BindableBase
    {
        public Entity Entity1 { get; set; }
        public Entity Entity2 { get; set; }

        private Point _point1;
        public Point Point1
        {
            get => _point1;
            set => SetProperty(ref _point1, value);
        }

        private Point _point2;
        public Point Point2
        {
            get => _point2;
            set => SetProperty(ref _point2, value);
        }
    }
}
