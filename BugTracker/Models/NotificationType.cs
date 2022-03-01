﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class NotificationType
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Notificatio Type")]
        public string? Name { get; set; }
    }
}
