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
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _lookupsService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTFileService _fileService;

        public ProjectsController(IBTProjectService projectService,
                                  UserManager<BTUser> userManager,
                                  IBTRolesService rolesService,
                                  IBTLookupService lookupsService,
                                  IBTCompanyInfoService companyInfoService,
                                  IBTFileService fileService)
        {
            _projectService = projectService;
            _userManager = userManager;
            _rolesService = rolesService;
            _lookupsService = lookupsService;
            _companyInfoService = companyInfoService;
            _fileService = fileService;
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
            List<Project> projects = new();
            int companyId = User.Identity.GetCompanyId();
            if (User.IsInRole(nameof(BTRole.Admin)) || User.IsInRole(nameof(BTRole.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);
            }

            return View(projects);
        }

        //GET: Archived Projects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(companyId);
            return View(projects);
        }

        //GET: Unassigned Projects
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId();
            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);
            return View(projects);
        }

        [HttpGet]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> AssignPM(int? projectId)
        {
            if(projectId == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();
            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(projectId.Value, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRole.ProjectManager), companyId), "Id", "FullName");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {

            if (!string.IsNullOrEmpty(model.PMID))
            {
                await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);
                return RedirectToAction(nameof(AllProjects));
            }
            return RedirectToAction(nameof(AssignPM),new { projectId = model.Project.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignMembers(int? projectId)
        {
            if (projectId == null)
            {
                return NotFound();
            }

            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId();
            model.Project = await _projectService.GetProjectByIdAsync(projectId.Value, companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRole.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRole.Submitter), companyId);

            List<BTUser> teamMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members.Select(p => p.Id).ToList();
            model.UsersList = new MultiSelectList(teamMembers,"Id", "FullName", projectMembers);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if(model.SelectedUsers != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                                                               .Select(m =>m.Id).ToList();
                foreach (string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

                foreach (string member in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }

                return RedirectToAction(nameof(AssignMembers), new { id = model.Project.Id });
            }
            return RedirectToAction(nameof(AssignMembers),new { id = model.Project.Id });
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId();

            AddProjectWithPMViewModel model = new();
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRole.ProjectManager), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {

            int companyId = User.Identity.GetCompanyId();
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;

                    }
                    model.Project.CreatedDate = DateTimeOffset.UtcNow;
                    model.Project.StartDate = DateTime.SpecifyKind(model.Project.StartDate.DateTime, DateTimeKind.Utc);
                    model.Project.EndDate = DateTime.SpecifyKind(model.Project.EndDate.DateTime, DateTimeKind.Utc);

                    model.Project.CompanyId = companyId;
                    await _projectService.AddNewProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PMID))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {

                    throw;
                }
            }

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRole.ProjectManager), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(model.Project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId();

            AddProjectWithPMViewModel model = new();
            model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if(model.Project == null)
            {
                return NotFound();
            }


            BTUser projectManager = await _projectService.GetProjectManagerAsync(id.Value);
            if (projectManager != null)
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRole.ProjectManager), companyId), "Id", "FullName", projectManager.Id);
            }
            else
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRole.ProjectManager), companyId), "Id", "FullName");
            }
            model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(model);

        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {                    
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    model.Project.CreatedDate = DateTime.SpecifyKind(model.Project.CreatedDate.DateTime, DateTimeKind.Utc);
                    model.Project.StartDate = DateTime.SpecifyKind(model.Project.StartDate.DateTime, DateTimeKind.Utc);
                    model.Project.EndDate = DateTime.SpecifyKind(model.Project.EndDate.DateTime, DateTimeKind.Utc);

                    await _projectService.UpdateProjectAsync(model.Project);

                    //Add Project Manager if one was chosen
                    if (!string.IsNullOrEmpty(model.PMID))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            int companyId = User.Identity.GetCompanyId();
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRole.ProjectManager), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupsService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(model);
        }

        // GET: Projects/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id, companyId);
            await _projectService.ArchiveProjectAsync(project);
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId();
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreProject(int id)
        {
            int companyId = User.Identity.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id, companyId);
            await _projectService.RestoreProjectAsync(project);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId();
            return (await _projectService.GetAllProjectsByCompanyAsync(companyId)).Any(p => p.Id == id);
        }
    }
}
