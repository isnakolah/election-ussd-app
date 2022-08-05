using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USSDTest.Models;

[Table("Results")]
public record Result
{
    [Key, Column("basemodel_ptr_id")] 
    public Guid Id { get; set; }
    
    [Column("candidate_name_id")]
    public Guid? CandidateId { get; set; }
    
    [Column("votes")]
    public int? Votes { get; set; }
    
    [Column("user_id_id")]
    public Guid UserId { get; set; }
    
    [Column("stream_id")]
    public Guid PollingStationId { get; set; }

    [Column("position")] 
    public int Position { get; set; } = 2;
    
    [Column("disputed")]
    public int? Disputed { get; set; }

    [Column("spoilt")]
    public int? Spoilt { get; set; }
}
