using System.ComponentModel.DataAnnotations;

namespace Entities
{
    /// <summary>
    /// Domain Model for Country
    /// </summary>
    public class Country
    {
        [Key]
        public Guid CountryID {  get; set; }
        public string? CountryName { get; set; }


        //Master Model class having a property of the child model type for navigation property
        public virtual ICollection<Person>? Person { get; set; }

    }
}
