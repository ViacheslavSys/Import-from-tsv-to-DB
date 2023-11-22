using System;
using System.Collections.Generic;

namespace Import_from_tsv_to_DB;

public partial class Jobtitle
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Employee> Employees { get; } = new List<Employee>();

   
}
