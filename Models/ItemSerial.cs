using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductsRazor.Models;

[PrimaryKey("ItemId", "SerialId")]
[Table("Item_Serial")]
public partial class ItemSerial
{
    [Key]
    public int ItemId { get; set; }

    [Key]
    public int SerialId { get; set; }

    public string? Remark { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("ItemSerials")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("SerialId")]
    [InverseProperty("ItemSerials")]
    public virtual Serial Serial { get; set; } = null!;
}
