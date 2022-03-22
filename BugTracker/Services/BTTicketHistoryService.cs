﻿using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            try
            {
                if (oldTicket == null && newTicket != null)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "",
                        OldValue = "",
                        NewValue = "",
                        Modified = DateTime.UtcNow,
                        UserId = userId,
                        Description = "New Ticket Created"
                    };

                    try
                    {
                        await _context.AddAsync(history);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                else
                {

                    if (oldTicket!.Title != newTicket!.Title)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Title",
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Modified = DateTime.UtcNow,
                            UserId = userId,
                            Description = $"New ticket title: {newTicket.Title}"
                        };
                        await _context.AddAsync(history);
                    }

                    if (oldTicket!.Description != newTicket!.Description)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Description",
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Modified = DateTime.UtcNow,
                            UserId = userId,
                            Description = $"New ticket description: {newTicket.Description}"
                        };
                        await _context.AddAsync(history);
                    }

                    if (oldTicket!.TicketPriorityId != newTicket!.TicketPriorityId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Ticket Priority",
                            OldValue = oldTicket.TicketPriority.Name,
                            NewValue = newTicket.TicketPriority.Name,
                            Modified = DateTime.UtcNow,
                            UserId = userId,
                            Description = $"New ticket priority: {newTicket.TicketPriority.Name}"
                        };
                        await _context.AddAsync(history);
                    }

                    if (oldTicket!.TicketStatusId != newTicket!.TicketStatusId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Ticket Status",
                            OldValue = oldTicket.TicketStatus.Name,
                            NewValue = newTicket.TicketStatus.Name,
                            Modified = DateTime.UtcNow,
                            UserId = userId,
                            Description = $"New ticket status: {newTicket.TicketStatus.Name}"
                        };
                        await _context.AddAsync(history);
                    }

                    if (oldTicket!.TicketTypeId != newTicket!.TicketTypeId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Ticket Type",
                            OldValue = oldTicket.TicketType.Name,
                            NewValue = newTicket.TicketType.Name,
                            Modified = DateTime.UtcNow,
                            UserId = userId,
                            Description = $"New ticket type: {newTicket.TicketType.Name}"
                        };
                        await _context.AddAsync(history);
                    }

                    if (oldTicket!.DeveloperUserId != newTicket!.DeveloperUserId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Developer",
                            OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                            NewValue = newTicket.DeveloperUser.FullName,
                            Modified = DateTime.UtcNow,
                            UserId = userId,
                            Description = $"New ticket developer: {newTicket.DeveloperUser.FullName}"
                        };
                        await _context.AddAsync(history);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId);
                string description = model.ToLower().Replace("ticket", "");
                description = $"New {description} added to ticket: {ticket!.Title}";

                TicketHistory history = new()
                {
                    TicketId = ticket.Id,
                    PropertyName = model,
                    OldValue = "",
                    NewValue = "",
                    Modified = DateTime.UtcNow,
                    UserId = userId,
                    Description = description
                };

                await _context.AddAsync(history);
                await _context.SaveChangesAsync();


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            try
            {
                List<Project> projects = (await _context.Companies
                                                        .Include(c => c.Projects)
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(t => t.History)
                                                                    .ThenInclude(h => h.User)
                                                        .FirstOrDefaultAsync(c => c.Id == companyId))!.Projects.ToList();

                List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();

                List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();
                return ticketHistories;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                List<Ticket> tickets = (await _context.Projects
                                                      .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.History)
                                                            .ThenInclude(h => h.User)
                                                      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId))!.Tickets.ToList();

                List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();
                return ticketHistories;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
