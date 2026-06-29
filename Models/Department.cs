using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackNGo.Models
{
    /// <summary>
    /// Department entity (Table 3).
    /// Represents an organizational department within the City Government of Mati.
    /// </summary>
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string DepartmentName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string DepartmentCode { get; set; } = string.Empty;

        public int? HeadUserId { get; set; }
        [ForeignKey("HeadUserId")]
        public User? HeadUser { get; set; }
    }
}
