using System.Collections.Generic;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Application.Abstractions.Persistence;

public interface ILabelRepository
{
    Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken ct = default);
    Task AddAsync(Label label, CancellationToken ct = default);
    Task<IReadOnlyList<Label>> GetAllAsync(CancellationToken ct = default);
    Task<Label?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task RemoveAsync(Label label, CancellationToken ct = default);
}
