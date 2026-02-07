using Microsoft.AspNetCore.Identity;

namespace BookwormsOnlineSecurity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string CreditCard { get; set; }  // Will be ENCRYPTED
        public string PhotoPath { get; set; }   // JPG upload path
        public string? LastSessionId { get; set; }
    }
}
