using AgateApp.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class AdvertNote
{
	public int Id { get; set; }

	[Required]
	public string NoteText { get; set; }

	public string AuthorName { get; set; } // Notu yazan kişi (Kullanıcı Adı veya Email)

	public DateTime CreatedAt { get; set; } = DateTime.Now;

	// Advert ile İlişki
	public int AdvertId { get; set; }
	public virtual Advert Advert { get; set; }
}