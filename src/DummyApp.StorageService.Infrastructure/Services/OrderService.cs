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

    public async Task<bool> AddOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null)
    {
        if (orderId == Guid.Empty || artworkId == Guid.Empty)
        {
            _logger.LogWarning("Invalid orderId or artworkId provided to AddOrderItemAsync.");
            return false;
        }

        if (quantity <= 0)
        {
            _logger.LogWarning("Invalid quantity {Quantity} provided to AddOrderItemAsync for artwork {ArtworkId}.", quantity, artworkId);
            return false;
        }

        var artwork = await _dbContext.Artworks.AsNoTracking().FirstOrDefaultAsync(a => a.Id == artworkId);
        if (artwork is null)
        {
            _logger.LogWarning("Artwork {ArtworkId} not found when creating order item.", artworkId);
            return false;
        }

        var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            order = new Order { Id = orderId };
            _dbContext.Orders.Add(order);
        }
        else if (order.Status != OrderStatus.Active && order.Status != OrderStatus.Processing && order.Status != OrderStatus.Address)
        {
            _logger.LogWarning("Cannot add item to order {OrderId} because it is not editable. Status: {Status}.", orderId, order.Status);
            return false;
        }

        if (order.Status == OrderStatus.Processing || order.Status == OrderStatus.Address)
        {
            order.Status = OrderStatus.Active;
            order.CompletedAt = null;
        }

        var existingItem = order.Items.FirstOrDefault(i => i.ArtworkId == artworkId);
        if (existingItem is not null)
        {
            _logger.LogWarning("Attempted to add duplicate artwork {ArtworkId} to order {OrderId}.", artworkId, orderId);
            return false;
        }

        var newItem = new OrderItem
        {
            OrderId = orderId,
            ArtworkId = artworkId,
            Quantity = quantity,
            PrintSizeId = printSizeId,
            PriceId = priceId,
            PriceValue = null
        };

        if (priceId.HasValue)
        {
            var selectedPrice = await _dbContext.Prices.AsNoTracking().FirstOrDefaultAsync(p => p.Id == priceId.Value);
            if (selectedPrice is not null)
            {
                newItem.PriceValue = selectedPrice.Value;
            }
        }

        order.Items.Add(newItem);

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

    public async Task<bool> UpdateOrderItemAsync(Guid orderId, Guid artworkId, int quantity, int? printSizeId = null, int? priceId = null)
    {
        if (orderId == Guid.Empty || artworkId == Guid.Empty)
        {
            _logger.LogWarning("Invalid orderId or artworkId provided to UpdateOrderItemAsync.");
            return false;
        }

        if (quantity < 0)
        {
            _logger.LogWarning("Invalid quantity {Quantity} provided to UpdateOrderItemAsync for artwork {ArtworkId}.", quantity, artworkId);
            return false;
        }

        var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found when updating order item.", orderId);
            return false;
        }

        if (order.Status != OrderStatus.Active && order.Status != OrderStatus.Processing && order.Status != OrderStatus.Address)
        {
            _logger.LogWarning("Cannot update order {OrderId} because it is not editable. Status: {Status}.", orderId, order.Status);
            return false;
        }

        if (order.Status == OrderStatus.Processing || order.Status == OrderStatus.Address)
        {
            order.Status = OrderStatus.Active;
            order.CompletedAt = null;
        }

        var existingItem = order.Items.FirstOrDefault(i => i.ArtworkId == artworkId);
        if (existingItem is null)
        {
            _logger.LogWarning("Attempted to update non-existent order item {ArtworkId} in order {OrderId}.", artworkId, orderId);
            return false;
        }

        if (quantity == 0)
        {
            order.Items.Remove(existingItem);
        }
        else
        {
            existingItem.Quantity = quantity;
            existingItem.PrintSizeId = printSizeId ?? existingItem.PrintSizeId;
            existingItem.PriceId = priceId ?? existingItem.PriceId;
            if (priceId.HasValue)
            {
                var selectedPrice = await _dbContext.Prices.AsNoTracking().FirstOrDefaultAsync(p => p.Id == priceId.Value);
                existingItem.PriceValue = selectedPrice?.Value;
            }
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
            .Include(i => i.Artwork)
            .Include(i => i.PrintSize)
            .Include(i => i.Price)
            .Select(orderItem => new OrderItemDto
            {
                OrderId = orderItem.OrderId,
                ArtworkId = orderItem.ArtworkId,
                Quantity = orderItem.Quantity,
                Name = orderItem.Artwork!.Name,
                Description = orderItem.Artwork.Description,
                ImgUrl = orderItem.Artwork.ImgUrl,
                ThumbnailUrl = orderItem.Artwork.ThumbnailUrl,
                PrintSizeId = orderItem.PrintSizeId,
                PrintSizeName = orderItem.PrintSize != null ? orderItem.PrintSize.Name : string.Empty,
                PriceId = orderItem.PriceId,
                PriceValue = orderItem.PriceValue ?? (orderItem.Price != null ? orderItem.Price.Value : (decimal?)null)
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

        var order = await _dbContext.Orders.AsNoTracking().Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return null;
        }

        var items = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(i => i.OrderId == orderId)
            .Include(i => i.Artwork)
            .Include(i => i.PrintSize)
            .Include(i => i.Price)
            .Select(orderItem => new OrderItemDto
            {
                OrderId = orderItem.OrderId,
                ArtworkId = orderItem.ArtworkId,
                Quantity = orderItem.Quantity,
                Name = orderItem.Artwork!.Name,
                Description = orderItem.Artwork.Description,
                ImgUrl = orderItem.Artwork.ImgUrl,
                ThumbnailUrl = orderItem.Artwork.ThumbnailUrl,
                PrintSizeId = orderItem.PrintSizeId,
                PrintSizeName = orderItem.PrintSize != null ? orderItem.PrintSize.Name : string.Empty,
                PriceId = orderItem.PriceId,
                PriceValue = orderItem.PriceValue ?? (orderItem.Price != null ? orderItem.Price.Value : (decimal?)null)
            })
            .ToListAsync();

        return new OrderSummaryDto
        {
            Items = items,
            Status = order.Status.ToString(),
            Address = order.Address is not null ? new OrderAddressDto
            {
                FirstName = order.Address.FirstName,
                LastName = order.Address.LastName,
                Phone = order.Address.Phone,
                Email = order.Address.Email,
                Country = order.Address.Country,
                City = order.Address.City,
                Street = order.Address.Street,
                HouseNumber = order.Address.HouseNumber,
                PostalCode = order.Address.PostalCode
            } : null
        };
    }

    public async Task<OrderAddressDto?> GetOrderAddressAsync(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderAddressAsync.");
            return null;
        }

        var order = await _dbContext.Orders.AsNoTracking().Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null || order.Address is null)
        {
            return null;
        }

        return new OrderAddressDto
        {
            FirstName = order.Address.FirstName,
            LastName = order.Address.LastName,
            Phone = order.Address.Phone,
            Email = order.Address.Email,
            Country = order.Address.Country,
            City = order.Address.City,
            Street = order.Address.Street,
            HouseNumber = order.Address.HouseNumber,
            PostalCode = order.Address.PostalCode
        };
    }

    public async Task<bool> SaveOrderAddressAsync(Guid orderId, OrderAddressDto address)
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to SaveOrderAddressAsync.");
            return false;
        }

        if (address is null)
        {
            _logger.LogWarning("Invalid address supplied to SaveOrderAddressAsync for order {OrderId}.", orderId);
            return false;
        }

        var order = await _dbContext.Orders.Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            order = new Order { Id = orderId, Status = OrderStatus.Address };
            _dbContext.Orders.Add(order);
        }
        else if (order.Status != OrderStatus.Active && order.Status != OrderStatus.Processing && order.Status != OrderStatus.Address)
        {
            _logger.LogWarning("Cannot save address for order {OrderId} because it is not editable. Status: {Status}.", orderId, order.Status);
            return false;
        }

        order.Status = OrderStatus.Address;
        order.CompletedAt = null;

        if (order.Address is null)
        {
            order.Address = new OrderAddress { OrderId = orderId };
        }

        order.Address.FirstName = address.FirstName;
        order.Address.LastName = address.LastName;
        order.Address.Phone = address.Phone;
        order.Address.Email = address.Email;
        order.Address.Country = address.Country;
        order.Address.City = address.City;
        order.Address.Street = address.Street;
        order.Address.HouseNumber = address.HouseNumber;
        order.Address.PostalCode = address.PostalCode;

        try
        {
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save order address for order {OrderId}.", orderId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving order address for order {OrderId}.", orderId);
            return false;
        }
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

        if (order.Status != OrderStatus.Active && order.Status != OrderStatus.Processing && order.Status != OrderStatus.WaitingForPayment && order.Status != OrderStatus.Address)
        {
            _logger.LogWarning("Cannot change status for order {OrderId} because it is not editable. Current status: {Status}.", orderId, order.Status);
            return false;
        }

        order.Status = status;
        if (status == OrderStatus.Processing)
        {
            order.CompletedAt = null;
        }
        else if (status == OrderStatus.Active)
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
