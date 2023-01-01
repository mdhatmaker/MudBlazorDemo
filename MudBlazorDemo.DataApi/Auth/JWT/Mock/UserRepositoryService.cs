using System;
using System.ComponentModel.DataAnnotations;

namespace MudBlazorDemo.Models.Auth.JWT.Mock
{
    // For code in JWT.Mock, we have created UserDTO and model classes to mock our
    // in-memory user store and a repository method to verify the credentials.

    public interface IUserRepositoryService
    {
        UserDto GetUser(UserModel userModel);
    }



    public class UserRepositoryService : IUserRepositoryService
    {
        private List<UserDto> _users => new()
        {
            new("admin", "abc123"),
        };

        public UserDto GetUser(UserModel userModel)
        {
            return _users.FirstOrDefault(x => string.Equals(x.UserName, userModel.UserName) && string.Equals(x.Password, userModel.Password));
        }
    }

}

