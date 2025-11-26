using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgateApp.Models
{
	public class Advert
	{
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		[Display(Name = "Media Channel")]
		public string MediaChannel { get; set; } 

		public string ProductionStatus { get; set; } = "Concept";

		[DataType(DataType.Date)]
		public DateTime ScheduledRunDateStart { get; set; }

		[DataType(DataType.Date)]
		public DateTime ScheduledRunDateEnd { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Cost { get; set; }

		public int CampaignId { get; set; }
		public Campaign? Campaign { get; set; }
	}
}
