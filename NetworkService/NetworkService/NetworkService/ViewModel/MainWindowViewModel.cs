using MVVM1;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        private BindableBase _currentViewModel;
        public MyICommand<string> NavCommand { get; private set; }
        public ObservableCollection<Entity> Entities { get; set; }

        private EntitiesViewModel entitiesViewModel;
        private DisplayViewModel displayViewModel;
        private MeasurementGraphViewModel graphViewModel;

        private string _terminalInput;
        public string TerminalInput
        {
            get => _terminalInput;
            set => SetProperty(ref _terminalInput, value);
        }

        public BindableBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MyICommand TerminalEnterCommand { get; private set; }

        public MainWindowViewModel()
        {
            var solarType = new EntityType { Name = "Solar Panel", ImagePath = "/Images/solar.png" };
            var windType = new EntityType { Name = "Wind Generator", ImagePath = "/Images/wind.png" };

            Entities = new ObservableCollection<Entity>
            {
                new Entity { Id = 0, Name = "Solar 1", Type = solarType, Value = 5 },
                new Entity { Id = 1, Name = "Solar 2", Type = solarType, Value = 1 },
                new Entity { Id = 2, Name = "Wind 1", Type = windType, Value = 3 },
                new Entity { Id = 3, Name = "Wind 2", Type = windType, Value = 6 }
            };

            entitiesViewModel = new EntitiesViewModel(Entities);
            displayViewModel = new DisplayViewModel(Entities);
            graphViewModel = new MeasurementGraphViewModel(Entities);

            Entities.CollectionChanged += Entities_CollectionChanged;

            NavCommand = new MyICommand<string>(OnNav);
            TerminalEnterCommand = new MyICommand(OnTerminalEnter);
            CurrentViewModel = entitiesViewModel;

            CreateListener();
        }

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "entities":
                    CurrentViewModel = entitiesViewModel;
                    break;
                case "display":
                    CurrentViewModel = displayViewModel;
                    break;
                case "graph":
                    CurrentViewModel = graphViewModel;
                    break;
            }
        }

        private void OnTerminalEnter()
        {
            if (string.IsNullOrWhiteSpace(TerminalInput))
                return;

            string input = TerminalInput.Trim();
            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                TerminalInput = string.Empty;
                return;
            }

            string command = parts[0].ToLower();

            // === HELP ===
            if (command == "help")
            {
                string helpText = "Available commands:\n";

                if (CurrentViewModel == entitiesViewModel)
                {
                    helpText += " - add <id> <name> <type> <value>\n";
                    helpText += " - delete <id>\n";
                    helpText += " - search <text> <name|type>\n";
                    helpText += " - clearsearch\n";
                    helpText += " - nav <entities|display|graph>\n";
                }
                else if (CurrentViewModel == displayViewModel)
                {
                    helpText += " - place <entityId> <row> <col>\n";
                    helpText += " - remove <entityId>\n";
                    helpText += " - clearcanvas\n";
                    helpText += " - nav <entities|display|graph>\n";
                }
                else if (CurrentViewModel == graphViewModel)
                {
                    helpText += " - select <entityName>\n";
                    helpText += " - nav <entities|display|graph>\n";
                }
                else
                {
                    helpText += " - nav <entities|display|graph>\n";
                }

                // dodaj hint na kraj
                helpText += "\nPress Enter to continue...";

                TerminalInput = helpText;
                return;
            }

            // === ADD ===
            if (command == "add")
            {
                if (CurrentViewModel != entitiesViewModel)
                {
                    TerminalInput = string.Empty;
                    return;
                }
                if (parts.Length < 5)
                {
                    TerminalInput = string.Empty;
                    return;
                }

                try
                {
                    int id = int.Parse(parts[1]);
                    string name = parts[2];

                    string typeName = "";
                    for (int i = 3; i < parts.Length - 1; i++)
                    {
                        if (i > 3) typeName += " ";
                        typeName += parts[i];
                    }

                    typeName = typeName.Replace("\"", "");

                    double value = double.Parse(parts[parts.Length - 1]);

                    var type = entitiesViewModel.AvailableTypes
                        .FirstOrDefault(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));

                    if (type == null) { TerminalInput = string.Empty; return; }

                    entitiesViewModel.CurrentEntity = new Entity
                    {
                        Id = id,
                        Name = name,
                        Type = type,
                        Value = value
                    };

                    ((ICommand)entitiesViewModel.AddCommand).Execute(null);
                }
                catch { }

                TerminalInput = string.Empty;
                return;
            }

            // === DELETE ===
            if (command == "delete")
            {
                if (CurrentViewModel != entitiesViewModel)
                {
                    TerminalInput = string.Empty;
                    return;
                }

                if (parts.Length < 2)
                {
                    TerminalInput = string.Empty;
                    return;
                }

                try
                {
                    int id = int.Parse(parts[1]);

                    var entityToDelete = entitiesViewModel.Entities
                        .FirstOrDefault(e => e.Id == id);

                    if (entityToDelete != null)
                    {
                        entitiesViewModel.SelectedEntity = entityToDelete;
                        ((ICommand)entitiesViewModel.DeleteCommand).Execute(null);
                    }
                }
                catch { }

                TerminalInput = string.Empty;
                return;
            }

            // === SEARCH ===
            if (command == "search")
            {
                if (CurrentViewModel != entitiesViewModel)
                {
                    TerminalInput = string.Empty;
                    return;
                }
                if (parts.Length < 3) { TerminalInput = string.Empty; return; }

                string searchText = parts[1];
                string mode = parts[2].ToLower();

                entitiesViewModel.SearchText = searchText;
                entitiesViewModel.SearchByName = mode == "name";
                entitiesViewModel.SearchByType = mode == "type";

                ((ICommand)entitiesViewModel.ApplySearchCommand).Execute(null);

                TerminalInput = string.Empty;
                return;
            }

            // === CLEAR SEARCH ===
            if (command == "clearsearch")
            {
                if (CurrentViewModel != entitiesViewModel)
                {
                    TerminalInput = string.Empty;
                    return;
                }
                ((ICommand)entitiesViewModel.ClearSearchCommand).Execute(null);

                TerminalInput = string.Empty;
                return;
            }

            // === SELECT ENTITY (GraphView) ===
            if (command == "select")
            {
                if (CurrentViewModel != graphViewModel)
                {
                    TerminalInput = string.Empty;
                    return;
                }
                if (parts.Length < 2) { TerminalInput = string.Empty; return; }

                string entityName = string.Join(" ", parts.Skip(1));
                var entityToSelect = graphViewModel.Entities
                    .FirstOrDefault(e => string.Equals(e.Name, entityName, StringComparison.OrdinalIgnoreCase));

                if (entityToSelect != null)
                    graphViewModel.SelectedEntity = entityToSelect;

                TerminalInput = string.Empty;
                return;
            }

            // === DISPLAY VIEW COMMANDS ===
            if (CurrentViewModel == displayViewModel)
            {
                // place <entityId> <row> <col>
                if (command == "place")
                {
                    if (parts.Length < 4) { TerminalInput = string.Empty; return; }
                    try
                    {
                        int id = int.Parse(parts[1]);
                        int row = int.Parse(parts[2]);
                        int col = int.Parse(parts[3]);

                        var entity = displayViewModel.Entities.FirstOrDefault(e => e.Id == id);
                        var slot = displayViewModel.CanvasSlots.FirstOrDefault(s => s.Row == row && s.Col == col);

                        if (entity != null && slot != null)
                        {
                            var oldSlot = displayViewModel.CanvasSlots.FirstOrDefault(s => s.CanvasEntity == entity);
                            if (oldSlot != null)
                                oldSlot.CanvasEntity = null;

                            if (slot.CanvasEntity == null)
                            {
                                slot.CanvasEntity = entity;
                                typeof(DisplayViewModel).GetMethod("RemoveFromGroups",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                    .Invoke(displayViewModel, new object[] { entity });
                            }
                        }
                    }
                    catch { }
                    TerminalInput = string.Empty;
                    return;
                }

                // remove <entityId>
                if (command == "remove")
                {
                    if (parts.Length < 2) { TerminalInput = string.Empty; return; }
                    try
                    {
                        int id = int.Parse(parts[1]);
                        var entity = displayViewModel.Entities.FirstOrDefault(e => e.Id == id);
                        var slot = displayViewModel.CanvasSlots.FirstOrDefault(s => s.CanvasEntity == entity);

                        if (entity != null && slot != null)
                        {
                            slot.CanvasEntity = null;
                            typeof(DisplayViewModel).GetMethod("AddToGroups",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .Invoke(displayViewModel, new object[] { entity });
                        }
                    }
                    catch { }
                    TerminalInput = string.Empty;
                    return;
                }

                // clearcanvas
                if (command == "clearcanvas")
                {
                    foreach (var slot in displayViewModel.CanvasSlots)
                    {
                        if (slot.CanvasEntity != null)
                        {
                            var ent = slot.CanvasEntity;
                            slot.CanvasEntity = null;
                            typeof(DisplayViewModel).GetMethod("AddToGroups",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .Invoke(displayViewModel, new object[] { ent });
                        }
                    }
                    TerminalInput = string.Empty;
                    return;
                }
            }

            // === NAVIGATION ===
            if (command == "nav")
            {
                if (parts.Length > 1)
                {
                    string target = parts[1].ToLower();
                    switch (target)
                    {
                        case "entities":
                            CurrentViewModel = entitiesViewModel;
                            break;
                        case "display":
                            CurrentViewModel = displayViewModel;
                            break;
                        case "graph":
                            CurrentViewModel = graphViewModel;
                            break;
                    }
                }
            }

            TerminalInput = string.Empty;
        }

        private void CreateListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25675);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        NetworkStream stream = tcpClient.GetStream();
                        string incoming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        incoming = Encoding.ASCII.GetString(bytes, 0, i);

                        if (incoming.Equals("Need object count"))
                        {
                            Byte[] data = Encoding.ASCII.GetBytes(Entities.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else if (incoming.StartsWith("Entitet_"))
                        {
                            string[] parts = incoming.Split(':');
                            int id = int.Parse(parts[0].Split('_')[1]);
                            double val = double.Parse(parts[1]);

                            var entity = Entities.FirstOrDefault(e => e.Id == id);
                            if (entity != null)
                                entity.Value = val;

                            File.AppendAllText("log.txt", $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}, Entity_{id}, {val}\n");
                        }
                        stream.Close();
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        private void Entities_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifySimulatorToRestart();
        }

        private void NotifySimulatorToRestart()
        {
            try
            {
                using (var client = new TcpClient("localhost", 25676))
                {
                    NetworkStream stream = client.GetStream();
                    byte[] msg = Encoding.ASCII.GetBytes("Restart");
                    stream.Write(msg, 0, msg.Length);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
