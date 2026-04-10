using Microsoft.EntityFrameworkCore;
using MiminalAPI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDB>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

vvar miljo = builder.Configuration["APP_ENV"] ?? "okänd miljö";

app.MapGet("/health", () => $"Hälsokontroll OK! Miljö: {miljo}");



app.MapGet("/", () => "Välkommen till Todo API! Tillgängliga endpoints:\n GET /todoitems - Hämta alla todos\n GET /todoitems/complete - Hämta alla slutförda todos\n GET /todoitems/{id} - Hämta en todo\n POST /todoitems - Skapa en todo\n PUT /todoitems/{id} - Uppdatera en todo\n PATCH /todoitems/{id} - Delvis uppdatera en todo\n DELETE /todoitems/{id} - Ta bort en todo");



app.MapGet("/todoitems", async (ToDoDB db) =>
    await db.ToDos.ToListAsync());

app.MapGet("/todoitems/complete", async (ToDoDB db) =>
    await db.ToDos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, ToDoDB db) =>
    await db.ToDos.FindAsync(id)
        is ToDo todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async (ToDo todo, ToDoDB db) =>
{
    db.ToDos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, ToDo inputTodo, ToDoDB db) =>
{
    var todo = await db.ToDos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, ToDoDB db) =>
{
    if (await db.ToDos.FindAsync(id) is ToDo todo)
    {
        db.ToDos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.MapPatch("/todoitems/{id}", async (int id, ToDoPatchDto inputTodo, ToDoDB db) =>
{
    var todo = await db.ToDos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    if (inputTodo.Name is not null) todo.Name = inputTodo.Name;
    if (inputTodo.IsComplete is not null) todo.IsComplete = inputTodo.IsComplete.Value;

    await db.SaveChangesAsync();

    return Results.NoContent();
});



app.Run();