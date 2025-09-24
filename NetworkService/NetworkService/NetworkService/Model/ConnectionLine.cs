using MVVM1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class ConnectionLine : BindableBase
    {
        public int SrcId { get; set; }
        public int DstId { get; set; }

        private double x1;
        public double X1 { get => x1; set => SetProperty(ref x1, value); }

        private double y1;
        public double Y1 { get => y1; set => SetProperty(ref y1, value); }

        private double x2;
        public double X2 { get => x2; set => SetProperty(ref x2, value); }

        private double y2;
        public double Y2 { get => y2; set => SetProperty(ref y2, value); }
    }
}
