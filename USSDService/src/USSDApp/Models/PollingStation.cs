using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using USSDTest.Models.Common;

namespace USSDTest.Models;

[Table("Streams")]
public record PollingStation
{
    [ForeignKey(nameof(BaseEntity)), Column("basemodel_ptr_id")] 
    public Guid Id { get; set; }

    [Column("name")] 
    public string Name { get; set; } = string.Empty;

    [Column("stream_code")] 
    public string Code { get; set; }
    
    [Column("ps_code_id")]
    public Guid PollingCenterId { get; set; }

    public BaseEntity BaseEntity { get; set; } = default!;

    public PollingCenter PollingCenter { get; set; } = default!;
};