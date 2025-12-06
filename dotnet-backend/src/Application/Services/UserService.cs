using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Service for user management operations, such as retrieving users, updating roles, and deleting users.
/// </summary>
public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
{
    /// <summary>
    /// Retrieves all users from the repository and maps them to <see cref="UserDto"/>.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="UserDto"/> objects representing all users.
    /// </returns>
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        // Fetch all user entities from the repository.
        var users = await userRepository.GetAllAsync();

        // Map each user entity to a UserDto object and return the collection.
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

    /// <summary>
    /// Updates the role of an existing user.
    /// </summary>
    /// <param name="userId">The ID of the user whose role will be updated.</param>
    /// <param name="newRole">The new role to assign to the user.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user with the specified ID does not exist.</exception>
    public async Task UpdateUserRoleAsync(int userId, UserRole newRole)
    {
        // Retrieve the user by ID; throw if not found.
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        // Update the user's role.
        user.UpdateRole(newRole);

        // Inform the repository that the user entity has changed.
        userRepository.Update(user);

        // Persist changes to the database.
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an existing user from the repository.
    /// </summary>
    /// <param name="userId">The ID of the user to delete.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user with the specified ID does not exist.</exception>
    public async Task DeleteUserAsync(int userId)
    {
        // Retrieve the user by ID; throw if not found.
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        // Remove the user entity from the repository.
        userRepository.Remove(user);

        // Persist changes to the database.
        await unitOfWork.SaveChangesAsync();
    }
}
