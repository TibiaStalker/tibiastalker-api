﻿using TibiaCharacterFinderAPI.Entities;

namespace TibiaCharacterFinderAPI.Models
{
    public class Model
    {
        private readonly TibiaCharacterFinderDbContext _dbContext;

        public Model(TibiaCharacterFinderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected List<World> GetAvailableWorlds()
        {
            return _dbContext.Worlds.Where(w => w.IsAvailable == true).ToList();
        }
    }
}
