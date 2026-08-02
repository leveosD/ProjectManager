using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Sibers.Core.Entities;
using Sibers.Core.Enums;
using Sibers.Core.Interfaces;

namespace Sibers.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var employeeRepo = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();

        // 1. Seed Roles
        string[] roles = [UserRoles.Director, UserRoles.ProjectManager, UserRoles.Employee];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Initial Users and Employee records
        await SeedUserAndEmployee(userManager, employeeRepo, "director@sibers.com", "Password123!", "Alice", "DirectorLastName", "Igorivna", UserRoles.Director);
        await SeedUserAndEmployee(userManager, employeeRepo, "pm@sibers.com", "Password123!", "Bob", "ManagerLastName", "Sergeevich", UserRoles.ProjectManager);
        await SeedUserAndEmployee(userManager, employeeRepo, "employee1@sibers.com", "Password123!", "Charlie", "DevLastName", "Petrovich", UserRoles.Employee);
        await SeedUserAndEmployee(userManager, employeeRepo, "employee2@sibers.com", "Password123!", "Diana", "QALastName", "Alexandrovna", UserRoles.Employee);

        await context.SaveChangesAsync();
    }

    private static async Task SeedUserAndEmployee(
        UserManager<IdentityUser> userManager,
        IEmployeeRepository employeeRepo,
        string email,
        string password,
        string firstName,
        string lastName,
        string middleName,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser == null)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);

                var employee = new Employee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    Email = email,
                    UserId = user.Id
                };

                employeeRepo.Add(employee);
            }
        }
    }
}
