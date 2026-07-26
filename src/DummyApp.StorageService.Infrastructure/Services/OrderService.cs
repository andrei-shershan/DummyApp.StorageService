using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly StorageDbContext _dbContext;
    private readonly ILogger<OrderService> _logger;

    public OrderService(StorageDbContext dbContext, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity)
    {
        if (orderId == Guid.Empty || artworkId == Guid.Empty)
        {
            _logger.LogWarning("Invalid orderId or artworkId provided to AddOrderItemAsync.");
            return false;
        }

        var artwork = await _dbContext.Artworks.AsNoTracking().FirstOrDefaultAsync(a => a.Id == artworkId);
        if (artwork is null)
        {
            _logger.LogWarning("Artwork {ArtworkId} not found when updating order item.", artworkId);
            return false;
        }

        var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            if (quantity <= 0)
            {
                _logger.LogWarning("Cannot decrement or remove item from non-existent order {OrderId}.", orderId);
                return false;
            }

            order = new Order { Id = orderId };
            _dbContext.Orders.Add(order);
        }
        else if (order.Status != OrderStatus.Active)
        {
            _logger.LogWarning("Cannot update order {OrderId} because it is not active. Status: {Status}.", orderId, order.Status);
            return false;
        }

        var existingItem = order.Items.FirstOrDefault(i => i.ArtworkId == artworkId);
        if (existingItem is null)
        {
            if (quantity <= 0)
            {
                _logger.LogWarning("Attempted to remove non-existent order item {ArtworkId} from order {OrderId}.", artworkId, orderId);
                return false;
            }

            order.Items.Add(new OrderItem
            {
                OrderId = orderId,
                ArtworkId = artworkId,
                Quantity = quantity
            });
        }
        else if (quantity == 0)
        {
            order.Items.Remove(existingItem);
        }
        else if (quantity < 0)
        {
            existingItem.Quantity += quantity;
            if (existingItem.Quantity <= 0)
            {
                order.Items.Remove(existingItem);
            }
        }
        else
        {
            existingItem.Quantity += quantity;
        }

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save order item for order {OrderId}.", orderId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving order item for order {OrderId}.", orderId);
            return false;
        }
    }

    public async Task<IEnumerable<OrderItemDto>> GetOrderItemsAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderItemsAsync.");
            return Array.Empty<OrderItemDto>();
        }

        var items = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(i => i.OrderId == orderId)
            .Join(_dbContext.Artworks,
                orderItem => orderItem.ArtworkId,
                artwork => artwork.Id,
                (orderItem, artwork) => new OrderItemDto
                {
                    OrderId = orderItem.OrderId,
                    ArtworkId = orderItem.ArtworkId,
                    Quantity = orderItem.Quantity,
                    Name = artwork.Name,
                    Description = artwork.Description,
                    ImgUrl = artwork.ImgUrl,
                    ThumbnailUrl = artwork.ThumbnailUrl
                })
            .ToListAsync();

        return items;
    }

    public async Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderSummaryAsync.");
            return null;
        }

        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return null;
        }

        var items = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(i => i.OrderId == orderId)
            .Join(_dbContext.Artworks,
                orderItem => orderItem.ArtworkId,
                artwork => artwork.Id,
                (orderItem, artwork) => new OrderItemDto
                {
                    OrderId = orderItem.OrderId,
                    ArtworkId = orderItem.ArtworkId,
                    Quantity = orderItem.Quantity,
                    Name = artwork.Name,
                    Description = artwork.Description,
                    ImgUrl = artwork.ImgUrl,
                    ThumbnailUrl = artwork.ThumbnailUrl
                })
            .ToListAsync();

        return new OrderSummaryDto
        {
            Items = items,
            Status = order.Status.ToString()
        };
    }

    public async Task<OrderStatus?> GetOrderStatusAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderStatusAsync.");
            return null;
        }

        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId);
        return order?.Status;
    }

    public async Task<bool> SetOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to SetOrderStatusAsync.");
            return false;
        }

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found when setting status.", orderId);
            return false;
        }

        if (order.Status != OrderStatus.Active)
        {
            _logger.LogWarning("Cannot change status for order {OrderId} because it is not active. Current status: {Status}.", orderId, order.Status);
            return false;
        }

        order.Status = status;
        if (status == OrderStatus.Processing)
        {
            order.CompletedAt = null;
        }
        else if (status == OrderStatus.Completed)
        {
            order.CompletedAt = DateTime.UtcNow;
        }

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update status for order {OrderId}.", orderId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating status for order {OrderId}.", orderId);
            return false;
        }
    }
}
