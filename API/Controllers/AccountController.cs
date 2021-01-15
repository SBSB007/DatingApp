using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _dataContext;
        public AccountController(DataContext dataContext)
        {
            _dataContext = dataContext;

        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register([FromBody] RegiserDtos regiserDtos)
        {
            if (regiserDtos == null) throw new ArgumentNullException();

            using var hmac = new HMACSHA512();

            var User = new AppUser
            {
                UserName = regiserDtos.UserName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(regiserDtos.Password)),
                PasswordSalt = hmac.Key
            };

            _dataContext.AppUsers.Add(User);
            await _dataContext.SaveChangesAsync();
            return User;
        }
    }
}