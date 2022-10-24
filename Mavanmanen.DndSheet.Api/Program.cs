using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;

const string CHARACTER_FOLDER = "./characters/";

if (!Directory.Exists(CHARACTER_FOLDER))
{
    Directory.CreateDirectory(CHARACTER_FOLDER);
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policyBuilder =>
    policyBuilder
        .WithOrigins("http://127.0.0.1:5173")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()
);

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/sheet/{id}", [Authorize] async (HttpRequest request, string id) =>
{
    using var reader = new StreamReader(request.Body);
    var json = await reader.ReadToEndAsync();
    await File.WriteAllTextAsync($"{CHARACTER_FOLDER}{id}", json);

    return Results.Ok();
});

app.MapGet("/sheet", [Authorize] async () =>
{
    var retVal = new List<object>();
    foreach (var file in Directory.EnumerateFiles(CHARACTER_FOLDER).Select(f => new FileInfo(f)))
    {
        var json = await File.ReadAllTextAsync(file.FullName);
        retVal.Add(new
        {
            Name = file.Name,
            Json = json
        });
    }

    return Results.Json(retVal);
});

app.Run();