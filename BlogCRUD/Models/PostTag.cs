namespace BlogCRUD.Models
{
    public class PostTag
    {
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;

        public PostTag(int postId, int tagId)
        {
            PostId = postId;
            TagId = tagId;
        }

        public PostTag() { }
    }
}
