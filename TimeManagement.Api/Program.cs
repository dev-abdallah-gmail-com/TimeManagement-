using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TimeManagement.Api.Data;
using TimeManagement.Api.Models;
using TimeManagement.Api.Services;
using TaskStatus = TimeManagement.Api.Models.TaskStatus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database0
builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
 options.Password.RequireDigit = true;
 options.Password.RequireLowercase = true;
 options.Password.RequireUppercase = true;
 options.Password.RequireNonAlphanumeric = false;
 options.Password.RequiredLength =6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateLifetime = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = jwtIssuer,
 ValidAudience = jwtAudience,
 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
 };
});

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITagService, TagService>();

// Configure CORS
builder.Services.AddCors(options =>
{
 options.AddPolicy("AllowAll", policy =>
 {
 policy.AllowAnyOrigin()
 .AllowAnyMethod()
 .AllowAnyHeader();
 });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
 }

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add a root endpoint to avoid404 on '/'
app.MapGet("/", () => Results.Ok("TimeManagement API is running. Visit /swagger for API docs."));

// Initialize database and seed roles, users, tags and sample tasks
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
 var context = services.GetRequiredService<ApplicationDbContext>();
 var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
 var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

 await context.Database.EnsureCreatedAsync();

 // Seed roles
 string[] roles = { "Admin", "User" };
 foreach (var role in roles)
 {
 if (!await roleManager.RoleExistsAsync(role))
 {
 await roleManager.CreateAsync(new IdentityRole(role));
 }
 }

 // Seed test users
 const string defaultPassword = "Password1";
 var seedUsers = new[]
 {
 (Email: "admin1@local.test", First: "Admin", Last: "One", Role: "Admin"),
 (Email: "admin2@local.test", First: "Admin", Last: "Two", Role: "Admin"),
 (Email: "user1@local.test", First: "User", Last: "One", Role: "User"),
 (Email: "user2@local.test", First: "User", Last: "Two", Role: "User")
 };

 foreach (var su in seedUsers)
 {
 var existing = await userManager.FindByEmailAsync(su.Email);
 if (existing == null)
 {
 var user = new ApplicationUser
 {
 UserName = su.Email,
 Email = su.Email,
 FirstName = su.First,
 LastName = su.Last,
 EmailConfirmed = true
 };

 var result = await userManager.CreateAsync(user, defaultPassword);
 if (result.Succeeded)
 {
 await userManager.AddToRoleAsync(user, su.Role);
 }
 else
 {
 var logger = services.GetRequiredService<ILogger<Program>>();
 logger.LogWarning("Failed to create seed user {Email}: {Errors}", su.Email, string.Join(',', result.Errors.Select(e => e.Description)));
 }
 }
 }

 // Seed tags
 if (!context.Tags.Any())
 {
 var tags = new List<Tag>
 {
 new Tag { Name = "Bug", Color = "#e74c3c" },
 new Tag { Name = "Feature", Color = "#3498db" },
 new Tag { Name = "Urgent", Color = "#f39c12" }
 };
 context.Tags.AddRange(tags);
 await context.SaveChangesAsync();
 }

 // Seed sample tasks
 if (!context.Tasks.Any())
 {
 var u1 = await userManager.FindByEmailAsync("user1@local.test");
 var u2 = await userManager.FindByEmailAsync("user2@local.test");

 var tagBug = await context.Tags.FirstOrDefaultAsync(t => t.Name == "Bug");
 var tagFeature = await context.Tags.FirstOrDefaultAsync(t => t.Name == "Feature");
 var tagUrgent = await context.Tags.FirstOrDefaultAsync(t => t.Name == "Urgent");

 if (u1 != null && u2 != null)
 {
 var task1 = new UserTask
 {
 Title = "Fix login issue",
 Description = "Users cannot login with special characters",
 ScheduledStartDate = DateTime.UtcNow.AddDays(1),
 ScheduledEndDate = DateTime.UtcNow.AddDays(1).AddHours(8),
 CreatedBy = u1.Id,
 CreatedAt = DateTime.UtcNow,
 Status = TaskStatus.Assigned,
 AssignedTo = u2.Id,
 Tags = new List<Tag>()
 };
 if (tagBug != null) task1.Tags.Add(tagBug);
 if (tagFeature != null) task1.Tags.Add(tagFeature);

 context.Tasks.Add(task1);

 var task2 = new UserTask
 {
 Title = "Research new technologies",
 Description = "Investigate React vs Vue",
 CreatedBy = u2.Id,
 CreatedAt = DateTime.UtcNow,
 Status = TaskStatus.Pending,
 Tags = new List<Tag>()
 };
 if (tagUrgent != null) task2.Tags.Add(tagUrgent);

 context.Tasks.Add(task2);

 await context.SaveChangesAsync();

 // Add history
 context.TaskHistories.Add(new TaskHistory
 {
 TaskId = task1.Id,
 Action = "Created",
 PerformedBy = u1.Id,
 Details = $"Task created: {task1.Title}",
 OldStatus = null,
 NewStatus = task1.Status,
 PerformedAt = DateTime.UtcNow
 });

 context.TaskHistories.Add(new TaskHistory
 {
 TaskId = task1.Id,
 Action = "Assigned",
 PerformedBy = u1.Id,
 Details = $"Assigned to {u2.Email}",
 OldStatus = null,
 NewStatus = task1.Status,
 PerformedAt = DateTime.UtcNow
 });

 context.TaskHistories.Add(new TaskHistory
 {
 TaskId = task2.Id,
 Action = "Created",
 PerformedBy = u2.Id,
 Details = $"Task created: {task2.Title}",
 OldStatus = null,
 NewStatus = task2.Status,
 PerformedAt = DateTime.UtcNow
 });

 await context.SaveChangesAsync();
 }
 }
}
catch (Exception ex)
{
 var logger = services.GetRequiredService<ILogger<Program>>();
 logger.LogError(ex, "An error occurred while seeding the database.");
 }

app.Run();
