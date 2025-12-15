using System;
using System.Collections.Generic;

namespace BL5_PRN212_MustPass_Project.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
