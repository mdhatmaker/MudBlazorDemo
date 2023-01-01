using System;
using System.Collections.Generic;

namespace MudBlazorDemo.DataAccess;

public partial class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; } = new List<Address>();

    public virtual ICollection<EmailAddress> EmailAddresses { get; } = new List<EmailAddress>();
}
