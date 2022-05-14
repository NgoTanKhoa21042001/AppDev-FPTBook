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
using Assignment1.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Assignment1.Controllers
{
    public class CartsController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<Assignment1User> _userManager;

        public CartsController(UserContext context, UserManager<Assignment1User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Carts
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            ViewBag.CartCount = _context.Cart.Count();
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            var userContext = _context.Cart.Where(c => c.UId == thisUserId).Include(c => c.Book).Include(c => c.User);
            return View(userContext.ToList());


        }
        public async Task<IActionResult> Increase(string isbn)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            Cart fromDb = _context.Cart.FirstOrDefault(c => c.UId == thisUserId && c.BookIsbn == isbn);

            fromDb.Quantity++;
            _context.Update(fromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> removeItem(string isbn)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            Cart fromDb = _context.Cart.FirstOrDefault(c => c.UId == thisUserId && c.BookIsbn == isbn);

            fromDb.Quantity--;
            while (fromDb.Quantity != 0)
            {
                _context.Update(fromDb);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
}
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Remove(string isbn)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            Cart fromDb = _context.Cart.FirstOrDefault(c => c.UId == thisUserId && c.BookIsbn == isbn);
            _context.Remove(fromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}