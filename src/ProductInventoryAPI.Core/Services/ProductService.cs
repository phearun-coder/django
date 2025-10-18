using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductInventoryAPI.Core.Exceptions;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Models.DTOs.Products;
using ProductInventoryAPI.Models.DTOs.Reports;
using ProductInventoryAPI.Models.Entities;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Core.Services
{
    /// <summary>
    /// Service implementation for Product business logic
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting product with ID: {ProductId}", id);

                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID: {ProductId} not found", id);
                    throw new NotFoundException(nameof(Product), id);
                }

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResult(productDto);
            }
            catch (NotFoundException)
            {
                return ApiResponse<ProductDto>.ErrorResult($"Product with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product with ID: {ProductId}", id);
                return ApiResponse<ProductDto>.ErrorResult("An error occurred while retrieving the product.");
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all products");

                var products = await _unitOfWork.Products.GetAllAsync();
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResult(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all products");
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResult("An error occurred while retrieving products.");
            }
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            string? searchTerm = null,
            int? categoryId = null)
        {
            try
            {
                _logger.LogInformation("Getting paged products - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, Category: {CategoryId}", 
                    pageNumber, pageSize, searchTerm, categoryId);

                var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize, searchTerm, categoryId);
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

                var pagedResult = PagedResult<ProductDto>.Create(productDtos, pageNumber, pageSize, totalCount);
                return ApiResponse<PagedResult<ProductDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged products");
                return ApiResponse<PagedResult<ProductDto>>.ErrorResult("An error occurred while retrieving products.");
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetByCategoryAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Getting products by category ID: {CategoryId}", categoryId);

                // Validate category exists
                var categoryExists = await _unitOfWork.Categories.ExistsAsync(categoryId);
                if (!categoryExists)
                {
                    throw new NotFoundException(nameof(Category), categoryId);
                }

                var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResult(productDtos);
            }
            catch (NotFoundException)
            {
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResult($"Category with ID {categoryId} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting products by category ID: {CategoryId}", categoryId);
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResult("An error occurred while retrieving products by category.");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetBySkuAsync(string sku)
        {
            try
            {
                _logger.LogInformation("Getting product by SKU: {SKU}", sku);

                var product = await _unitOfWork.Products.GetBySkuAsync(sku);
                if (product == null)
                {
                    throw new NotFoundException($"Product with SKU '{sku}' was not found.");
                }

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResult(productDto);
            }
            catch (NotFoundException ex)
            {
                return ApiResponse<ProductDto>.ErrorResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product by SKU: {SKU}", sku);
                return ApiResponse<ProductDto>.ErrorResult("An error occurred while retrieving the product.");
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating new product: {ProductName}", createDto.Name);

                // Business rule validation
                await ValidateCreateProductAsync(createDto);

                await _unitOfWork.BeginTransactionAsync();

                var product = _mapper.Map<Product>(createDto);
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                var createdProduct = await _unitOfWork.Products.CreateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                // Create initial inventory record
                var inventory = _mapper.Map<Inventory>(createDto);
                inventory.ProductId = createdProduct.Id;
                inventory.LastUpdated = DateTime.UtcNow;

                await _unitOfWork.Inventory.CreateAsync(inventory);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Reload product with inventory for response
                var productWithInventory = await _unitOfWork.Products.GetByIdAsync(createdProduct.Id);
                var productDto = _mapper.Map<ProductDto>(productWithInventory);

                _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);
                return ApiResponse<ProductDto>.SuccessResult(productDto, "Product created successfully.");
            }
            catch (ConflictException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ProductDto>.ErrorResult(ex.Message);
            }
            catch (ValidationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<ProductDto>.ErrorResult("Validation failed", ex.Errors);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while creating product: {ProductName}", createDto.Name);
                return ApiResponse<ProductDto>.ErrorResult("An error occurred while creating the product.");
            }
        }

        public async Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating product with ID: {ProductId}", id);

                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Product with ID: {ProductId} not found for update", id);
                    throw new NotFoundException(nameof(Product), id);
                }

                // Business rule validation
                await ValidateUpdateProductAsync(id, updateDto);

                _mapper.Map(updateDto, existingProduct);
                existingProduct.UpdatedAt = DateTime.UtcNow;

                var updatedProduct = await _unitOfWork.Products.UpdateAsync(existingProduct);
                await _unitOfWork.SaveChangesAsync();

                var productDto = _mapper.Map<ProductDto>(updatedProduct);
                _logger.LogInformation("Product updated successfully with ID: {ProductId}", id);

                return ApiResponse<ProductDto>.SuccessResult(productDto, "Product updated successfully.");
            }
            catch (NotFoundException)
            {
                return ApiResponse<ProductDto>.ErrorResult($"Product with ID {id} was not found.");
            }
            catch (ConflictException ex)
            {
                return ApiResponse<ProductDto>.ErrorResult(ex.Message);
            }
            catch (ValidationException ex)
            {
                return ApiResponse<ProductDto>.ErrorResult("Validation failed", ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID: {ProductId}", id);
                return ApiResponse<ProductDto>.ErrorResult("An error occurred while updating the product.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID: {ProductId}", id);

                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    _logger.LogWarning("Product with ID: {ProductId} not found for deletion", id);
                    throw new NotFoundException(nameof(Product), id);
                }

                await _unitOfWork.BeginTransactionAsync();

                // Delete associated inventory first
                var inventoryExists = await _unitOfWork.Inventory.ExistsForProductAsync(id);
                if (inventoryExists)
                {
                    var inventory = await _unitOfWork.Inventory.GetByProductIdAsync(id);
                    if (inventory != null)
                    {
                        await _unitOfWork.Inventory.DeleteAsync(inventory.Id);
                    }
                }

                var result = await _unitOfWork.Products.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);
                return ApiResponse<bool>.SuccessResult(result, "Product deleted successfully.");
            }
            catch (NotFoundException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<bool>.ErrorResult($"Product with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while deleting product with ID: {ProductId}", id);
                return ApiResponse<bool>.ErrorResult("An error occurred while deleting the product.");
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(int id)
        {
            try
            {
                var exists = await _unitOfWork.Products.ExistsAsync(id);
                return ApiResponse<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if product exists with ID: {ProductId}", id);
                return ApiResponse<bool>.ErrorResult("An error occurred while checking product existence.");
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductInventoryReport>>> GetInventoryReportAsync(
            DateTime? fromDate = null, 
            DateTime? toDate = null)
        {
            try
            {
                _logger.LogInformation("Getting inventory report from {FromDate} to {ToDate}", fromDate, toDate);

                var report = await _unitOfWork.Products.GetInventoryReportAsync(fromDate, toDate);
                return ApiResponse<IEnumerable<ProductInventoryReport>>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating inventory report");
                return ApiResponse<IEnumerable<ProductInventoryReport>>.ErrorResult("An error occurred while generating the inventory report.");
            }
        }

        public async Task<ApiResponse<IEnumerable<LowStockReport>>> GetLowStockReportAsync(int threshold = 10)
        {
            try
            {
                _logger.LogInformation("Getting low stock report with threshold: {Threshold}", threshold);

                var report = await _unitOfWork.Products.GetLowStockReportAsync(threshold);
                return ApiResponse<IEnumerable<LowStockReport>>.SuccessResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating low stock report");
                return ApiResponse<IEnumerable<LowStockReport>>.ErrorResult("An error occurred while generating the low stock report.");
            }
        }

        #region Private Helper Methods

        private async Task ValidateCreateProductAsync(CreateProductDto createDto)
        {
            var errors = new List<string>();

            // Check if category exists
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(createDto.CategoryId);
            if (!categoryExists)
            {
                errors.Add($"Category with ID {createDto.CategoryId} does not exist.");
            }

            // Check for duplicate SKU
            var skuExists = await _unitOfWork.Products.SkuExistsAsync(createDto.SKU);
            if (skuExists)
            {
                errors.Add($"Product with SKU '{createDto.SKU}' already exists.");
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        private async Task ValidateUpdateProductAsync(int id, UpdateProductDto updateDto)
        {
            var errors = new List<string>();

            // Check if category exists
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(updateDto.CategoryId);
            if (!categoryExists)
            {
                errors.Add($"Category with ID {updateDto.CategoryId} does not exist.");
            }

            // Check for duplicate SKU (excluding current product)
            var skuExists = await _unitOfWork.Products.SkuExistsAsync(updateDto.SKU, id);
            if (skuExists)
            {
                errors.Add($"Another product with SKU '{updateDto.SKU}' already exists.");
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        #endregion
    }
}