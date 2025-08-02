using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductsRazor.Models;

[Table("Item")]
public partial class Item
{
    [Key]
    [Column("id_item")]
    public int IdItem { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("price")]
    public int Price { get; set; }

    [Column("id_category")]
    public int IdCategory { get; set; }

    [Column("create_by")]
    [StringLength(50)]
    public string CreateBy { get; set; } = null!;

    [Column("create_date")]
    public DateTime CreateDate { get; set; }

    [Column("update_by")]
    [StringLength(50)]
    public string UpdateBy { get; set; } = null!;

    [Column("update_date")]
    public DateTime UpdateDate { get; set; }

    [Column("isDeleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("IdCategory")]
    [InverseProperty("Items")]
    public virtual Category IdCategoryNavigation { get; set; } = null!;

    [InverseProperty("IdItemNavigation")]
    public virtual ICollection<Serial> Serials { get; set; } = new List<Serial>();
}
