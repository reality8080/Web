﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Data
{
    [Table("Room")]
    public class Room
    {
        [Key]
        [MaxLength(50)]
        private int id;
        [MaxLength(100)]
        private String? nameHost;
        [MaxLength(100)]
        private String? pass;
        [MaxLength(20)]
        private String? status;// Broadcast, unicast, unique
        [MaxLength(500)]
        private String? description;
        private DateTime? time;
        [MaxLength(10)]
        private String? contactNumeber;

        public int Id { get => id; set => id = value; }
        public string? NameHost { get => nameHost; set => nameHost = value; }
        public string? Pass { get => pass; set => pass = value; }
        public string? Status { get => status; set => status = value; }
        public string? Description { get => description; set => description = value; }
        public DateTime? Time { get => time; set => time = value; }
        public string? ContactNumeber { get => contactNumeber; set => contactNumeber = value; }
    }
}
