﻿namespace CC_Backend.Models
{
    public class StampCollected
    {
        public int StampCollectedId { get; set; }
        public virtual Geodata? Geodata { get; set; }
        public virtual Stamp Stamp { get; set; }
        public virtual User User { get; set; }

       
    }
}