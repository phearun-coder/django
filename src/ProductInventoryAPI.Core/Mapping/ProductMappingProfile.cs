using AutoMapper;
using ProductInventoryAPI.Models.DTOs.Products;
using ProductInventoryAPI.Models.Entities;

namespace ProductInventoryAPI.Core.Mapping
{
    /// <summary>
    /// AutoMapper profile for Product entity mappings
    /// </summary>
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.Quantity : (int?)null))
                .ForMember(dest => dest.ReorderPoint, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.ReorderPoint : (int?)null))
                .ForMember(dest => dest.LastStockUpdate, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.LastUpdated : (DateTime?)null));

            // DTO to Entity mappings
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore());

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore());

            // Create inventory mapping for new products
            CreateMap<CreateProductDto, Inventory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.InitialQuantity))
                .ForMember(dest => dest.ReorderPoint, opt => opt.MapFrom(src => src.ReorderPoint))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }
    }
}