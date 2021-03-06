﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace CompetitionPlatform.Data.AzureRepositories.Vote
{
    public class ProjectVoteEntity : TableEntity, IProjectVoteData
    {
        public static string GeneratePartitionKey(string projectId)
        {
            return projectId;
        }

        public static string GenerateRowKey(string userId)
        {
            return userId;
        }

        internal void Update(IProjectVoteData src)
        {
            ForAgainst = src.ForAgainst;
        }

        public static ProjectVoteEntity Create(IProjectVoteData src)
        {
            var result = new ProjectVoteEntity
            {
                RowKey = GenerateRowKey(src.VoterUserId),
                PartitionKey = GeneratePartitionKey(src.ProjectId),
                ForAgainst = src.ForAgainst
            };

            return result;
        }

        public string ProjectId { get; set; }
        public string VoterUserId { get; set; }
        public int ForAgainst { get; set; }
    }

    public class ProjectVoteRepository : IProjectVoteRepository
    {
        private readonly IAzureTableStorage<ProjectVoteEntity> _projectVoteTableStorage;

        public ProjectVoteRepository(IAzureTableStorage<ProjectVoteEntity> projectVoteTableStorage)
        {
            _projectVoteTableStorage = projectVoteTableStorage;
        }

        public async Task SaveAsync(IProjectVoteData projectVoteData)
        {
            var newEntity = ProjectVoteEntity.Create(projectVoteData);
            await _projectVoteTableStorage.InsertAsync(newEntity);
        }

        public async Task<IProjectVoteData> GetAsync(string projectId, string user)
        {
            var partitionKey = ProjectVoteEntity.GeneratePartitionKey(projectId);
            var rowKey = ProjectVoteEntity.GenerateRowKey(user);

            return await _projectVoteTableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IProjectVoteData>> GetProjectVotesAsync(string projectId)
        {
            var partitionKey = ProjectVoteEntity.GeneratePartitionKey(projectId);
            return await _projectVoteTableStorage.GetDataAsync(partitionKey);
        }

        public Task UpdateAsync(IProjectVoteData projectVoteData)
        {
            var partitionKey = ProjectVoteEntity.GeneratePartitionKey(projectVoteData.ProjectId);
            var rowKey = ProjectVoteEntity.GenerateRowKey(projectVoteData.VoterUserId);

            return _projectVoteTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(projectVoteData);
                return itm;
            });
        }
    }
}
