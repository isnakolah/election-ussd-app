using System.ComponentModel.DataAnnotations.Schema;
using USSDTest.Models.Common;

namespace USSDTest.Models;

[Table("Polling_Stations")]
public record PollingCenter
{
    [ForeignKey(nameof(BaseEntity)), Column("basemodel_ptr_id")] 
    public Guid Id { get; set; }

    [Column("name")] 
    public string Name { get; set; } = string.Empty;

    [Column("ward_code_id")]
    public Guid WardId { get; set; }

    public Ward Ward { get; set; } = default!;
    public BaseEntity BaseEntity { get; set; } = default!;
    public ICollection<PollingStation> PollingStations { get; set; } = default!;
};