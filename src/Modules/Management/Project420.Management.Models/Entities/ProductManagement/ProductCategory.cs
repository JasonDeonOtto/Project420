using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project420.Shared.Core.Entities;

namespace Project420.Management.Models.Entities.ProductManagement
{
    public class ProductCategory : AuditableEntity
    {
        [MaxLength(50)]
        [Display(Name = "Product Category Name")]
        public string? Name { get; set; } = string.Empty;

        [Required] //Should be PK id is indx correct? apply this to all models please
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; }

        [Required]
        [Display(Name = "Special Rules")]
        public bool SpecialRules { get; set; } = false;

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = false;

    }
}
