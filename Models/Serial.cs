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

    [InverseProperty("Serial")]
    public virtual ICollection<JoinTable> JoinTables { get; set; } = new List<JoinTable>();
}
