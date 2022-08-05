using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USSDTest.Models;

[Table("Agents")]
public record Agent
{
    [Key, Column("basemodel_ptr_id")] 
    public Guid Id { get; set; }

    [Column("ward_code_id")]
    public Guid? WardId { get; set; }
    
    [Column("ps_code_id")]
    public Guid PollingCenterId { get; set; }
    
    [Column("stream_code_id")]
    public Guid? PollingStationId { get; set; }

    [Column("user_id_id")]
    public Guid UserId { get; set; }
}