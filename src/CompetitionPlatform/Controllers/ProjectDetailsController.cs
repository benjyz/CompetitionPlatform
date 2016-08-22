using System;
using System.Linq;
using System.Threading.Tasks;
using CompetitionPlatform.Data.AzureRepositories.Project;
using CompetitionPlatform.Data.AzureRepositories.Result;
using CompetitionPlatform.Data.AzureRepositories.Users;
using CompetitionPlatform.Data.AzureRepositories.Vote;
using CompetitionPlatform.Helpers;
using CompetitionPlatform.Models;
using CompetitionPlatform.Models.ProjectViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CompetitionPlatform.Controllers
{
    public class ProjectDetailsController : Controller
    {
        private readonly IProjectCommentsRepository _projectCommentsRepository;
        private readonly IProjectFileRepository _projectFileRepository;
        private readonly IProjectFileInfoRepository _projectFileInfoRepository;
        private readonly IProjectVoteRepository _projectVoteRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectParticipantsRepository _projectParticipantsRepository;
        private readonly IProjectResultRepository _projectResultRepository;
        private readonly IProjectResultVoteRepository _projectResultVoteRepository;

        public ProjectDetailsController(IProjectCommentsRepository projectCommentsRepository, IProjectFileRepository projectFileRepository,
            IProjectFileInfoRepository projectFileInfoRepository, IProjectVoteRepository projectVoteRepository,
            IProjectRepository projectRepository, IProjectParticipantsRepository projectParticipantsRepository,
            IProjectResultRepository projectResultRepository, IProjectResultVoteRepository projectResultVoteRepository)
        {
            _projectCommentsRepository = projectCommentsRepository;
            _projectFileRepository = projectFileRepository;
            _projectFileInfoRepository = projectFileInfoRepository;
            _projectVoteRepository = projectVoteRepository;
            _projectRepository = projectRepository;
            _projectParticipantsRepository = projectParticipantsRepository;
            _projectResultRepository = projectResultRepository;
            _projectResultVoteRepository = projectResultVoteRepository;
        }

        public async Task<IActionResult> AddComment(ProjectCommentPartialViewModel model)
        {
            var user = GetAuthenticatedUser();
            model.UserId = user.Email;
            model.FullName = user.GetFullName();
            model.Created = DateTime.UtcNow;
            model.LastModified = model.Created;

            await _projectCommentsRepository.SaveAsync(model);
            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId });
        }

        public async Task<IActionResult> DownloadProjectFile(string id)
        {
            var fileInfo = await _projectFileInfoRepository.GetAsync(id);

            var fileStream = await _projectFileRepository.GetProjectFile(id);
            return File(fileStream, fileInfo.ContentType, fileInfo.FileName);
        }

        private async Task DoVoteFor(string id)
        {
            var user = GetAuthenticatedUser();

            var result = new ProjectVoteEntity
            {
                ProjectId = id,
                VoterUserId = user.Email,
                ForAgainst = 1,
            };

            var vote = await _projectVoteRepository.GetAsync(id, user.Email);

            if (vote == null)
            {
                await _projectVoteRepository.SaveAsync(result);

                var project = await _projectRepository.GetAsync(id);

                project.VotesFor += 1;

                await _projectRepository.UpdateAsync(project);
            }
            else
            {
                await _projectVoteRepository.UpdateAsync(result);
                if (vote.ForAgainst == -1)
                {
                    var project = await _projectRepository.GetAsync(id);

                    project.VotesFor += 1;
                    project.VotesAgainst -= 1;

                    await _projectRepository.UpdateAsync(project);
                }
            }
        }

        private async Task DoVoteAgainst(string id)
        {
            var user = GetAuthenticatedUser();

            var result = new ProjectVoteEntity
            {
                ProjectId = id,
                VoterUserId = user.Email,
                ForAgainst = -1
            };

            var vote = await _projectVoteRepository.GetAsync(id, user.Email);

            if (vote == null)
            {
                await _projectVoteRepository.SaveAsync(result);

                var project = await _projectRepository.GetAsync(id);

                project.VotesAgainst += 1;

                await _projectRepository.UpdateAsync(project);
            }
            else
            {
                await _projectVoteRepository.UpdateAsync(result);
                if (vote.ForAgainst == 1)
                {
                    var project = await _projectRepository.GetAsync(id);

                    project.VotesAgainst += 1;
                    project.VotesFor -= 1;

                    await _projectRepository.UpdateAsync(project);
                }
            }
        }

        public async Task<IActionResult> VoteFor(string projectId)
        {
            await DoVoteFor(projectId);

            return await ProjectVotingBarsPartial(projectId);
        }

        public async Task<IActionResult> VoteAgainst(string projectId)
        {
            await DoVoteAgainst(projectId);

            return await ProjectVotingBarsPartial(projectId);
        }

        private async Task<IActionResult> ProjectVotingBarsPartial(string projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            var viewModel = new ProjectVoteViewModel
            {
                VotesFor = project.VotesFor,
                VotesAgainst = project.VotesAgainst
            };

            return PartialView("~/Views/Project/VotingBarsPartial.cshtml", viewModel);
        }

        public async Task<IActionResult> AddParticipant(string id)
        {
            var user = GetAuthenticatedUser();

            var participant = await _projectParticipantsRepository.GetAsync(id, user.Email);

            if (participant == null)
            {
                var viewModel = new ProjectParticipateViewModel
                {
                    ProjectId = id,
                    UserId = user.Email,
                    FullName = user.GetFullName(),
                    Registered = DateTime.UtcNow,
                    Result = false
                };

                await _projectParticipantsRepository.SaveAsync(viewModel);
            }

            return RedirectToAction("ProjectDetails", "Project", new { id = id });
        }

        public IActionResult AddResult(string id)
        {
            var viewModel = new AddResultViewModel
            {
                ProjectId = id,
                ParticipantId = GetAuthenticatedUser().Email
            };
            return View("~/Views/Project/AddResult.cshtml", viewModel);
        }

        public async Task<IActionResult> SaveResult(AddResultViewModel model)
        {
            var participant = await _projectParticipantsRepository.GetAsync(model.ProjectId, model.ParticipantId);

            var result = await _projectResultRepository.GetAsync(model.ProjectId, model.ParticipantId);

            model.ParticipantFullName = participant.FullName;
            model.Score = 0;
            model.Submitted = DateTime.UtcNow;

            if (result == null)
            {
                await _projectResultRepository.SaveAsync(model);

                participant.Result = true;

                await _projectParticipantsRepository.UpdateAsync(participant);
            }

            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId });
        }

        public async Task<IActionResult> VoteForResult(ResultVoteViewModel model)
        {
            var voterId = GetAuthenticatedUser().Email;

            var vote = await _projectResultVoteRepository.GetAsync(model.ProjectId, model.ParticipantId, voterId);

            if (vote == null)
            {
                model.VoterUserId = voterId;

                await _projectResultVoteRepository.SaveAsync(model);
                var votes = await _projectResultVoteRepository.GetProjectResultVotesAsync(model.ProjectId);

                var totalVotes = votes.Count();
                var result = await _projectResultRepository.GetAsync(model.ProjectId, model.ParticipantId);

                result.Votes += 1;

                await _projectResultRepository.UpdateAsync(result);
                await CalculateScores(totalVotes, model.ProjectId);
            }

            return RedirectToAction("ProjectDetails", "Project", new { id = model.ProjectId });
        }

        private async Task CalculateScores(int totalVotes, string projectId)
        {
            var results = await _projectResultRepository.GetResultsAsync(projectId);

            foreach (var result in results)
            {
                result.Score = result.Votes * 100 / totalVotes;
                await _projectResultRepository.UpdateAsync(result);
            }
        }

        private CompetitionPlatformUser GetAuthenticatedUser()
        {
            return ClaimsHelper.GetUser(User.Identity);
        }
    }
}