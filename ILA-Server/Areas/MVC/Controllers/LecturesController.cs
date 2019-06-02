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
using ILA_Server.Services;
using Microsoft.AspNetCore.Authorization;

namespace ILA_Server.Areas.MVC.Controllers
{
    [Area("MVC")]
    [Authorize]
    public class LecturesController : Controller
    {
        private readonly ILADbContext _context;
        private readonly IFireBaseService _fireBaseService;
        public LecturesController(ILADbContext context, IFireBaseService fireBaseService)
        {
            _context = context;
            _fireBaseService = fireBaseService;
        }

        // GET: MVC/Lectures
        public async Task<IActionResult> Index()
        {
            return View(await _context.Lectures
                .Where(x => x.Course.Owner.Id == GetUserId())
                .Include(x => x.Course)
                .OrderBy(x => x.Course)
                .ToListAsync());
        }

        // GET: MVC/Lectures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures
                .Include(x => x.Questions)
                .ThenInclude(x => x.Answers)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.Course.Owner.Id == GetUserId());

            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // GET: MVC/Lectures/Create
        public async Task<IActionResult> Create()
        {
            var courses = await _context.Courses
                .Where(x => x.Owner.Id == GetUserId())
                .Select(x => new
                {
                    ID = x.Id,
                    Name = x.Title,
                })
                .ToListAsync();

            ViewData["Courses"] = new SelectList(courses, "ID", "Name");

            return View();
        }

        // POST: MVC/Lectures/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Visible,Start,Stop,CourseId")] Lecture lecture)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == lecture.CourseId && x.Owner.Id == GetUserId());
            if (course == null)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                _context.Add(lecture);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lecture);
        }

        // GET: MVC/Lectures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
            {
                return NotFound();
            }
            return View(lecture);
        }

        // POST: MVC/Lectures/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Visible,Start,Stop,CourseId")] Lecture lecture)
        {
            if (id != lecture.Id)
            {
                return NotFound();
            }

            var lectureWithOwner = await _context.Lectures.Where(x => x.Course.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (lectureWithOwner == null)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                lectureWithOwner.Title = lecture.Title;
                lectureWithOwner.Description = lecture.Description;
                lectureWithOwner.Visible = lecture.Visible;
                try
                {
                    _context.Update(lectureWithOwner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LectureExists(lectureWithOwner.Id))
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
            return View(lecture);
        }

        // GET: MVC/Lectures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures
                .FirstOrDefaultAsync(m => m.Id == id && m.Course.Owner.Id == GetUserId());
            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // POST: MVC/Lectures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lecture = await _context.Lectures.FirstOrDefaultAsync(m => m.Id == id && m.Course.Owner.Id == GetUserId());
            if (lecture == null)
            {
                return NotFound();
            }

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> CreateQuestion(int? id)
        {
            if (id == null)
                return NotFound();

            Lecture lecture = await _context.Lectures
                .Where(x => x.Course.Owner.Id == GetUserId())
                .SingleOrDefaultAsync(x => x.Id == id);

            if (lecture == null)
                return NotFound();

            return View(new Question
            {
                Lecture = lecture,
                LectureId = lecture.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion([Bind("PointedQuestion,LectureId")] Question question)
        {
            if (question == null)
                return NotFound();
            
            Lecture lecture = await _context.Lectures
                .Include(x => x.Questions)
                .Where(x => x.Course.Owner.Id == GetUserId())
                .SingleOrDefaultAsync(x => x.Id == question.LectureId);

            if (lecture == null)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(GetUserId());
            if (user == null)
                return NotFound();

            question.User = user;

            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(question);
        }

        public async Task<IActionResult> QuestionDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Where(x => x.Id == id)
                .Where(x => x.Lecture.Course.Owner.Id == GetUserId())
                .Include(x => x.User)
                .Include(x => x.Answers)
                .ThenInclude(x => x.User)
                .SingleOrDefaultAsync();

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        public async Task<ActionResult> QuestionEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Where(x => x.Id == id)
                .Where(x => x.Lecture.Course.Owner.Id == GetUserId())
                .SingleOrDefaultAsync();
            if (question == null)
            {
                return NotFound();
            }
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuestionEdit(int id, [Bind("Id,PointedQuestion")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            var questionWithOwner = await _context.Questions.Where(x => x.Lecture.Course.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (questionWithOwner == null)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                questionWithOwner.PointedQuestion = question.PointedQuestion;

                _context.Update(questionWithOwner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(questionWithOwner);
        }


        public async Task<IActionResult> QuestionDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.Where(x => x.Lecture.Course.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        [HttpPost, ActionName("QuestionDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuestionDeleteConfirmed(int id)
        {
            var question = await _context.Questions.Where(x => x.Lecture.Course.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CreateAnswer(int? id)
        {
            if (id == null)
                return NotFound();

            Question question = await _context.Questions
                .Where(x => x.Lecture.Course.Owner.Id == GetUserId() && x.Id == id)
                .SingleOrDefaultAsync();

            if (question == null)
                return NotFound();

            return View(new Answer { Question = question, QuestionId = id.Value });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnswer([Bind("Id,Comment,QuestionId")] Answer answer)
        {
            var question = await _context.Questions
                .Include(x => x.User)
                .ThenInclude(x => x.PushTokens)
                .FirstOrDefaultAsync(x => x.Id == answer.QuestionId && x.Lecture.Course.Owner.Id == GetUserId());

            if (question == null)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(GetUserId());
            if (user == null)
                return NotFound();

            answer.User = user;

            if (ModelState.IsValid)
            {
                _context.Add(answer);
                await _context.SaveChangesAsync();
                await _fireBaseService.SendPushNotificationMessageToSingleUser(question.User, "New Answer",
                    "Someone answerd your question", new Dictionary<string, string> { { "questionId", question.Id.ToString() } });

                return RedirectToAction(nameof(Index));
            }
            return View(answer);
        }

        public async Task<IActionResult> AnswerDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answer = await _context.Answers.Where(x => x.Question.Lecture.Course.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (answer == null)
            {
                return NotFound();
            }

            return View(answer);
        }

        [HttpPost, ActionName("AnswerDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerDeleteConfirmed(int id)
        {
            var answer = await _context.Answers.Where(x => x.Question.Lecture.Course.Owner.Id == GetUserId() && x.Id == id).SingleOrDefaultAsync();
            if (answer == null)
            {
                return NotFound();
            }

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AnswerDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answer = await _context.Answers
                .Where(x => x.Id == id)
                .Where(x => x.Question.Lecture.Course.Owner.Id == GetUserId())
                .Include(x => x.User)
                .Include(x => x.Question)
                .ThenInclude(x => x.User)
                .SingleOrDefaultAsync();

            if (answer == null)
            {
                return NotFound();
            }

            return View(answer);
        }

        public async Task<ActionResult> AnswerEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var answer = await _context.Answers
                .Where(x => x.Id == id)
                .Where(x => x.Question.Lecture.Course.Owner.Id == GetUserId())
                .SingleOrDefaultAsync();
            if (answer == null)
            {
                return NotFound();
            }
            return View(answer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerEdit(int id, [Bind("Id,Comment")] Answer answer)
        {
            if (id != answer.Id)
            {
                return NotFound();
            }

            var answerWithOwner = await _context.Answers
                .Where(x => x.Question.Lecture.Course.Owner.Id == GetUserId() && x.Id == id)
                .SingleOrDefaultAsync();
            if (answerWithOwner == null)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                answerWithOwner.Comment = answer.Comment;

                _context.Update(answerWithOwner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(answerWithOwner);
        }

        private bool LectureExists(int id)
        {
            return _context.Lectures.Any(e => e.Id == id);
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
