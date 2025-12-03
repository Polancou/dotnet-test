using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            CreationDate = u.CreationDate
        });
    }

    public async Task UpdateUserRoleAsync(int userId, UserRole newRole)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.UpdateRole(newRole);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        userRepository.Remove(user);
        await unitOfWork.SaveChangesAsync();
    }
}
