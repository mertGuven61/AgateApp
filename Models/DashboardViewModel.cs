namespace AgateApp.Models
{
	public class DashboardViewModel
	{
		// List of all clients to display in the first column
		public List<Client> Clients { get; set; } = new List<Client>();

		// List of campaigns belonging to the selected client (Column 2)
		public List<Campaign> Campaigns { get; set; } = new List<Campaign>();

		// List of adverts belonging to the selected campaign (Column 3)
		public List<Advert> Adverts { get; set; } = new List<Advert>();

		// To highlight the selected items in the UI
		public int? SelectedClientId { get; set; }
		public int? SelectedCampaignId { get; set; }
	}
}
