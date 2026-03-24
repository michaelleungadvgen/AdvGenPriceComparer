using AdvGenFlow;

namespace AdvGenPriceComparer.Application.Commands;

public record DeletePriceRecordCommand(string PriceRecordId) : IRequest<DeletePriceRecordResult>;

public record DeletePriceRecordResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static DeletePriceRecordResult SuccessResult() => new() { Success = true };
    public static DeletePriceRecordResult NotFound(string id) =>
        new() { Success = false, ErrorMessage = $"Price record not found: {id}" };
    public static DeletePriceRecordResult Failure(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
