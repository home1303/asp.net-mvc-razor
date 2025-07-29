using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductsRazor.Models;

[Table("Category")]
public partial class Category
{
    [Key]
    [Column("id_category")]
    public int IdCategory { get; set; }

    [Column("name_category")]
    [StringLength(50)]
    public string NameCategory { get; set; } = null!;

    [InverseProperty("IdCategoryNavigation")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
