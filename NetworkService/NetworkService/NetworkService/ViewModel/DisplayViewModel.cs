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

        private Entity _draggedEntity;
        public Entity DraggedEntity
        {
            get => _draggedEntity;
            set => SetProperty(ref _draggedEntity, value);
        }

        public MyICommand<object> DragOverCommand { get; private set; }
        public MyICommand<object> DropCommand { get; private set; }
        public MyICommand<object> MouseDownCommand { get; private set; }
        public MyICommand MouseUpCommand { get; private set; }
        public MyICommand<object> TreeViewDropCommand { get; private set; }

        public DisplayViewModel(ObservableCollection<Entity> sharedEntities)
        {
            Entities = sharedEntities;

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

            Entities.CollectionChanged += (s, e) => RefreshGroups();

            DragOverCommand = new MyICommand<object>(OnDragOver);
            DropCommand = new MyICommand<object>(OnDrop);
            MouseDownCommand = new MyICommand<object>(OnMouseDown);
            MouseUpCommand = new MyICommand(OnMouseUp);
            TreeViewDropCommand = new MyICommand<object>(OnTreeViewDrop);
        }

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
                }
                else
                {
                    MessageBox.Show("Slot već zauzet!");
                }

                DraggedEntity = null;
            }
        }

        private void OnTreeViewDrop(object obj)
        {
            if (DraggedEntity != null)
            {
                var oldSlot = CanvasSlots.FirstOrDefault(s => s.CanvasEntity == DraggedEntity);
                if (oldSlot != null)
                    oldSlot.CanvasEntity = null;

                AddToGroups(DraggedEntity);

                DraggedEntity = null;
            }
        }

        private void RemoveFromGroups(Entity e)
        {
            foreach (var g in EntityGroups)
                g.Entities.Remove(e);
        }

        private void AddToGroups(Entity e)
        {
            if (e.Type.Name == "Solar Panel" && !EntityGroups[0].Entities.Contains(e))
                EntityGroups[0].Entities.Add(e);
            else if (e.Type.Name == "Wind Generator" && !EntityGroups[1].Entities.Contains(e))
                EntityGroups[1].Entities.Add(e);
        }
    }
}
