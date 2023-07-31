using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using latest.Models;
using Microsoft.AspNetCore.Identity;
using latest.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using latest.Stripe;
using latest.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddDbContext<CareGiverContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
// builder.Services.AddDbContext<CareUserContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddDbContext<MContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


// builder.Services.Configure<DataProtectionTokenProviderOptions>(o => {
//     o.Name = "CareGiver";
//     o.TokenLifespan = TimeSpan.FromHours(1);
// });
// builder.Services.AddIdentity<Care, IdentityRole>()
// .AddEntityFrameworkStores<MContext>()
// builder.Services.AddScoped<IUserTwoFactorTokenProvider<CareGiver>, CareTokenProvider<CareGiver>>();
// builder.Services.AddScoped<IUserTwoFactorTokenProvider<CareUser>, CareTokenProvider<CareUser>>();

builder.Services.AddIdentity<CareUser, CareRole>(options => {
    options.SignIn.RequireConfirmedEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(1);
}).AddEntityFrameworkStores<MContext>()
.AddDefaultTokenProviders();

// builder.Services.AddIdentityCore<CareGiver>().AddEntityFrameworkStores<MContext>().AddUserManager<CareGiverManager>();
// builder.Services.AddIdentityCore<CareConsumer>().AddEntityFrameworkStores<MContext>();


// builder.Services.AddIdentity<CareUser, IdentityRole>().AddEntityFrameworkStores<CareUserContext>().AddDefaultTokenProviders();


// builder.Services.AddIdentity<CareGiver, IdentityRole>().AddEntityFrameworkStores<MContext>().AddDefaultTokenProviders();
// builder.Services.AddIdentity<CareUser, IdentityRole>().AddEntityFrameworkStores<MContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("SOME MAGIC UNICORNS GENERATE THIS SECRET")),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddSingleton(builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
builder.Services.AddScoped<IEmailSender, EmailSender>();


builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddStripeInfrastructure(builder.Configuration);

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder => {
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.UseCors("corsapp");
app.Run();
