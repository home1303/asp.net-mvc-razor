using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProductsRazor.Models;

[Table("Serial")]
public partial class Serial
{
    [Key]
    [Column("id_serial")]
    public int IdSerial { get; set; }

    [Column("serial_number")]
    [StringLength(50)]
    [Unicode(false)]
    public string SerialNumber { get; set; } = null!;

    [Column("cretae_date")]
    public DateTime CretaeDate { get; set; }

    [Column("update_date")]
    public DateTime UpdateDate { get; set; }

    [Column("status")]
    [StringLength(50)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("id_item")]
    public int IdItem { get; set; }

    [Column("isdeleted")]
    public bool Isdeleted { get; set; }

    [ForeignKey("IdItem")]
    [InverseProperty("Serials")]
    public virtual Item IdItemNavigation { get; set; } = null!;
}
