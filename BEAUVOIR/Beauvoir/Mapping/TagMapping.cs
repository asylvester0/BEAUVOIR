using Beauvoir.DTO;
using Beauvoir.Models;

namespace Beauvoir.Mapping
{
    public class TagMapping
    {


        public static IEnumerable<TagDto> MapToBL(IEnumerable<Tag> Tags) =>
           Tags.Select(x => MapToBL(x));

        public static TagDto MapToBL(Tag Tag) =>
            new TagDto
            {
                Id = Tag.Id,
                Name = Tag.Name,

            };
        public static IEnumerable<Tag> MapToDAL(IEnumerable<TagDto> blTags) =>
           blTags.Select(x => MapToDAL(x));

        public static Tag MapToDAL(TagDto Tagdto) =>
            new Tag
            {
                Id = Tagdto.Id,
                Name = Tagdto.Name,

            };

    }
}