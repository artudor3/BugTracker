#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Identity;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTLookupService _lookupService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;

        public TicketsController(UserManager<BTUser> userManager,
                                 IBTCompanyInfoService companyInfoService,
                                 IBTTicketService ticketService,
                                 IBTProjectService projectService,
                                 IBTLookupService lookupService,
                                 IBTRolesService rolesService,
                                 IBTFileService fileService,
                                 IBTTicketHistoryService ticketHistoryService, 
                                 IBTNotificationService notificationService)
        {
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _ticketService = ticketService;
            _projectService = projectService;
            _lookupService = lookupService;
            _rolesService = rolesService;
            _fileService = fileService;
            _ticketHistoryService = ticketHistoryService;
            _notificationService = notificationService;
        }

        //GET: Tickets by User
        public async Task<IActionResult> MyTickets()
        {
            int companyId = User.Identity.GetCompanyId();
            string userId = _userManager.GetUserId(User);
            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);
            return View(tickets);
        }

        //GET: All Tickets
        public async Task<IActionResult> AllTickets()
        {
            List<Ticket> tickets = new();
            int companyId = User.Identity.GetCompanyId();

            if (User.IsInRole(nameof(BTRole.Admin)) || User.IsInRole(nameof(BTRole.ProjectManager)))
            {
                tickets = await _companyInfoService.GetAllTicketsAsync(companyId);
            }
            else
            {

                tickets = (await _ticketService.GetAllTicketsByCompanyAsync(companyId))
                                                .Where(t => t.Archived == false).ToList();
            }

            return View(tickets);
        }

        //GET: Archived Tickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);
            return View(tickets);
        }

        //GET: Unassigned Tickets
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);
            return View(tickets);
        }

        //GET: Assign Ticket Developer
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDev(int? ticketId)
        {
            if (ticketId == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();
            AssignDevViewModel model = new();

            model.Ticket = await _ticketService.GetTicketByIdAsync(ticketId.Value);
            model.DevelopersList = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(BTRole.Developer)), "Id", "FullName");

            return View(model);
        }

        //POST: Assign Ticket Developer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDev(AssignDevViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DevId))
            {   
                
                //Copy the current ticket information before it changes
                model.Ticket = await _ticketService.GetTicketByIdAsync(model.Ticket.Id);
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);

                await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DevId);                
                BTUser btUser = await _userManager.GetUserAsync(User);

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);
                //Add change to Ticket History
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);

                //Assign Developer Notification
                if (model.Ticket.DeveloperUserId != null)
                {
                    Notification devNotification = new()
                    {
                        TicketId = model.Ticket.Id,
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationType.Ticket))).Value,
                        Title = "Ticket Updated",
                        Message = $"Ticket: {model.Ticket.Title}, was updated by {btUser.FullName}",
                        Created = DateTime.UtcNow,
                        SenderId = btUser.Id,
                        RecipientId = model.Ticket.DeveloperUserId
                    };
                    await _notificationService.AddNotificationAsync(devNotification);
                    await _notificationService.SendEmailNotificationAsync(devNotification, "Ticket Updated");
                }

                return RedirectToAction(nameof(AllTickets));
            }
            return RedirectToAction(nameof(AssignDev), new { ticketId = model.Ticket.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            ModelState.Remove("UserId");
            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.UserId = _userManager.GetUserId(User);
                ticketAttachment.Created = DateTimeOffset.UtcNow;

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";

                await _ticketHistoryService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketComment), ticketAttachment.UserId);
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,CommentBody")] TicketComment comment)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                try
                {
                    comment.UserId = _userManager.GetUserId(User);
                    comment.Created = DateTime.UtcNow;

                    await _ticketService.AddTicketCommentAsync(comment);


                    //Add History
                    await _ticketHistoryService.AddHistoryAsync(comment.TicketId, nameof(TicketComment), comment.UserId);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return RedirectToAction("Details", new { id = comment.TicketId });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser btUser = await _userManager.GetUserAsync(User);

            if (User.IsInRole(nameof(BTRole.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(btUser.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,Description,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            ModelState.Remove("OwnerUserId");
            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DateTimeOffset.UtcNow;
                    ticket.OwnerUserId = _userManager.GetUserId(User);

                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(BTTicketStatus.New))).Value;
                    
                    await _ticketService.AddNewTicketAsync(ticket);

                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _ticketHistoryService.AddHistoryAsync(null!, newTicket, btUser.Id);

                    //: Ticket Create Notification
                    BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                    int companyId = User.Identity!.GetCompanyId()!;
                    Notification notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = "New Ticket",
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationType.Ticket))).Value,
                        Message = $"New Ticket: {ticket.Title}, was created by {btUser.FullName}",
                        Created = DateTime.UtcNow,
                        SenderId = btUser.Id,
                        RecipientId = projectManager?.Id
                    };
                    if (projectManager != null)
                    {
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                    }
                    else
                    {
                        //Admin notification
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, nameof(BTRole.Admin));
                    }



                }
                catch (Exception)
                {

                    throw;
                }
                return RedirectToAction(nameof(AllTickets));
            }

            if (User.IsInRole(nameof(BTRole.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(btUser.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,Description,Created,Archived,ArchivedByProject,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }
            ModelState.Remove("OwnerUserId");
            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created.DateTime, DateTimeKind.Utc);
                    ticket.Updated = DateTimeOffset.UtcNow;

                    await _ticketService.UpdateTicketAsync(ticket);

                    // Ticket Edit notification
                    BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                    int companyId = User.Identity!.GetCompanyId()!;
                    Notification notification = new()
                    {
                        TicketId = ticket.Id,
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationType.Ticket))).Value,
                        Title = "Ticket updated",
                        Message = $"Ticket: {ticket.Title}, was updated by {btUser.FullName}",
                        Created = DateTime.UtcNow,
                        SenderId = btUser.Id,
                        RecipientId = projectManager?.Id
                    };
                    // Notify PM or Admin
                    if (projectManager != null)
                    {
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "Ticket Updated");
                    }
                    else
                    {
                        //Admin notification
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, nameof(BTRole.Admin));
                    }
                    //Notify Developer
                    if (ticket.DeveloperUserId != null)
                    {
                        Notification devNotification = new()
                        {
                            TicketId = ticket.Id,
                            NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(BTNotificationType.Ticket))).Value,
                            Title = "Ticket Updated",
                            Message = $"Ticket: {ticket.Title}, was updated by {btUser.FullName}",
                            Created = DateTimeOffset.Now,
                            SenderId = btUser.Id,
                            RecipientId = ticket.DeveloperUserId
                        };
                        await _notificationService.AddNotificationAsync(devNotification);
                        await _notificationService.SendEmailNotificationAsync(devNotification, "Ticket Updated");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);
                return RedirectToAction(nameof(AllTickets));
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }


        // POST: Tickets/Archive/
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveTicket(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            await _ticketService.ArchiveTicketAsync(ticket);
            return RedirectToAction(nameof(AllTickets));
        }

        // GET: Tickets/Restore/
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Projects/Restore/
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreTicket(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            await _ticketService.RestoreTicketAsync(ticket);
            return RedirectToAction(nameof(AllTickets));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId();
            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }
    }
}
