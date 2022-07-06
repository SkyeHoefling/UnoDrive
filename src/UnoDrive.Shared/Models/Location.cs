namespace UnoDrive.Models
{
	public class Location
	{
		public string Id { get; set; }
		public string Path { get; set; }
		public bool CanMoveBack => Back != null;
		public bool CanMoveForward => Forward != null;
		public Location Back { get; set; }
		public Location Forward { get; set; }
	}
}