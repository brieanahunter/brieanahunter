using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BHunter_Final.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using ClosedXML.Excel;

public class HomeController : Controller
{
    private readonly MovieDbContext _context;
    private readonly IWebHostEnvironment _environment;
    public HomeController(MovieDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    private async Task LoadSidebarUserAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId != null)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user != null)
            {
                ViewBag.SidebarUser = new SidebarUserViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfileImagePath = user.ProfileImagePath
                };
            }
        }
    }
    public async Task<IActionResult> Index(string search, int? genreId)
    {
        await LoadSidebarUserAsync();

        var moviesQuery = _context.Movies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            moviesQuery = moviesQuery.Where(m => m.Title.Contains(search));
        }

        if (genreId.HasValue)
        {
            moviesQuery = moviesQuery.Where(m => m.GenreId == genreId.Value);
        }

        var movies = await moviesQuery.ToListAsync();

        return View(movies);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Welcome()
    {
        return View();
    }

    //This for login
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]

    //This for login 
    public async Task<IActionResult> Login(Login model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToAction("Index");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult CreateAccount()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(CreateAccount model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string imagePath = null;

        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(stream);
            }

            imagePath = "/images/" + fileName;
        }

        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Password = model.Password,
            ProfileImagePath = imagePath
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(int id)
    {
        await LoadSidebarUserAsync();

        var movie = await _context.Movies
            .Include(m => m.Cast)
            .FirstOrDefaultAsync(m => m.MovieId == id);

        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    public async Task<IActionResult> User()
    {
        await LoadSidebarUserAsync();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = await _context.Users
            .Include(u => u.Favorites)
            .ThenInclude(f => f.Movie)
            .FirstOrDefaultAsync(u => u.UserId == userId.Value);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var favoriteMovies = user.Favorites
            .Where(f => f.Movie != null)
            .Select(f => f.Movie!)
            .ToList();

        ViewBag.FavoriteMovies = favoriteMovies;

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> AddFavoriteMovie()
    {
        await LoadSidebarUserAsync();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var favoriteMovieIds = await _context.Favorites
            .Where(f => f.UserId == userId.Value)
            .Select(f => f.MovieId)
            .ToListAsync();

        var availableMovies = await _context.Movies
            .Where(m => !favoriteMovieIds.Contains(m.MovieId))
            .OrderBy(m => m.Title)
            .ToListAsync();

        var model = new AddFavoriteMovie
        {
            AvailableMovies = availableMovies
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFavoriteMovie(AddFavoriteMovie model)
    {
        await LoadSidebarUserAsync();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        bool alreadyExists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId.Value && f.MovieId == model.MovieId);

        if (!alreadyExists)
        {
            var favorite = new Favorites
            {
                UserId = userId.Value,
                MovieId = model.MovieId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("User");
    }

    [HttpGet]
    public async Task<IActionResult> EditFavorites()
    {
        await LoadSidebarUserAsync();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId.Value)
            .Include(f => f.Movie)
            .OrderBy(f => f.Movie!.Title)
            .ToListAsync();

        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFavorite(int movieId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId.Value && f.MovieId == movieId);

        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("EditFavorites");
    }

    public async Task<IActionResult> Settings()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        var model = new EditAccount
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CurrentProfileImagePath = user.ProfileImagePath
        };

        return View(model);
    }

    public async Task<IActionResult> Group()
    {
        await LoadSidebarUserAsync();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var user = await _context.Users
       .Include(u => u.Group)    
       .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var model = new GroupPage
        {
            CurrentUser = user,
            Movies = await _context.Movies.OrderBy(m => m.Title).ToListAsync()
        };

        if (user.GroupId == null)
        {
            model.AvailableGroups = await _context.Groups
                .Include(g => g.Users)
                .Include(g => g.MovieOfTheWeek)
                .ToListAsync();

            return View(model);
        }

        var group = await _context.Groups
            .Include(g => g.MovieOfTheWeek)
            .Include(g => g.Users)
            .Include(g => g.Messages)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.GroupId == user.GroupId.Value);

        model.CurrentGroup = group;
        model.Messages = group?.Messages
            .OrderBy(m => m.SentAt)
            .ToList() ?? new List<Messages>();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinGroup(int groupId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user == null)
        {
            return RedirectToAction("Login", "Home");
        }

        user.GroupId = groupId;
        await _context.SaveChangesAsync();

        return RedirectToAction("Group");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroup(GroupPage model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var group = new Group
        {
            GroupName = model.NewGroupName,
            Description = model.NewGroupDescription,
            CreatedByUserId = user.UserId,
            WeekStartDate = DateTime.Today
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        user.GroupId = group.GroupId;
        await _context.SaveChangesAsync();

        return RedirectToAction("Group");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(GroupPage model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user == null || user.GroupId == null)
        {
            return RedirectToAction("Group");
        }

        if (!string.IsNullOrWhiteSpace(model.NewMessageText))
        {
            var message = new Messages
            {
                GroupId = user.GroupId.Value,
                UserId = user.UserId,
                MessageText = model.NewMessageText,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Group");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMovieOfTheWeek(int selectedMovieId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (user == null || user.GroupId == null)
        {
            return RedirectToAction("Group");
        }

        var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == user.GroupId.Value);
        if (group == null || group.CreatedByUserId != user.UserId)
        {
            return RedirectToAction("Group");
        }

        group.MovieOfTheWeekId = selectedMovieId;
        group.WeekStartDate = DateTime.Today;

        await _context.SaveChangesAsync();

        return RedirectToAction("Group");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUserFromGroup(int removeUserId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Home");
        }

        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        if (currentUser == null || currentUser.GroupId == null)
        {
            return RedirectToAction("Group");
        }

        var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == currentUser.GroupId.Value);
        if (group == null || group.CreatedByUserId != currentUser.UserId)
        {
            return RedirectToAction("Group");
        }

        var userToRemove = await _context.Users.FirstOrDefaultAsync(u => u.UserId == removeUserId && u.GroupId == group.GroupId);
        if (userToRemove != null && userToRemove.UserId != group.CreatedByUserId)
        {
            userToRemove.GroupId = null;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Group");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    //Leaves Group
    [HttpPost]
    public IActionResult ResetFavoriteTeams()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = _context.Users.FirstOrDefault(u => u.UserId == userId); // finds user in db based on userID in the HttpContext.Session

        if (user != null)
        {
            user.GroupId = null;
            _context.SaveChanges(); //saves changes
        }

        return RedirectToAction("Group"); //refreshes page so changes are visible to user
    }

    //Forgot Password - Verify Code - Reset Password
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPassword model)
    {
        Random random = new Random();
        int randomNumber = random.Next(0, 100000);
        string code = randomNumber.ToString("D5");


        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            ModelState.AddModelError("", "No account was found with that email.");
            return View(model);
        }

        try
        {
            if (user == null)
            {
                ModelState.AddModelError("", "No account was found with that email.");
                return View(model);
            }
            else
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(user.Email);
                mail.From = new MailAddress("sportsapplication206@gmail.com", "Silver Screen Society", Encoding.UTF8);
                mail.Subject = "Your Silver Screen Society Password Recovery Code";
                mail.Body = "<p>Your recovery code is: <b>" + code + "</b></p><p>Enter this code on the Silver Screen Society website to reset your password</p>";
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;


                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.Credentials = new NetworkCredential(
                        "sportsapplication206@gmail.com",
                        "tvbm affx ignj gbfu"
                    );
                    client.EnableSsl = true;

                    await client.SendMailAsync(mail);
                }
                HttpContext.Session.SetString("ResetCode", code);
                HttpContext.Session.SetString("ResetEmail", model.Email);

                return RedirectToAction("VerifyCode");
            }

        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Email failed to send: " + ex.Message);
            return View(model);
        }


    }

    [HttpGet]
    public IActionResult VerifyCode()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyCode(VerifyCode model)
    {
        var storedCode = HttpContext.Session.GetString("ResetCode");
        var storedEmail = HttpContext.Session.GetString("ResetEmail");

        if (string.IsNullOrWhiteSpace(model.Code))
        {
            ModelState.AddModelError("", "Please enter the code.");
            return View(model);
        }

        if (storedCode == null || storedEmail == null)
        {
            ModelState.AddModelError("", "Your reset session expired. Please try again.");
            return RedirectToAction("ForgotPassword");
        }

        if (model.Code == storedCode)
        {
            return RedirectToAction("ResetPassword");
        }

        ModelState.AddModelError("", "Invalid code.");
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        var email = HttpContext.Session.GetString("ResetEmail");

        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("ForgotPassword");
        }

        var model = new ResetPassword
        {
            Email = email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPassword model)
    {
        var sessionEmail = HttpContext.Session.GetString("ResetEmail");

        if (string.IsNullOrEmpty(sessionEmail))
        {
            return RedirectToAction("ForgotPassword");
        }

        model.Email = sessionEmail;

        if (string.IsNullOrWhiteSpace(model.NewPassword))
        {
            ModelState.AddModelError("", "Please enter a new password.");
            return View(model);
        }

        if (model.NewPassword != model.ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == sessionEmail);

        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View(model);
        }

        user.Password = model.NewPassword;

        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("ResetCode");
        HttpContext.Session.Remove("ResetEmail");

        return RedirectToAction("Login");
    }

    public IActionResult UpcomingReleases(string category)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        List<UpcomingReleases> events  = new List<UpcomingReleases>();

        string filePath = Path.Combine(_environment.ContentRootPath, "Data", "UpcomingReleases.xlsx");

        if (System.IO.File.Exists(filePath))
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    DateTime releaseDate;
                    bool validDate = DateTime.TryParse(row.Cell(5).GetValue<string>(), out releaseDate);

                    if (validDate)
                    {
                        events.Add(new UpcomingReleases
                        {
                            Title = row.Cell(2).GetValue<string>(),
                            Description = row.Cell(3).GetValue<string>(),
                            MaturityRating = row.Cell(4).GetValue<string>(),
                            Trailer = row.Cell(6).GetValue<string>(),
                            ReleaseDate = releaseDate,
                            PosterImage = row.Cell(7).GetValue<string>(),
                            GenreId = row.Cell(8).GetValue<int>()
                        });
                    }
                }
            }
        }

        var upcomingReleases = events
            .Where(e =>
            {
              

                if (DateTime.TryParse($"{e.ReleaseDate:yyyy-MM-dd}", out DateTime eventDateTime))
                {
                    return eventDateTime >= DateTime.Now;
                }

                return false;
            });

       

        var filteredEvents = upcomingReleases
            .OrderBy(e =>
            {
              

                if (DateTime.TryParse($"{e.ReleaseDate:yyyy-MM-dd}", out DateTime eventDateTime))
                {
                    return eventDateTime;
                }

                return DateTime.MaxValue;
            })
            .ToList();

        ViewBag.SelectedCategory = category;

        return View(filteredEvents);
    }
}
