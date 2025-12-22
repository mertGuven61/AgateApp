using System.ComponentModel.DataAnnotations;

namespace AgateApp.Models
{
	public class CampaignAssignment
	{
		public int Id { get; set; }

		public int CampaignId { get; set; }
		public Campaign? Campaign { get; set; }

		// Personelin (IdentityUser) Email adresi veya ID'si
		public string StaffEmail { get; set; }
	}
}