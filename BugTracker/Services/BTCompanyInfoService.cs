using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context;
        public BTCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            Company? company = new();

            try
            {
                if (companyId != null)
                {
                    company = await _context.Companies.Include(c => c.Projects)
                                                       .Include(c => c.Members)
                                                       .Include(c => c.Invites)
                                                       .FirstOrDefaultAsync(c => c.Id == companyId);
                }
                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetAllMemebersAsync(int? companyId)
        {
            List<BTUser> members = new();

            try
            {
                members = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();


                //if (companyId != null)
                //{
                //    members = await _context.Companies.Include(c => c.Members)
                //                                      .Where(c => c.Id == companyId)
                //                                      .SelectMany(c => c.Members)
                //                                      .ToListAsync();
                //}
                return members;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Project>> GetAllProjectsAsync(int? companyId)
        {
            List<Project>? projects = new();

            try
            {
                projects = await _context.Projects.Where(p => p.CompanyId == companyId)
                                        .Include(p => p.Members)!
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.Comments)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.Attachments)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.History)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.Notifications)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.DeveloperUser)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.OwnerUser)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.TicketStatus)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.TicketPriority)
                                        .Include(p => p.Tickets)!
                                            .ThenInclude(t => t.TicketType)
                                        .Include(p => p.ProjectPriority)
                                        .ToListAsync();


                //if (companyId != null)
                //{
                //    projects = await _context.Companies.Include(c => c.Projects)
                //                                      .Where(c => c.Id == companyId)
                //                                      .SelectMany(c => c.Projects)
                //                                      .ToListAsync();
                //}
                return projects;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Ticket>> GetAllTicketsAsync(int? companyId)
        {
            List<Ticket> tickets = new();
            List<Project> projects = new();

            try
            {
                projects = await GetAllProjectsAsync(companyId);
                tickets = projects.SelectMany(p => p.Tickets!).ToList();

                //if (companyId != null)
                //{
                //    tickets = await _context.Companies.Where(c => c.Id == companyId)
                //                                      .Include(c => c.Projects)
                //                                      .ThenInclude(p => p.Tickets)
                //                                      .SelectMany(c => c.Projects.SelectMany(p => p.Tickets)).ToListAsync();
                //}

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Invite>> GetAllInvitesAsync(int? companyId)
        {
            List<Invite>? invites = new();

            try
            {
                //if (companyId != null)
                //{
                //    invites = await _context.Companies.Include(c => c.Invites)
                //                                      .Where(c => c.Id == companyId)
                //                                      .SelectMany(c => c.Invites)
                //                                      .ToListAsync();
                //}

                invites = (await _context.Companies
                                       .Include(c => c.Invites)!.ThenInclude(i => i.Company)
                                       .Include(c => c.Invites)!.ThenInclude(i => i.Project)
                                       .Include(c => c.Invites)!.ThenInclude(i => i.Invitee)
                                       .Include(c => c.Invites)!.ThenInclude(i => i.Invitor)
                                       .FirstOrDefaultAsync(c => c.Id != companyId))?.Invites.ToList();

                return invites!;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
