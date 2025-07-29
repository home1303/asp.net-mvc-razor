using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductsRazor.Models;

[PrimaryKey("ItemId", "SerialId")]
[Table("join_table")]
public partial class JoinTable
{
    [Key]
    public int ItemId { get; set; }

    [Key]
    public int SerialId { get; set; }

    public string? Remark { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("JoinTables")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("SerialId")]
    [InverseProperty("JoinTables")]
    public virtual Serial Serial { get; set; } = null!;
}
