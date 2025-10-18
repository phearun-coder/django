using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductInventoryAPI.Core.Exceptions;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Models.DTOs.Inventory;
using ProductInventoryAPI.Models.Entities;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Core.Services
{
    /// <summary>
    /// Service implementation for Inventory business logic
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<InventoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<InventoryDto>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting inventory with ID: {InventoryId}", id);

                var inventory = await _unitOfWork.Inventory.GetByIdAsync(id);
                if (inventory == null)
                {
                    _logger.LogWarning("Inventory with ID: {InventoryId} not found", id);
                    throw new NotFoundException(nameof(Inventory), id);
                }

                var inventoryDto = _mapper.Map<InventoryDto>(inventory);
                return ApiResponse<InventoryDto>.SuccessResult(inventoryDto);
            }
            catch (NotFoundException)
            {
                return ApiResponse<InventoryDto>.ErrorResult($"Inventory with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting inventory with ID: {InventoryId}", id);
                return ApiResponse<InventoryDto>.ErrorResult("An error occurred while retrieving the inventory.");
            }
        }

        public async Task<ApiResponse<InventoryDto>> GetByProductIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation("Getting inventory for product ID: {ProductId}", productId);

                // Validate product exists
                var productExists = await _unitOfWork.Products.ExistsAsync(productId);
                if (!productExists)
                {
                    throw new NotFoundException(nameof(Product), productId);
                }

                var inventory = await _unitOfWork.Inventory.GetByProductIdAsync(productId);
                if (inventory == null)
                {
                    throw new NotFoundException($"No inventory found for product with ID {productId}");
                }

                var inventoryDto = _mapper.Map<InventoryDto>(inventory);
                return ApiResponse<InventoryDto>.SuccessResult(inventoryDto);
            }
            catch (NotFoundException ex)
            {
                return ApiResponse<InventoryDto>.ErrorResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting inventory for product ID: {ProductId}", productId);
                return ApiResponse<InventoryDto>.ErrorResult("An error occurred while retrieving the inventory.");
            }
        }

        public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all inventory records");

                var inventories = await _unitOfWork.Inventory.GetAllAsync();
                var inventoryDtos = _mapper.Map<IEnumerable<InventoryDto>>(inventories);

                return ApiResponse<IEnumerable<InventoryDto>>.SuccessResult(inventoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all inventory records");
                return ApiResponse<IEnumerable<InventoryDto>>.ErrorResult("An error occurred while retrieving inventory records.");
            }
        }

        public async Task<ApiResponse<PagedResult<InventoryDto>>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Getting paged inventory - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                var (inventories, totalCount) = await _unitOfWork.Inventory.GetPagedAsync(pageNumber, pageSize);
                var inventoryDtos = _mapper.Map<IEnumerable<InventoryDto>>(inventories);

                var pagedResult = PagedResult<InventoryDto>.Create(inventoryDtos, pageNumber, pageSize, totalCount);
                return ApiResponse<PagedResult<InventoryDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged inventory");
                return ApiResponse<PagedResult<InventoryDto>>.ErrorResult("An error occurred while retrieving inventory records.");
            }
        }

        public async Task<ApiResponse<IEnumerable<InventoryDto>>> GetLowStockItemsAsync(int threshold = 10)
        {
            try
            {
                _logger.LogInformation("Getting low stock items with threshold: {Threshold}", threshold);

                var lowStockItems = await _unitOfWork.Inventory.GetLowStockItemsAsync(threshold);
                var inventoryDtos = _mapper.Map<IEnumerable<InventoryDto>>(lowStockItems);

                return ApiResponse<IEnumerable<InventoryDto>>.SuccessResult(inventoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting low stock items");
                return ApiResponse<IEnumerable<InventoryDto>>.ErrorResult("An error occurred while retrieving low stock items.");
            }
        }

        public async Task<ApiResponse<InventoryDto>> CreateAsync(CreateInventoryDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating inventory for product ID: {ProductId}", createDto.ProductId);

                // Business rule validation
                await ValidateCreateInventoryAsync(createDto);

                var inventory = _mapper.Map<Inventory>(createDto);
                inventory.LastUpdated = DateTime.UtcNow;

                var createdInventory = await _unitOfWork.Inventory.CreateAsync(inventory);
                await _unitOfWork.SaveChangesAsync();

                var inventoryDto = _mapper.Map<InventoryDto>(createdInventory);
                _logger.LogInformation("Inventory created successfully with ID: {InventoryId}", createdInventory.Id);

                return ApiResponse<InventoryDto>.SuccessResult(inventoryDto, "Inventory created successfully.");
            }
            catch (ConflictException ex)
            {
                return ApiResponse<InventoryDto>.ErrorResult(ex.Message);
            }
            catch (ValidationException ex)
            {
                return ApiResponse<InventoryDto>.ErrorResult("Validation failed", ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating inventory for product ID: {ProductId}", createDto.ProductId);
                return ApiResponse<InventoryDto>.ErrorResult("An error occurred while creating the inventory.");
            }
        }

        public async Task<ApiResponse<InventoryDto>> UpdateAsync(int id, UpdateInventoryDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating inventory with ID: {InventoryId}", id);

                var existingInventory = await _unitOfWork.Inventory.GetByIdAsync(id);
                if (existingInventory == null)
                {
                    _logger.LogWarning("Inventory with ID: {InventoryId} not found for update", id);
                    throw new NotFoundException(nameof(Inventory), id);
                }

                _mapper.Map(updateDto, existingInventory);
                existingInventory.LastUpdated = DateTime.UtcNow;

                var updatedInventory = await _unitOfWork.Inventory.UpdateAsync(existingInventory);
                await _unitOfWork.SaveChangesAsync();

                var inventoryDto = _mapper.Map<InventoryDto>(updatedInventory);
                _logger.LogInformation("Inventory updated successfully with ID: {InventoryId}", id);

                return ApiResponse<InventoryDto>.SuccessResult(inventoryDto, "Inventory updated successfully.");
            }
            catch (NotFoundException)
            {
                return ApiResponse<InventoryDto>.ErrorResult($"Inventory with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating inventory with ID: {InventoryId}", id);
                return ApiResponse<InventoryDto>.ErrorResult("An error occurred while updating the inventory.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting inventory with ID: {InventoryId}", id);

                var existingInventory = await _unitOfWork.Inventory.GetByIdAsync(id);
                if (existingInventory == null)
                {
                    _logger.LogWarning("Inventory with ID: {InventoryId} not found for deletion", id);
                    throw new NotFoundException(nameof(Inventory), id);
                }

                var result = await _unitOfWork.Inventory.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Inventory deleted successfully with ID: {InventoryId}", id);
                return ApiResponse<bool>.SuccessResult(result, "Inventory deleted successfully.");
            }
            catch (NotFoundException)
            {
                return ApiResponse<bool>.ErrorResult($"Inventory with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting inventory with ID: {InventoryId}", id);
                return ApiResponse<bool>.ErrorResult("An error occurred while deleting the inventory.");
            }
        }

        public async Task<ApiResponse<InventoryDto>> UpdateStockAsync(int productId, UpdateStockDto updateStockDto)
        {
            try
            {
                _logger.LogInformation("Updating stock for product ID: {ProductId}", productId);

                var inventory = await _unitOfWork.Inventory.GetByProductIdAsync(productId);
                if (inventory == null)
                {
                    throw new NotFoundException($"No inventory found for product with ID {productId}");
                }

                // Apply stock change
                var newQuantity = inventory.QuantityOnHand + updateStockDto.QuantityChange;
                if (newQuantity < 0)
                {
                    throw new BusinessRuleViolationException("Stock quantity cannot be negative.");
                }

                inventory.QuantityOnHand = newQuantity;
                inventory.LastUpdated = DateTime.UtcNow;

                var updatedInventory = await _unitOfWork.Inventory.UpdateAsync(inventory);
                await _unitOfWork.SaveChangesAsync();

                var inventoryDto = _mapper.Map<InventoryDto>(updatedInventory);
                _logger.LogInformation("Stock updated successfully for product ID: {ProductId}. New quantity: {Quantity}", 
                    productId, newQuantity);

                return ApiResponse<InventoryDto>.SuccessResult(inventoryDto, "Stock updated successfully.");
            }
            catch (NotFoundException ex)
            {
                return ApiResponse<InventoryDto>.ErrorResult(ex.Message);
            }
            catch (BusinessRuleViolationException ex)
            {
                return ApiResponse<InventoryDto>.ErrorResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating stock for product ID: {ProductId}", productId);
                return ApiResponse<InventoryDto>.ErrorResult("An error occurred while updating the stock.");
            }
        }

        public async Task<ApiResponse<InventoryDto>> SetStockAsync(int productId, SetStockDto setStockDto)
        {
            try
            {
                _logger.LogInformation("Setting stock for product ID: {ProductId} to {NewQuantity}", 
                    productId, setStockDto.NewQuantity);

                var inventory = await _unitOfWork.Inventory.GetByProductIdAsync(productId);
                if (inventory == null)
                {
                    throw new NotFoundException($"No inventory found for product with ID {productId}");
                }

                inventory.QuantityOnHand = setStockDto.NewQuantity;
                inventory.LastUpdated = DateTime.UtcNow;

                var updatedInventory = await _unitOfWork.Inventory.UpdateAsync(inventory);
                await _unitOfWork.SaveChangesAsync();

                var inventoryDto = _mapper.Map<InventoryDto>(updatedInventory);
                _logger.LogInformation("Stock set successfully for product ID: {ProductId}. New quantity: {Quantity}", 
                    productId, setStockDto.NewQuantity);

                return ApiResponse<InventoryDto>.SuccessResult(inventoryDto, "Stock set successfully.");
            }
            catch (NotFoundException ex)
            {
                return ApiResponse<InventoryDto>.ErrorResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while setting stock for product ID: {ProductId}", productId);
                return ApiResponse<InventoryDto>.ErrorResult("An error occurred while setting the stock.");
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(int id)
        {
            try
            {
                var exists = await _unitOfWork.Inventory.ExistsAsync(id);
                return ApiResponse<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if inventory exists with ID: {InventoryId}", id);
                return ApiResponse<bool>.ErrorResult("An error occurred while checking inventory existence.");
            }
        }

        public async Task<ApiResponse<bool>> ExistsForProductAsync(int productId)
        {
            try
            {
                var exists = await _unitOfWork.Inventory.ExistsForProductAsync(productId);
                return ApiResponse<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if inventory exists for product ID: {ProductId}", productId);
                return ApiResponse<bool>.ErrorResult("An error occurred while checking inventory existence.");
            }
        }

        #region Private Helper Methods

        private async Task ValidateCreateInventoryAsync(CreateInventoryDto createDto)
        {
            var errors = new List<string>();

            // Check if product exists
            var productExists = await _unitOfWork.Products.ExistsAsync(createDto.ProductId);
            if (!productExists)
            {
                errors.Add($"Product with ID {createDto.ProductId} does not exist.");
            }

            // Check if inventory already exists for this product
            var inventoryExists = await _unitOfWork.Inventory.ExistsForProductAsync(createDto.ProductId);
            if (inventoryExists)
            {
                errors.Add($"Inventory already exists for product with ID {createDto.ProductId}.");
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        #endregion
    }
}