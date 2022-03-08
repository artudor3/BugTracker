#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.ViewModels;
using BugTracker.Models.Enums;

namespace BugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _lookupsService;

        public ProjectsController(ApplicationDbContext context,
                                  IBTProjectService projectService,
                                  UserManager<BTUser> userManager,
                                  IBTRolesService rolesService, 
                                  IBTLookupService lookupsService)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _rolesService = rolesService;
            _lookupsService = lookupsService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);
            return View(projects);
            //var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.ProjectPriority);
            //return View(await applicationDbContext.ToListAsync());
        }

        //GET: Projects by User
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);
            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);
            return View(projects);
        }

        //GET: All Projects
        public async Task<IActionResult> AllProjects()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);
            return View(projects);
        }

        //GET: Archived
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(companyId);
            return View(projects);
        }
        
        //GET: Archived
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //string userId = _userManager.GetUserId(User);
            //BTUser btUser = _context.Users.Find(userId);

            int companyId = User.Identity.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [Authorize (Roles = "Admin, ProjectManager")]
        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId();

            AddProjectWithPMViewModel model = new();
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRole.ProjectManager), companyId),"Id","FullName");            
            model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //,StartDate,EndDate
        public async Task<IActionResult> Create([Bind("Name,Description,ProjectPriorityId,ImageFileName,ImageFileData,ImageContentType")] Project project)
        {
            if (ModelState.IsValid)
            {   
                //project.CreatedDate = DateTimeOffset.UtcNow;
                await _projectService.AddNewProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,CreatedDate,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageFileData,ImageContentType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _projectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
