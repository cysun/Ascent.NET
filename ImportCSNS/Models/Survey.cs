using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportCSNS.Models;

public class Survey
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
}
