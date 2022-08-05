using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USSDTest.Models;

[Table("Wards")]
public record Ward
{
    [Key, Column("basemodel_ptr_id")] 
    public Guid Id { get; set; } = default!;

    [Column("name")]
    public string Name { get; set; } = default!;

    public ICollection<PollingCenter> PollingCenters { get; set; } = default!;
    public ICollection<Agent> Agents { get; set; } = default!;
}