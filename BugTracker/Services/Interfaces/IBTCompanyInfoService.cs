﻿using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTCompanyInfoService
    {
        public Task<Company> GetCompanyInfoByIdAsync(int? companyId);

        public Task<List<BTUser>> GetAllMemebersAsync(int? companyId);

        public Task<List<Project>> GetAllProjectsAsync(int? companyId);

        public Task<List<Ticket>> GetAllTicketsAsync(int? companyId);

        public Task<List<Invite>> GetAllInvitesAsync(int? companyId);
        
    }
}
