﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class EntityType
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
