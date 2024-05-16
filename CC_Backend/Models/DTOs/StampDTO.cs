﻿namespace CC_Backend.Models.DTOs
{
    public class StampDTO
    {
        public string Name { get; set; }
        public string? Facts { get; set; }
        public double? Rarity { get; set; }
        public byte[]? Icon { get; set; }
        public CategoryDTO? Category { get; set; }
    }
}
