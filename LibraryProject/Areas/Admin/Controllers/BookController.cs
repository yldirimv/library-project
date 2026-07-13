using LibraryProject.Areas.Admin.Controllers;
using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Web.Areas.Admin.Controllers
{
    public class BookController : AdminBaseController
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService) => _bookService = bookService;

        public async Task<IActionResult> Index(string? search)
        {
            var books = await _bookService.GetAllAsync(search);

            //her kitabın müsait stoğunu hesapla, view'a sözlükle taşı
            var availableStock = new Dictionary<int, int>();
            foreach (var book in books)
            {
                var activeLoans = await _bookService.GetActiveLoanCountAsync(book.Id);
                availableStock[book.Id] = book.TotalStock - activeLoans;
            }

            ViewBag.AvailableStock = availableStock;
            ViewBag.Search = search;   //arama kutusu dolu kalsın 
            return View(books);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid) return View(book);
            await _bookService.CreateAsync(book);
            TempData["Success"] = "Kitap eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Book book)
        {
            if (!ModelState.IsValid) return View(book);
            await _bookService.UpdateAsync(book);
            TempData["Success"] = "Kitap güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _bookService.DeleteAsync(id);
            TempData["Success"] = "Kitap silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}