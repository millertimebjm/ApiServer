﻿using ApiServer.Model;
using ApiServer.Repository;
using ApiServer.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace ApiServer.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly IPasswordService _passwordService;
        private readonly string _salt;

        public UsersController(IRepository repository, IPasswordService passwordService) : base()
        {
            _repository = repository;
            _passwordService = passwordService;
            _salt = _passwordService.GenerateSalt();
        }

        [HttpGet("{userId}")]
        public async Task<UserModel?> Get(Guid userId)
        {
            return new UserModel(await _repository.GetUserAsync(userId));
        }

        [HttpPost]
        public async Task<UserModel?> Post([FromBody] UsernamePasswordModel user)
        {
            if (user.Username == null || user.Password == null)
            {
                throw new ArgumentNullException();
            }

            var hashSaltPassword = ComputeHash(Encoding.UTF8.GetBytes(user.Password), Encoding.UTF8.GetBytes(_salt));
            if (await _repository.GetUserExists(user.Username))
            {
                return new UserModel(await _repository.GetUserAsync(user.Username, hashSaltPassword));
            }
            else
            {
                return new UserModel(await _repository.SetUserAsync(new User()
                {
                    Username = user.Username,
                    Password = hashSaltPassword,
                }));
            }
        }

        public static string ComputeHash(byte[] bytesToHash, byte[] salt)
        {
            var byteResult = new Rfc2898DeriveBytes(bytesToHash, salt, 10000);
            return Convert.ToBase64String(byteResult.GetBytes(24));
        }

        public static string GenerateSalt()
        {
            var bytes = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
