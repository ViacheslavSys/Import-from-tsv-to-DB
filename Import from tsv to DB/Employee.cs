using System;
using System.Collections.Generic;

namespace Import_from_tsv_to_DB;

public partial class Employee
{
    public long Id { get; set; }

    public long? Departament { get; set; }

    public string? Fullname { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public long? Jobtitleid { get; set; }

    public virtual Departament? DepartamentNavigation { get; set; }

    public virtual ICollection<Departament> Departaments { get; } = new List<Departament>();

    public virtual Jobtitle? Jobtitle { get; set; }
}
