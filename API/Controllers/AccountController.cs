using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _dataContext;
        public AccountController(DataContext dataContext)
        {
            _dataContext = dataContext;

        }

        [HttpPost("Login")]
        public async Task<ActionResult<AppUser>> Login([FromBody] LoginDtos loginDtos)
        {
            var user = await _dataContext.AppUsers.SingleOrDefaultAsync(x => x.UserName == loginDtos.UserName);
            if (user is null) return Unauthorized("User is Unauthorized");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var hashPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDtos.Password));

            for (int i = 0; i < hashPassword.Length; i++)
            {
                if (hashPassword[i] != user.PasswordHash[i]) return Unauthorized("Password is Invalid");
            }
            return user;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register([FromBody] RegiserDtos regiserDtos)
        {
            if (regiserDtos == null) throw new ArgumentNullException();

            if (await UserAlreadyExisted(regiserDtos.UserName)) throw new Exception("User Already Existed");

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

        private async Task<bool> UserAlreadyExisted(string username)
        {
            return await _dataContext.AppUsers.AnyAsync(appuser => appuser.UserName == username);
        }
    }
}