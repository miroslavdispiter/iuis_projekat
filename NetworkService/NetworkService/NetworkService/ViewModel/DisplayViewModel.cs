using MVVM1;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService.ViewModel
{
    public class DisplayViewModel : BindableBase
    {
        public ObservableCollection<Entity> Entities { get; set; }
        public ObservableCollection<EntityGroup> EntityGroups { get; set; }
        public ObservableCollection<CanvasSlot> CanvasSlots { get; set; }
        public ObservableCollection<ConnectionLine> Linije { get; set; }

        private Entity _draggedEntity;
        public Entity DraggedEntity
        {
            get => _draggedEntity;
            set => SetProperty(ref _draggedEntity, value);
        }

        private Entity _firstSelectedForLine;
        public MyICommand<object> ConnectCommand { get; private set; }
        public MyICommand<object> DragOverCommand { get; private set; }
        public MyICommand<object> DropCommand { get; private set; }
        public MyICommand<object> MouseDownCommand { get; private set; }
        public MyICommand MouseUpCommand { get; private set; }
        public MyICommand<object> TreeViewDropCommand { get; private set; }

        public DisplayViewModel(ObservableCollection<Entity> sharedEntities)
        {
            Entities = sharedEntities;
            Linije = new ObservableCollection<ConnectionLine>();

            EntityGroups = new ObservableCollection<EntityGroup>
            {
                new EntityGroup("Solar Panels"),
                new EntityGroup("Wind Generators")
            };

            CanvasSlots = new ObservableCollection<CanvasSlot>();
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 4; c++)
                    CanvasSlots.Add(new CanvasSlot { Row = r, Col = c, CanvasEntity = null });

            RefreshGroups();

            Entities.CollectionChanged += (s, e) =>
            {
                RefreshGroups();

                // LINIJE: brisanje ako entitet obrisan
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (Entity removed in e.OldItems)
                    {
                        for (int i = Linije.Count - 1; i >= 0; i--)
                        {
                            if (Linije[i].SrcId == removed.Id || Linije[i].DstId == removed.Id)
                                Linije.RemoveAt(i);
                        }
                    }
                }
            };

            DragOverCommand = new MyICommand<object>(OnDragOver);
            DropCommand = new MyICommand<object>(OnDrop);
            MouseDownCommand = new MyICommand<object>(OnMouseDown);
            MouseUpCommand = new MyICommand(OnMouseUp);
            TreeViewDropCommand = new MyICommand<object>(OnTreeViewDrop);
            ConnectCommand = new MyICommand<object>(OnConnectCommand);
        }

        // === POMOĆNE ===
        private void RefreshGroups()
        {
            foreach (var group in EntityGroups)
                group.Entities.Clear();

            foreach (var e in Entities)
            {
                if (!CanvasSlots.Any(s => s.CanvasEntity == e))
                {
                    if (e.Type.Name == "Solar Panel")
                        EntityGroups[0].Entities.Add(e);
                    else if (e.Type.Name == "Wind Generator")
                        EntityGroups[1].Entities.Add(e);
                }
            }
        }

        private Point GetSlotCenter(CanvasSlot slot)
        {
            double cellWidth = 800 / 4;
            double cellHeight = 450 / 3;
            double x = slot.Col * cellWidth + cellWidth / 2;
            double y = slot.Row * cellHeight + cellHeight / 2;
            return new Point(x, y);
        }

        private void UpdateLinesForEntity(Entity entity)
        {
            var slot = CanvasSlots.FirstOrDefault(s => s.CanvasEntity?.Id == entity.Id);
            if (slot == null) return;

            var center = GetSlotCenter(slot);
            foreach (var line in Linije)
            {
                if (line.SrcId == entity.Id)
                {
                    line.X1 = center.X; line.Y1 = center.Y;
                }
                if (line.DstId == entity.Id)
                {
                    line.X2 = center.X; line.Y2 = center.Y;
                }
            }
        }

        // === LINIJE ===
        private void OnConnectCommand(object obj)
        {
            if (!(obj is Entity selected)) return;

            if (_firstSelectedForLine == null)
            {
                _firstSelectedForLine = selected;
                return;
            }

            if (_firstSelectedForLine != null && selected != _firstSelectedForLine)
            {
                if (_firstSelectedForLine.Type.Name == selected.Type.Name)
                {
                    MessageBox.Show("Možete povezati samo Solar i Wind generator!");
                    _firstSelectedForLine = null;
                    return;
                }

                if (Linije.Any(l =>
                    (l.SrcId == _firstSelectedForLine.Id && l.DstId == selected.Id) ||
                    (l.SrcId == selected.Id && l.DstId == _firstSelectedForLine.Id)))
                {
                    MessageBox.Show("Veza već postoji!");
                    _firstSelectedForLine = null;
                    return;
                }

                var srcSlot = CanvasSlots.FirstOrDefault(s => s.CanvasEntity?.Id == _firstSelectedForLine.Id);
                var dstSlot = CanvasSlots.FirstOrDefault(s => s.CanvasEntity?.Id == selected.Id);
                if (srcSlot == null || dstSlot == null)
                {
                    MessageBox.Show("Oba entiteta moraju biti na canvas mreži!");
                    _firstSelectedForLine = null;
                    return;
                }

                var srcCenter = GetSlotCenter(srcSlot);
                var dstCenter = GetSlotCenter(dstSlot);

                Linije.Add(new ConnectionLine
                {
                    SrcId = _firstSelectedForLine.Id.Value,
                    DstId = selected.Id.Value,
                    X1 = srcCenter.X,
                    Y1 = srcCenter.Y,
                    X2 = dstCenter.X,
                    Y2 = dstCenter.Y
                });

                _firstSelectedForLine = null;
            }
        }

        // === DRAG&DROP ===
        private void OnMouseDown(object obj)
        {
            if (obj is Entity entity)
            {
                DraggedEntity = entity;
                DragDrop.DoDragDrop(new TextBlock(), entity, DragDropEffects.Move);
            }
        }

        private void OnMouseUp() => DraggedEntity = null;

        private void OnDragOver(object obj)
        {
            if (obj is DragEventArgs e && e.Source is FrameworkElement fe)
            {
                if (fe.DataContext is CanvasSlot slot)
                {
                    e.Effects = slot.CanvasEntity == null ? DragDropEffects.Move : DragDropEffects.None;
                    e.Handled = true;
                }
            }
        }

        private void OnDrop(object obj)
        {
            if (!(obj is DragEventArgs e)) return;
            if (!(e.Source is FrameworkElement fe)) return;
            if (!(fe.DataContext is CanvasSlot slot)) return;

            if (DraggedEntity != null)
            {
                var oldSlot = CanvasSlots.FirstOrDefault(s => s.CanvasEntity == DraggedEntity);
                if (oldSlot != null)
                    oldSlot.CanvasEntity = null;

                if (slot.CanvasEntity == null)
                {
                    slot.CanvasEntity = DraggedEntity;
                    RemoveFromGroups(DraggedEntity);
                    UpdateLinesForEntity(DraggedEntity);
                }
                else
                {
                    MessageBox.Show("Slot vec zauzet!");
                }

                DraggedEntity = null;
            }
        }

        // === TREEVIEW DROP (vracanje) ===
        private void OnTreeViewDrop(object obj)
        {
            if (DraggedEntity != null)
            {
                var oldSlot = CanvasSlots.FirstOrDefault(s => s.CanvasEntity == DraggedEntity);
                if (oldSlot != null)
                    oldSlot.CanvasEntity = null;

                // obrisati sve linije vezane za taj entitet
                for (int i = Linije.Count - 1; i >= 0; i--)
                {
                    if (Linije[i].SrcId == DraggedEntity.Id || Linije[i].DstId == DraggedEntity.Id)
                        Linije.RemoveAt(i);
                }

                AddToGroups(DraggedEntity);
                DraggedEntity = null;
            }
        }

        // === GROUPS ===
        private void RemoveFromGroups(Entity e) { foreach (var g in EntityGroups) g.Entities.Remove(e); }
        private void AddToGroups(Entity e)
        {
            if (e.Type.Name == "Solar Panel" && !EntityGroups[0].Entities.Contains(e))
                EntityGroups[0].Entities.Add(e);
            else if (e.Type.Name == "Wind Generator" && !EntityGroups[1].Entities.Contains(e))
                EntityGroups[1].Entities.Add(e);
        }
    }
}
