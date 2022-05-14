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
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;

namespace Assignment1.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly UserContext _context;
        private readonly int _recordsPerPage = 21;
        private readonly int _recordsPersPage = 21;
        private readonly UserManager<Assignment1User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BooksController(UserContext context, UserManager<Assignment1User> userManager, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Books
        //public async Task<IActionResult> Index()
        //{
        //    var userContext = _context.Book.Include(b => b.Store);
        //    return View(await userContext.ToListAsync());
        //}
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Index(int id, string searchString)
        {
            Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);
            Store thisStore = await _context.Store.FirstOrDefaultAsync(s => s.UId == thisUser.Id);

            var bookContext = _context.Book.Where(b => b.StoreId == thisStore.Id).Include(b => b.Store);
            var books = from b in bookContext
                           select b;
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString)
                                       || b.Category.Contains(searchString));
            }

            int numberOfRecords = await books.CountAsync();     //Count SQL
            int numberOfPages = (int)Math.Ceiling((double)numberOfRecords / _recordsPerPage);
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.currentPage = id;
            ViewData["CurrentFilter"] = searchString;
            List<Book> booksList = await books
                .Skip(id * _recordsPerPage)  //Offset SQL
                .Take(_recordsPerPage)       //Top SQL
                .ToListAsync();
            return View(booksList);
        }
        [AllowAnonymous]//cho phep ko dang nhap
        public async Task<IActionResult> List(int id, string searchString)
        {

            var books = from b in _context.Book
                        select b;
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString)
                                       || b.Desc.Contains(searchString));
            }


            int numberOfRecords = await books.CountAsync();     //Count SQL
            int numberOfPages = (int)Math.Ceiling((double)numberOfRecords / _recordsPersPage);
            ViewBag.numberOfPages = numberOfPages;
            ViewBag.currentPage = id;
            ViewData["CurrentFilter"] = searchString;
            List<Book> booksList = await books
                .Skip(id * _recordsPerPage)  //Offset SQL
                .Take(_recordsPerPage)       //Top SQL
                .ToListAsync();
            return View(booksList);
        }
        public async Task<IActionResult> SendMail()
        {
            await _emailSender.SendEmailAsync("Kakashi21042001@gmail.com", "test send mail", "just test");
            return RedirectToAction("Index", "Carts");
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddToCart(string isbn)
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            Cart myCart = new Cart() { UId = thisUserId, BookIsbn = isbn, Quantity = 1}
            
            ;
            Cart fromDb = _context.Cart.FirstOrDefault(c => c.UId == thisUserId && c.BookIsbn == isbn);
            //if not existing (or null), add it to cart. If already added to Cart before, ignore it.
            if (fromDb != null)
            {
                fromDb.Quantity++;
                _context.Update(fromDb);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Add(myCart);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("List");
        }
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Checkout()
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            List<Cart> myDetailsInCart = await _context.Cart
                .Where(c => c.UId == thisUserId)
                .Include(c => c.Book)
                .ToListAsync();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    //Step 1: create an order
                    Order myOrder = new Order();
                    myOrder.UId = thisUserId;
                    myOrder.OrderTime = DateTime.Now;
                    myOrder.Total = myDetailsInCart.Select(c => c.Book.Price*c.Quantity)
                        .Aggregate((c1, c2) => Math.Round((c1 + c2),1));
                    _context.Add(myOrder);
                    await _context.SaveChangesAsync();

                    //Step 2: insert all order details by var "myDetailsInCart"
                    foreach (var item in myDetailsInCart)
                    {
                        OrderDetail detail = new OrderDetail()
                        {
                            OrderId = myOrder.Id,
                            BookIsbn = item.BookIsbn,
                            Quantity = item.Quantity
                        };
                        _context.Add(detail);
                    }
                    await _context.SaveChangesAsync();

                    //Step 3: empty/delete the cart we just done for thisUser
                    _context.Cart.RemoveRange(myDetailsInCart);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error occurred in Checkout" + ex);
                }
            }
            return RedirectToAction("SendMail");
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(/*[Bind("Isbn,Title,Pages,Author,Category,Price,Desc,ImgUrl")]*/ Book book, IFormFile image)
        {
            if (image != null)
            {
                //set key name
                string ImageName = book.Isbn + Path.GetExtension(image.FileName);

                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image", ImageName);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                book.ImgUrl =  ImageName;
    
                Assignment1User thisUser = await _userManager.GetUserAsync(HttpContext.User);
                Store thisStore = await _context.Store.FirstOrDefaultAsync(s => s.UId == thisUser.Id);
                book.StoreId = thisStore.Id;
            }
            else
            {
                return View(book);
            }                
                    _context.Add(book);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));                
                ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);

            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(string id, [Bind("Isbn,Title,Pages,Author,Category,Price,Desc,ImgUrl")] Book book)
        {
            if (id != book.Isbn)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var bookToUpdate = await _context.Book.FirstOrDefaultAsync(s => s.Isbn == id);
                if (bookToUpdate == null)
                {
                    return NotFound();
                }
                bookToUpdate.Title = book.Title ; 
                bookToUpdate.Pages = book.Pages ;
                bookToUpdate.Category = book.Category ;
                bookToUpdate.Author = book.Author ;
                bookToUpdate.Pages = book.Pages;
                bookToUpdate.Price = book.Price ;
                bookToUpdate.Desc = book.Desc ;
                try
                {
                    _context.Update(bookToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Isbn))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index","Stores");
            }
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [Authorize(Roles = "Seller")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Book.FindAsync(id);
            if(book == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                _context.Book.Remove(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Stores");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Unable to delete book " + id + ". Error is: " + ex.Message);
                return NotFound();

            }
        }

        private bool BookExists(string id)
        {
            return _context.Book.Any(e => e.Isbn == id);
        }
        public IActionResult BookList()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var obj = _context.Book.Where(x => x.Store.UId == userId).ToList();
;            return View(obj);
        }

    }

}
