using System;
using System.Collections.Generic;

namespace Import_from_tsv_to_DB;

public partial class Departament
{
    public long Id { get; set; }

    public long? Parentid { get; set; }

    public long? Managerid { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Employee> Employees { get; } = new List<Employee>();

    public virtual ICollection<Departament> InverseParent { get; } = new List<Departament>();

    public virtual Employee? Manager { get; set; }

    public virtual Departament? Parent { get; set; }
}
