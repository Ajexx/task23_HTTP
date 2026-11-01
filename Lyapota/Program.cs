using Lyapota;
using Lyapota.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://jsonplaceholder.typicode.com"),
};
var api = new ApiClient(httpClient);

while (true)
{
    Console.WriteLine("\n=== Main Menu ===");
    Console.WriteLine("1. Get users");
    Console.WriteLine("2. Get user by ID");
    Console.WriteLine("3. Create user");
    Console.WriteLine("4. Delete user by ID");
    Console.WriteLine("5. Check HTTP request status");
    Console.WriteLine("6. Exit");
    Console.Write("Choice: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1": 
            await ShowUsersAsync(); 
            break;
        case "2": 
            await ShowUserByIdAsync();
            break;
        case "3": 
            await CreateUserAsync(); 
            break;
        case "4": 
            await DeleteUserAsync(); 
            break;
        case "5": 
            await CheckStatusesAsync(); 
            break;
        case "6": return;
        default: Console.WriteLine("Invalid menu option."); break;
    }
}

int ReadId()
{
    while (true)
    {
        Console.Write("Enter ID: ");
        if (int.TryParse(Console.ReadLine(), out var id) && id > 0)
        {
            return id;
        }
        Console.WriteLine("Invalid ID. Please try again.");
    }
}

string ReadNonEmpty(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var value = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }
        Console.WriteLine("Value cannot be empty.");
    }
}

bool PrintStatus(ApiResult result)
{
    if (result.Error != null)
    {
        Console.WriteLine(result.Error);
        return false;
    }

    Console.WriteLine($"Status: {result.StatusCode} {result.StatusText}");
    return result.IsSuccess();
}

async Task ShowUsersAsync()
{
    var result = await api.GetUsersAsync();
    if (!PrintStatus(result)) return;

    try
    {
        var users = ApiClient.Deserialize<List<User>>(result.Body) ?? new();
        foreach (var user in users)
        {
            Console.WriteLine($"Id: {user.Id} | Name: {user.Name} | Email: {user.Email}");
        }
    }
    catch (JsonException)
    {
        Console.WriteLine("Failed to parse JSON response.");
    }
}

string ReadEmail(string text)
{
    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    while (true)
    {
        Console.Write(text);
        var value = Console.ReadLine()?.Trim();

        if (!string.IsNullOrEmpty(value) && emailRegex.IsMatch(value))
        {
            return value;
        }

        Console.WriteLine("Invalid email. Please try again.");
    }
}

async Task ShowUserByIdAsync()
{
    var id = ReadId();
    var result = await api.GetUserAsync(id);

    if (result.StatusCode == 404)
    {
        Console.WriteLine($"User with ID {id} not found.");
        return;
    }
    if (!PrintStatus(result)) return;

    try
    {
        var user = ApiClient.Deserialize<User>(result.Body);
        if (user != null)
        {
            Console.WriteLine($"Id: {user.Id}\nName: {user.Name}\nEmail: {user.Email}");
        }
    }
    catch (JsonException)
    {
        Console.WriteLine("Failed to parse JSON response.");
    }
}

async Task CreateUserAsync()
{
    var user = new User
    {
        Name = ReadNonEmpty("Name: "),
        Email = ReadEmail("Email: ")
    };
    user.Id = 0;

    var result = await api.CreateUserAsync(new { user.Name, user.Email });
    if (PrintStatus(result))
    {
        Console.WriteLine($"Body: {result.Body}");
    }
}

async Task DeleteUserAsync()
{
    var id = ReadId();
    var result = await api.DeleteUserAsync(id);
    if (PrintStatus(result))
    {
        Console.WriteLine($"User {id} deleted.");
    }
}

async Task CheckStatusesAsync()
{
    string[] paths = { "/users/1", "/users/9999", "/invalid-resource" };

    foreach (var path in paths)
    {
        var result = await api.SendAsync(HttpMethod.Get, path);
        var url = new Uri(httpClient.BaseAddress!, path);

        Console.WriteLine($"\nURL: {url}");
        if (result.Error != null)
        {
            Console.WriteLine($"Error: {result.Error}");
            continue;
        }

        Console.WriteLine($"Status: {result.StatusCode} {result.StatusText}");
        Console.WriteLine($"Success: {(result.IsSuccess() ? "yes" : "no")}");
        Console.WriteLine(result.IsSuccess() ? "Resource is available." : result.StatusCode == 404 ? "Resource not found." : "Request failed.");
    }
}