using System;
using System.ComponentModel.DataAnnotations;

namespace MudBlazorDemo.Models.Auth.JWT.Mock
{
    public record UserModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

}

