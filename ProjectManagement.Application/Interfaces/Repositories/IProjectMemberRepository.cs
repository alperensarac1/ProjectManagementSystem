using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Interfaces.Repositories;


public interface IProjectMemberRepository
{
   
    Task<IReadOnlyCollection<ProjectMember>> GetByProjectIdAsync(
        int projectId,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<ProjectMember?> GetAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken = default);

 
    Task<ProjectMember?> GetByProjectAndUserForUpdateAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> IsActiveMemberAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken = default);


    Task AddAsync(
        ProjectMember projectMember,
        CancellationToken cancellationToken = default);


    void Update(
        ProjectMember projectMember);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}