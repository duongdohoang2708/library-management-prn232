using LibraryManagement.BLL.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.BLL.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);

    }
}
