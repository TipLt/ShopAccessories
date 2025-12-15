using System;
using System.Collections.Generic;

namespace BL5_PRN212_MustPass_Project.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? CustomerId { get; set; }

    public string? FullName { get; set; }

    public bool IsActive { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
