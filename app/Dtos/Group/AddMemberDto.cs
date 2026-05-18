using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace app.Dtos.Group
{
    public class AddMemberDto
    {
        [Required]
        public Guid UserId { get; set; }
    }
}