using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AssignDevViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DevelopersList { get; set; }
        public string? DevId { get; set; }
    }
}
