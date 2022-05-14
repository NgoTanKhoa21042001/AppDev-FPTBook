#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Identity;
using Assignment1.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace Assignment1.Controllers
{
    public class OrdersController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<Assignment1User> _userManager;
        public OrdersController(UserContext context, UserManager<Assignment1User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        [Authorize(Roles ="Customer")]
        public async Task<IActionResult> Index()
        {
            ViewBag.OrderId = _context.OrderDetail.ToList();
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            var userContext = _context.Order.Where(o => o.UId == thisUserId).Include(o => o.User);
            return View(await userContext.ToListAsync());
        }

        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> manage()
        {
            Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);
            Store thisStore = await _context.Store.FirstOrDefaultAsync(s => s.UId == thisUser.Id);

            OrderDetail thisOrderDetail = _context.OrderDetail.FirstOrDefault(o => o.Book.StoreId == thisStore.Id);
            var userContext = _context.Order/*.Where(o => o.Id == thisOrderDetail.OrderId)*/.Include(o => o.User);
            return View(await userContext.ToListAsync());
        }
        //public async Task<IActionResult> Remove(string id)
        //{
           // string thisUserId = _userManager.GetUserId(HttpContext.User);
            //var Order = _context.Order.Where(c => c.UId == thisUserId).Include(c => c.OrderDetails);
            //_context.Remove(Order);
            //await _context.SaveChangesAsync();
            //return RedirectToAction("Index", "OrdersDetails");
        //}

    }
}