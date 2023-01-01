using System;
using System.Collections.Generic;

namespace MudBlazorDemo.DataAccess;

public partial class EmailAddress
{
    public int Id { get; set; }

    public string EmailAddress1 { get; set; } = null!;

    public int? PersonId { get; set; }

    public virtual Person? Person { get; set; }
}
