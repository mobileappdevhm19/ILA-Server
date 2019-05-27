using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using ILA_Server.Areas.Identity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ILA_Server.Data;
using ILA_Server.Models;
using Microsoft.AspNetCore.Authorization;

namespace ILA_Server.Areas.MVC.Controllers
{
    [Area("MVC")]
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ILADbContext _context;

        public CoursesController(ILADbContext context)
        {
            _context = context;
        }

        // GET: MVC/Courses
        public async Task<IActionResult> Index()
        {
            return View(await _context.Courses
                .Where(x => x.Owner.Id == GetUserId())
                .OrderBy(x => x.Title)
                .ToListAsync());
        }

        // GET: MVC/Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Where(x => x.Owner.Id == GetUserId() && x.Id == id)
                .Include(x => x.Lectures)
                .Include(x => x.Tokens)
                .Include(x => x.Members)
                .ThenInclude(y => y.Member)
                .SingleOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }
            course.Members = course.Members?.OrderBy(x => x.Member.LastName).ThenBy(x => x.Member.FirstName).ToList();
            course.Lectures = course.Lectures?.OrderBy(x => x.Start).ToList();

            return View(course);
        }

        // GET: MVC/Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MVC/Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Archived")] Course course)
        {
            if (ModelState.IsValid)
            {
                ILAUser user = await _context.Users.FindAsync(GetUserId());
                if (user == null)
                    return NotFound();

                course.Owner = user;

                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: MVC/Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.Where(x => x.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeTokenState(int id)
        {
            var token = await _context.CourseTokens
                .Where(x => x.Id == id && x.Course.Owner.Id == GetUserId())
                .Include(x => x.Course)
                .SingleOrDefaultAsync();
            if (token == null)
            {
                return NotFound();
            }

            token.Active = !token.Active;

            _context.Update(token);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { @id = token.Course.Id });
        }
        [HttpGet]
        public async Task<IActionResult> DeleteToken(int id)
        {
            var token = await _context.CourseTokens
                .Where(x => x.Id == id && x.Course.Owner.Id == GetUserId())
                .Include(x => x.Course)
                .SingleOrDefaultAsync();
            if (token == null)
            {
                return NotFound();
            }


            _context.CourseTokens.Remove(token);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { @id = token.Course.Id });
        }

        public async Task<IActionResult> CreateToken(int id)
        {
            var course = await _context.Courses
                .Where(x => x.Id == id && x.Owner.Id == GetUserId())
                .SingleOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }

            CourseToken token = CourseToken.GenerateNewToken(course);

            await _context.CourseTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { @id = course.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ShowTokenQr(int id)
        {
            var token = await _context.CourseTokens
                .Where(x => x.Id == id && x.Course.Owner.Id == GetUserId())
                .Include(x => x.Course)
                .SingleOrDefaultAsync();
            if (token == null)
            {
                return NotFound();
            }

            return View(token);
        }



        // POST: MVC/Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Archived")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            var courseWithOwner = await _context.Courses.Where(x => x.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (courseWithOwner == null)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                courseWithOwner.Title = course.Title;
                courseWithOwner.Description = course.Description;
                courseWithOwner.Archived = course.Archived;
                try
                {
                    _context.Update(courseWithOwner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: MVC/Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.Where(x => x.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: MVC/Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.Where(x => x.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (course == null)
            {
                return NotFound();
            }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        private string GetUserId()
        {
            string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UserException("Internal Error");
            return userId;
        }
    }
}
