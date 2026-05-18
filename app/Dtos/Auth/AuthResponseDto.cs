using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Dtos.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}