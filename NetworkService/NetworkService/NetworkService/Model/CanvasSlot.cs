using MVVM1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class CanvasSlot : BindableBase
    {
        private Entity _canvasEntity;
        public Entity CanvasEntity
        {
            get => _canvasEntity;
            set => SetProperty(ref _canvasEntity, value);
        }

        public int Row { get; set; }
        public int Col { get; set; }
    }
}
