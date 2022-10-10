﻿namespace TibiaCharacterFinderAPI.Entities
{
    public class CharacterLogoutOrLogin
    {
        public int CharacterLogoutOrLoginId { get; set; }
        public int CharacterId { get; set; }
        public Character Character { get; set; }
        public int WorldScanId { get; set; }
        public bool IsOnline { get; set; }
        public short WorldId { get; set; }
        public WorldScan WorldScan { get; set; }
        public World World { get; set; }

    }
}
