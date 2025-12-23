using Ma.TimeManagement.Api.Extentions;
using Ma.TimeManagement.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddLocalServices();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//.AddNegotiate() // Windows Authentication (Kerberos/NTLM)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "sdfgtryeu546okfdjfgvnhgitdfugtre"))
        {
            KeyId = "LocalServerKey"
        },
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(requireAuthPolicy);

//builder.Services.AddAuthorization(options =>
//{
//    var policy = new AuthorizationPolicyBuilder()
//        .AddAuthenticationSchemes(
//            //NegotiateDefaults.AuthenticationScheme,
//            JwtBearerDefaults.AuthenticationScheme)
//        .RequireAuthenticatedUser()
//        .Build();

//    options.DefaultPolicy = policy;
//    //options.FallbackPolicy = policy;   // ← critical line
//});

// 4. Add Controllers + Swagger (optional but recommended)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ma.TimeManagement API", Version = "v1" });
    c.UseAllOfToExtendReferenceSchemas();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: <b>Bearer eyJhbGci...</b>",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer",doc),
            []
        }
    });

    //c.OperationFilter<SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContextFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

    using var applicationDbContext = dbContextFactory.CreateDbContext();

    applicationDbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        //options => // UseSwaggerUI is called only in Development.
        //{
        //    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        //    options.RoutePrefix = string.Empty;
        //}
        );
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
