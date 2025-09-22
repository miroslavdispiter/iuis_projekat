using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class EntityGroup
    {
        public string GroupName { get; set; }
        public ObservableCollection<Entity> Entities { get; set; }

        public EntityGroup(string name)
        {
            GroupName = name;
            Entities = new ObservableCollection<Entity>();
        }
    }
}
