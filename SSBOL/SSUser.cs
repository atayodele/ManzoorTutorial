using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SSBOL
{
    [Table("SSUser")]
    public class SSUser : IdentityUser
    {
        [Required]
        public DateTime DOB { get; set; }
        public string ProfilePicPath { get; set; }

        public IEnumerable<Story> Stories { get; set; }
    }
}
