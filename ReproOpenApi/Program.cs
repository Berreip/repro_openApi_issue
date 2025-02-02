using ReproOpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();
app.MapOpenApi();
app.UseHttpsRedirection();
app.MapGet("/repro", () =>
{
    return Enumerable.Range(1, 5).Select(_ =>
        new ChangesetDescDto
        {
            Prop1 = new ChangesetIntDto { Added = [] },
            Prop2 = new ChangesetIntDto { Added = [] },
        }).ToArray();
}).WithName("reproendpoint");

app.Run();