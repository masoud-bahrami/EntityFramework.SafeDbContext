using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.SafeDbContext
{
    [NotMapped]
    public class BaseDbEntity
    {
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedOn { get; set; }
        public virtual string UpdatedBy { get; set; }
        public virtual DateTime? UpdatedOn { get; set; }
    }
    public class BaseDbEntity<TId> : BaseDbEntity
        where TId : struct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TId Id { get; set; }
    }
}