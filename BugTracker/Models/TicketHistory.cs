using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [DisplayName("Updated Ticket Item")]
        public string? PropertyName { get; set; }

        [DisplayName("Description of Change")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Modified")]
        public DateTimeOffset Modified { get; set; }

        [DisplayName("Previous")]
        public string? OldValue { get; set; }


        [DisplayName("Current")]
        public string? NewValue { get; set; }

        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }


        [DisplayName("Ticket")]
        public virtual Ticket? Ticket { get; set; }

        [DisplayName("Team Member")]
        public virtual BTUser? User { get; set; }
    }
}
