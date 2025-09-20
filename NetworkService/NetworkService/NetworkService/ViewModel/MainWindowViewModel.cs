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
            CurrentViewModel = entitiesViewModel;

            CreateListener();
        }

        public BindableBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
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
