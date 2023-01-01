using System;
using System.Collections.Generic;

namespace MudBlazorDemo.DataAccess;

public partial class Address
{
    public int Id { get; set; }

    public string StreetAddress { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Zip { get; set; } = null!;

    public int? PersonId { get; set; }

    public virtual Person? Person { get; set; }
}
