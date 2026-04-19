using CustomLogin.Domain;
using CustomLogin.Domain.ProviderManagement;

namespace CustomLogin.Application.ProviderManagement.Commands;

public sealed class DeleteProviderConfigCommandHandler
{
    private readonly IProviderConfigRepository _repository;

    public DeleteProviderConfigCommandHandler(IProviderConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteProviderConfigCommand command, CancellationToken ct = default)
    {
        var id = ProviderId.From(command.Id);
        var config = await _repository.GetByIdAsync(id, ct);

        if (config is null)
            return Result.Failure("Provider config not found.");

        await _repository.DeleteAsync(id, ct);

        return Result.Success();
    }
}
