using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.BLL.DTO.Auth
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new ();
        public string AccessToken { get; set; } = string.Empty;

    }
}
