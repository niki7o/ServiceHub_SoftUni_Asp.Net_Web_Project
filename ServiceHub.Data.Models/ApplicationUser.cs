using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Data.Models
{
    public class ApplicationUser:IdentityUser
    {

        public ApplicationUser() : base() { }

        public bool IsBusiness { get; set; }
        public DateTime? BusinessExpiresOn { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
     
        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
