using System.ComponentModel.DataAnnotations;

namespace AgateApp.Models
{
	public class Client
	{
		public int Id { get; set; }

		[Required]
		[Display(Name = "Company Name")]
		public string CompanyName { get; set; }

		[Display(Name = "Address Line 1")]
		public string AddressLine1 { get; set; }

		[Display(Name = "Address Line 2")]
		public string AddressLine2 { get; set; }

		public string City { get; set; }

		[Display(Name = "Post Code")]
		public string PostalCode { get; set; }

		[Display(Name = "Contact Person")]
		public string ContactPersonName { get; set; }

		[EmailAddress]
		public string ContactEmail { get; set; }

		[Phone]
		public string PhoneNumber { get; set; }

		public List<Campaign> Campaigns { get; set; } = new List<Campaign>();
	}
}
