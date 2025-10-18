using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductInventoryAPI.Core.Exceptions;
using ProductInventoryAPI.Core.Interfaces.Repositories;
using ProductInventoryAPI.Core.Interfaces.Services;
using ProductInventoryAPI.Models.DTOs.Categories;
using ProductInventoryAPI.Models.Entities;
using ProductInventoryAPI.Shared.Common;

namespace ProductInventoryAPI.Core.Services
{
    /// <summary>
    /// Service implementation for Category business logic
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<CategoryDto>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting category with ID: {CategoryId}", id);

                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID: {CategoryId} not found", id);
                    throw new NotFoundException(nameof(Category), id);
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return ApiResponse<CategoryDto>.SuccessResult(categoryDto);
            }
            catch (NotFoundException)
            {
                return ApiResponse<CategoryDto>.ErrorResult($"Category with ID {id} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting category with ID: {CategoryId}", id);
                return ApiResponse<CategoryDto>.ErrorResult("An error occurred while retrieving the category.");
            }
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all categories");

                var categories = await _unitOfWork.Categories.GetAllAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all categories");
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResult("An error occurred while retrieving categories.");
            }
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetActiveAsync()
        {
            try
            {
                _logger.LogInformation("Getting all active categories");

                var categories = await _unitOfWork.Categories.GetActiveAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active categories");
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResult("An error occurred while retrieving active categories.");
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating new category: {CategoryName}", createDto.Name);

                // Business rule validation
                await ValidateCreateCategoryAsync(createDto);

                var category = _mapper.Map<Category>(createDto);
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                var createdCategory = await _unitOfWork.Categories.CreateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                var categoryDto = _mapper.Map<CategoryDto>(createdCategory);
                _logger.LogInformation("Category created successfully with ID: {CategoryId}", createdCategory.Id);

                return ApiResponse<CategoryDto>.SuccessResult(categoryDto, "Category created successfully.");
            }
            catch (ConflictException ex)
            {
                return ApiResponse<CategoryDto>.ErrorResult(ex.Message);
            }
            catch (ValidationException ex)
            {
                return ApiResponse<CategoryDto>.ErrorResult("Validation failed", ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category: {CategoryName}", createDto.Name);
                return ApiResponse<CategoryDto>.ErrorResult("An error occurred while creating the category.");
            }
        }

        public async Task<ApiResponse<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating category with ID: {CategoryId}", id);

                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    _logger.LogWarning("Category with ID: {CategoryId} not found for update", id);
                    throw new NotFoundException(nameof(Category), id);
                }

                // Business rule validation
                await ValidateUpdateCategoryAsync(id, updateDto);

                _mapper.Map(updateDto, existingCategory);
                existingCategory.UpdatedAt = DateTime.UtcNow;

                var updatedCategory = await _unitOfWork.Categories.UpdateAsync(existingCategory);
                await _unitOfWork.SaveChangesAsync();

                var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);
                _logger.LogInformation("Category updated successfully with ID: {CategoryId}", id);

                return ApiResponse<CategoryDto>.SuccessResult(categoryDto, "Category updated successfully.");
            }
            catch (NotFoundException)
            {
                return ApiResponse<CategoryDto>.ErrorResult($"Category with ID {id} was not found.");
            }
            catch (ConflictException ex)
            {
                return ApiResponse<CategoryDto>.ErrorResult(ex.Message);
            }
            catch (ValidationException ex)
            {
                return ApiResponse<CategoryDto>.ErrorResult("Validation failed", ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category with ID: {CategoryId}", id);
                return ApiResponse<CategoryDto>.ErrorResult("An error occurred while updating the category.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting category with ID: {CategoryId}", id);

                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    _logger.LogWarning("Category with ID: {CategoryId} not found for deletion", id);
                    throw new NotFoundException(nameof(Category), id);
                }

                // Business rule: Cannot delete category that has products
                var hasProducts = await _unitOfWork.Categories.HasProductsAsync(id);
                if (hasProducts)
                {
                    throw new BusinessRuleViolationException("Cannot delete category that contains products. Please remove all products from this category first.");
                }

                var result = await _unitOfWork.Categories.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category deleted successfully with ID: {CategoryId}", id);
                return ApiResponse<bool>.SuccessResult(result, "Category deleted successfully.");
            }
            catch (NotFoundException)
            {
                return ApiResponse<bool>.ErrorResult($"Category with ID {id} was not found.");
            }
            catch (BusinessRuleViolationException ex)
            {
                return ApiResponse<bool>.ErrorResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category with ID: {CategoryId}", id);
                return ApiResponse<bool>.ErrorResult("An error occurred while deleting the category.");
            }
        }

        public async Task<ApiResponse<PagedResult<CategoryDto>>> GetPagedAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting paged categories - Page: {Page}, PageSize: {PageSize}", page, pageSize);

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var totalCount = await _unitOfWork.Categories.CountAsync();
                var categories = await _unitOfWork.Categories.GetPagedAsync(page, pageSize);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                var pagedResult = new PagedResult<CategoryDto>(categoryDtos, page, pageSize, totalCount);

                return ApiResponse<PagedResult<CategoryDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged categories");
                return ApiResponse<PagedResult<CategoryDto>>.ErrorResult("An error occurred while retrieving categories.");
            }
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> SearchAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching categories with term: {SearchTerm}", searchTerm);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllAsync();
                }

                var categories = await _unitOfWork.Categories.SearchAsync(searchTerm);
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching categories with term: {SearchTerm}", searchTerm);
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResult("An error occurred while searching categories.");
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(int id)
        {
            try
            {
                var exists = await _unitOfWork.Categories.ExistsAsync(id);
                return ApiResponse<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if category exists with ID: {CategoryId}", id);
                return ApiResponse<bool>.ErrorResult("An error occurred while checking category existence.");
            }
        }

        #region Private Helper Methods

        private async Task ValidateCreateCategoryAsync(CreateCategoryDto createDto)
        {
            var errors = new List<string>();

            // Check for duplicate name (if needed - depends on business rules)
            // This is a placeholder for business-specific validation rules

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        private async Task ValidateUpdateCategoryAsync(int id, UpdateCategoryDto updateDto)
        {
            var errors = new List<string>();

            // Check for duplicate name (if needed - depends on business rules)
            // This is a placeholder for business-specific validation rules

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        #endregion
    }
}