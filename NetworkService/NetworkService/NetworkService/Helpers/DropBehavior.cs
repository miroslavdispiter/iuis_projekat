using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NetworkService.Helpers
{
    public static class DropBehavior
    {
        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached(
                "DropCommand",
                typeof(ICommand),
                typeof(DropBehavior),
                new PropertyMetadata(null, OnDropCommandChanged));

        public static void SetDropCommand(UIElement element, ICommand value)
        {
            element.SetValue(DropCommandProperty, value);
        }

        public static ICommand GetDropCommand(UIElement element)
        {
            return (ICommand)element.GetValue(DropCommandProperty);
        }

        private static void OnDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                uiElement.Drop -= UiElement_Drop;

                if (e.NewValue is ICommand)
                {
                    uiElement.Drop += UiElement_Drop;
                }
            }
        }

        private static void UiElement_Drop(object sender, DragEventArgs e)
        {
            var element = (UIElement)sender;
            var command = GetDropCommand(element);

            if (command != null)
            {
                var pos = e.GetPosition(element);

                if (command.CanExecute(pos))
                    command.Execute(pos);
            }
        }
    }
}
