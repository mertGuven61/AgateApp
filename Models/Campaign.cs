using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgateApp.Models
{
	public class Campaign
	{
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		[Display(Name = "Planned Start")]
		[DataType(DataType.Date)]
		public DateTime PlannedStartDate { get; set; }

		[Display(Name = "Planned Finish")]
		[DataType(DataType.Date)]
		public DateTime PlannedFinishDate { get; set; }

		[Display(Name = "Actual Finish")]
		[DataType(DataType.Date)]
		public DateTime? ActualFinishDate { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		[DataType(DataType.Currency)]
		public decimal EstimatedCost { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		[DataType(DataType.Currency)]
		public decimal Budget { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		[DataType(DataType.Currency)]
		public decimal ActualCost { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		[DataType(DataType.Currency)]
		public decimal AmountPaid { get; set; }

		[DataType(DataType.Date)]
		public DateTime? DatePaid { get; set; }

		public string Status { get; set; } = "Planned"; 

		public int ClientId { get; set; }
		public Client? Client { get; set; }

		public List<Advert> Adverts { get; set; } = new List<Advert>();
	}
}
