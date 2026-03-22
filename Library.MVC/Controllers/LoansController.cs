
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CommunityLibraryDesk.Data;
using Library.Domain.Entities;

namespace CommunityLibraryDesk.Controllers
{
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Loan loan)
        {
            var activeLoan = _context.Loans
                .Any(l => l.BookId == loan.BookId && l.ReturnedDate == null);

            if (activeLoan)
            {
                ModelState.AddModelError("", "This book is already on loan.");
                PopulateDropdowns();
                return View(loan);
            }

            var book = await _context.Books.FindAsync(loan.BookId);
            if (book != null)
            {
                book.IsAvailable = false;
            }

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null) return NotFound();

            PopulateDropdowns(loan);
            return View(loan);
        }

        // POST: Loans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Loan loan)
        {
            if (id != loan.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingLoan = await _context.Loans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(l => l.Id == id);

                    var book = await _context.Books.FindAsync(loan.BookId);

                    // If returned → make book available
                    if (existingLoan != null && existingLoan.ReturnedDate == null && loan.ReturnedDate != null)
                    {
                        if (book != null)
                        {
                            book.IsAvailable = true;
                        }
                    }

                    _context.Update(loan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanExists(loan.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(loan);
            return View(loan);
        }

        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan != null)
            {
                var book = await _context.Books.FindAsync(loan.BookId);

                if (loan.ReturnedDate == null && book != null)
                {
                    book.IsAvailable = true;
                }

                _context.Loans.Remove(loan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.Id == id);
        }

        // FIXED DROPDOWN METHOD (NO MORE NULL ERROR)
        private void PopulateDropdowns(Loan loan = null)
        {
            var booksQuery = _context.Books.AsQueryable();

            if (loan != null)
            {
                booksQuery = booksQuery.Where(b => b.IsAvailable || b.Id == loan.BookId);
            }
            else
            {
                booksQuery = booksQuery.Where(b => b.IsAvailable);
            }

            ViewData["BookId"] = new SelectList(
                booksQuery,
                "Id",
                "Title",
                loan?.BookId
            );

            ViewData["MemberId"] = new SelectList(
                _context.Members,
                "Id",
                "FullName",
                loan?.MemberId
            );
        }
    }
}

