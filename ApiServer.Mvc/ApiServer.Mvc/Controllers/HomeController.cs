﻿using ApiServer.EntityFrameworkCore;
using ApiServer.Model;
using ApiServer.Mvc.Models;
using ApiServer.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ApiServer.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository _repository;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _repository = new EntityFrameworkCoreRepository("asdf");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _repository.GetUserAsync(username, password);
            if (user == null)
            {
                TempData["loginFailed"] = "loginFailed";
                return RedirectToAction("Index");
            }
            HttpContext.Session.SetString("user", user.UserId.ToString());
            return RedirectToAction("manager");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["logout"] = true;
            return RedirectToAction("Index");
        }

        public IActionResult Manager()
        {
            var userId = HttpContext.Session.GetString("user");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }
            var model = new ManagerModel()
            {
                ItemsTask = _repository.GetItemsAsync(Guid.Parse(userId)),
                UserTask = _repository.GetUserAsync(Guid.Parse(userId)),
                PostResult = TempData["saved"] != null && ((string)TempData["saved"]) == "saved" ? true : null,
            };

            return View(model);
        }

        public IActionResult ManagerPost(IEnumerable<Item> items)
        {
            TempData["saved"] = "saved";
            return RedirectToAction("Manager");
        }

        public static void Init()
        {
            var _repository = new EntityFrameworkCoreRepository("asdf");
            var userId = Guid.NewGuid();
            _repository.SetUserAsync(new User()
            {
                UserId = userId,
                Username = "a",
                Password = "b",
                Items = new List<Item>()
                {
                    new Item()
                    {
                        UserId = userId,
                        ItemId = Guid.NewGuid(),
                        Identifier = "Item1 Identifier",
                        Value = "Item1 Value",
                    }
                }
            });
        }
    }
}