using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USSDTest.Models;

[Table("Users")]
public record User
{
    [Key, Column("basemodel_ptr_id")] 
    public Guid Id { get; set; } = default!;

    [Column("first_name")]
    public string FirstName { get; set; } = default!;

    [Column("last_name")]
    public string LastName { get; set; } = default!;

    [Column("mobile")] 
    public string PhoneNumber { get; set; } = default!;
    
    [Column("access_level")]
    public AccessLevel AccessLevel { get; set; }
};

public enum AccessLevel
{
    CANDIDATE,
    AGENT,
    STAFF
}