﻿using System.Collections.Generic;

namespace TibiaCharFinder.Entities
{
    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Correlation> Correlations { get; set; }
    }
}