using ProduceAndDistributeTest.Models;

//Setup setup = new Setup();
//ProduceAndDistribute proc = new ProduceAndDistribute(setup);
//Console.WriteLine("Test!");
//proc.StartProcess(100);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseDeveloperExceptionPage(); 
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseHttpsRedirection();

app.Run();


