using CampusQ.MVP.Data;
using CampusQ.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("CampusQ") ?? DbConfig.ConnectionString;
builder.Services.AddScoped(_ => new QueueRepository(connectionString));
builder.Services.AddScoped<TicketStatusService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/ticket/{ticketNumber:int}/status", (int ticketNumber, TicketStatusService service) =>
{
    var result = service.GetStatus(ticketNumber);
    return Results.Json(result);
});

app.Run();
