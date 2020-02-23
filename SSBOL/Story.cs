using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSBOL
{
    [Table("Story")]
    public class Story
    {
        [Key]
        public int SSId { get; set; }
        [Required]
        public string SSTitle { get; set; }
        [Required]
        public string SSDescription { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsApproved { get; set; }

        public int Like { get; set; }
        public int Dislike { get; set; }

        [ForeignKey("SSUser")]
        public string Id { get; set; }

        public SSUser SSUser { get; set; }
    }
}
