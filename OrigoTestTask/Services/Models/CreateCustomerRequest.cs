﻿using System.ComponentModel.DataAnnotations;

namespace OrigoTestTask.Services.Models
{
    public class CreateCustomerRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string SocialSecurityNumber { get; set; }
    }

}
