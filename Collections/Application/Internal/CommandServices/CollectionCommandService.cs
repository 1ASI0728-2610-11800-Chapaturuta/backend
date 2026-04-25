using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Collections.Domain.Model.Commands;
using Frock_backend.Collections.Domain.Repositories;
using Frock_backend.Collections.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Collections.Application.Internal.CommandServices;

public class CollectionCommandService(
    ICollectionRepository collectionRepository,
    ICollectionItemRepository collectionItemRepository,
    IUnitOfWork unitOfWork) : ICollectionCommandService
{
    public async Task<Collection?> Handle(CreateCollectionCommand command)
    {
        var collection = new Collection(command.Name, command.FkIdUser);
        try
        {
            await collectionRepository.AddAsync(collection);
            await unitOfWork.CompleteAsync();
            return collection;
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating collection: {e.Message}");
        }
    }

    public async Task<Collection?> Handle(UpdateCollectionCommand command)
    {
        var collection = await collectionRepository.FindByIdAsync(command.Id);
        if (collection == null) return null;

        collection.Name = command.Name;
        try
        {
            collectionRepository.Update(collection);
            await unitOfWork.CompleteAsync();
            return collection;
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating collection: {e.Message}");
        }
    }

    public async Task<Collection?> Handle(DeleteCollectionCommand command)
    {
        var collection = await collectionRepository.FindByIdAsync(command.Id);
        if (collection == null) return null;

        try
        {
            collectionRepository.Remove(collection);
            await unitOfWork.CompleteAsync();
            return collection;
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting collection: {e.Message}");
        }
    }

    public async Task<CollectionItem?> Handle(AddRouteToCollectionCommand command)
    {
        var existing = await collectionItemRepository.FindByCollectionAndRouteAsync(command.CollectionId, command.RouteId);
        if (existing != null)
            throw new InvalidOperationException("Route already exists in this collection");

        var item = new CollectionItem(command.CollectionId, command.RouteId);
        try
        {
            await collectionItemRepository.AddAsync(item);
            await unitOfWork.CompleteAsync();
            return item;
        }
        catch (Exception e)
        {
            throw new Exception($"Error adding route to collection: {e.Message}");
        }
    }

    public async Task Handle(RemoveRouteFromCollectionCommand command)
    {
        var item = await collectionItemRepository.FindByCollectionAndRouteAsync(command.CollectionId, command.RouteId);
        if (item == null) throw new KeyNotFoundException("Route not found in this collection");

        try
        {
            collectionItemRepository.Remove(item);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            throw new Exception($"Error removing route from collection: {e.Message}");
        }
    }
}
