using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class GraphPoint : MeasurementPoint
    {
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
    }
}
