using AutoMapper;
using ProductInventoryAPI.Models.DTOs.Categories;
using ProductInventoryAPI.Models.Entities;

namespace ProductInventoryAPI.Core.Mapping
{
    /// <summary>
    /// AutoMapper profile for Category entity mappings
    /// </summary>
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));

            // DTO to Entity mappings
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Products, opt => opt.Ignore());
        }
    }
}