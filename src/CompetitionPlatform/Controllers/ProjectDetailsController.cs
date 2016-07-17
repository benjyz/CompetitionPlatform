using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class ProjectDetailsController : Controller
    {
        private readonly IProjectCommentsRepository _projectCommentsRepository;

        public ProjectDetailsController(IProjectCommentsRepository projectCommentsRepository)
        {
            _projectCommentsRepository = projectCommentsRepository;
        }

        public IActionResult AddComment(string projectId, string comment)
        {
            var commentViewModel = new ProjectCommentsViewModel()
            {
                ProjectId = projectId,
                Comment = comment,
                User = "User1"
            };
            _projectCommentsRepository.SaveAsync(commentViewModel);

            return RedirectToAction("Index", "Home");
        }
    }
}