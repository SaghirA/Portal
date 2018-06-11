using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Job.Models
{
    public class SignupVM
    {
        [Required(ErrorMessage="Please provide username",AllowEmptyStrings=false)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please provide full name", AllowEmptyStrings = false)]
        public string Name { get; set; }
        [Required(ErrorMessage ="Please provide password",AllowEmptyStrings =false)]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [StringLength(50,MinimumLength =8,ErrorMessage ="Passwordmust be 8 char long")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage ="Confirm Password does not match.")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string ConfrimPassword { get; set; }

    }
}