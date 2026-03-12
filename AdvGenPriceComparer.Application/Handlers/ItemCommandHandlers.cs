using AdvGenPriceComparer.Application.Commands;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvGenPriceComparer.Application.Handlers;

/// <summary>
/// Handler for CreateItemCommand
/// </summary>
public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, CreateItemResult>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<CreateItemCommandHandler> _logger;

    public CreateItemCommandHandler(IItemRepository itemRepository, ILogger<CreateItemCommandHandler> logger)
    {
        _itemRepository = itemRepository;
        _logger = logger;
    }

    public Task<CreateItemResult> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Task.FromResult(CreateItemResult.Failure("Item name is required."));
            }

            var item = new Item
            {
                Name = request.Name.Trim(),
                Brand = request.Brand?.Trim(),
                Category = request.Category?.Trim(),
                Barcode = request.Barcode?.Trim(),
                PackageSize = request.PackageSize?.Trim(),
                Unit = request.Unit?.Trim(),
                Description = request.Description?.Trim(),
                DateAdded = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _itemRepository.Add(item);

            _logger.LogInformation("Created item: {ItemName} with ID: {ItemId}", item.Name, item.Id);

            return Task.FromResult(CreateItemResult.SuccessResult(item.Id, item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item: {ItemName}", request.Name);
            return Task.FromResult(CreateItemResult.Failure($"Failed to create item: {ex.Message}"));
        }
    }
}

/// <summary>
/// Handler for UpdateItemCommand
/// </summary>
public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, UpdateItemResult>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<UpdateItemCommandHandler> _logger;

    public UpdateItemCommandHandler(IItemRepository itemRepository, ILogger<UpdateItemCommandHandler> logger)
    {
        _itemRepository = itemRepository;
        _logger = logger;
    }

    public Task<UpdateItemResult> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var item = _itemRepository.GetById(request.ItemId);
            if (item == null)
            {
                return Task.FromResult(UpdateItemResult.NotFound(request.ItemId));
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(request.Name))
                item.Name = request.Name.Trim();
            if (request.Brand != null)
                item.Brand = request.Brand.Trim();
            if (request.Category != null)
                item.Category = request.Category.Trim();
            if (request.Barcode != null)
                item.Barcode = request.Barcode.Trim();
            if (request.PackageSize != null)
                item.PackageSize = request.PackageSize.Trim();
            if (request.Unit != null)
                item.Unit = request.Unit.Trim();
            if (request.Description != null)
                item.Description = request.Description.Trim();

            item.LastUpdated = DateTime.UtcNow;

            _itemRepository.Update(item);

            _logger.LogInformation("Updated item: {ItemName} with ID: {ItemId}", item.Name, item.Id);

            return Task.FromResult(UpdateItemResult.SuccessResult(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item: {ItemId}", request.ItemId);
            return Task.FromResult(UpdateItemResult.Failure($"Failed to update item: {ex.Message}"));
        }
    }
}

/// <summary>
/// Handler for DeleteItemCommand
/// </summary>
public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, DeleteItemResult>
{
    private readonly IItemRepository _itemRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILogger<DeleteItemCommandHandler> _logger;

    public DeleteItemCommandHandler(
        IItemRepository itemRepository,
        IPriceRecordRepository priceRecordRepository,
        ILogger<DeleteItemCommandHandler> logger)
    {
        _itemRepository = itemRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    public Task<DeleteItemResult> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var item = _itemRepository.GetById(request.ItemId);
            if (item == null)
            {
                return Task.FromResult(DeleteItemResult.NotFound(request.ItemId));
            }

            // Note: Price records are typically kept for historical purposes
            // but could be deleted here if needed

            _itemRepository.Delete(request.ItemId);

            _logger.LogInformation("Deleted item with ID: {ItemId}", request.ItemId);

            return Task.FromResult(DeleteItemResult.SuccessResult());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item: {ItemId}", request.ItemId);
            return Task.FromResult(DeleteItemResult.Failure($"Failed to delete item: {ex.Message}"));
        }
    }
}
