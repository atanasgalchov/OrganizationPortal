namespace OrganizationPortal
{
	public class DataResult
	{
		public object Data { get; set; }
		public string Message { get; set; }
		public string RedirectUrl { get; set; }
		public DataResultTypes Type { get; set; }
		public string TypeString { get { return this.Type.ToString(); } }
		public bool IsSuccess { get { return Type == DataResultTypes.Success; } }
	}
	public class DataTableRequestModel
	{
		public int draw { get; set; }
		public int start { get; set; }
		public int length { get; set; }
		public string search { get; set; }

		// custom
		public string orderBy { get; set; }
		public string sortOrder { get; set; }
	}
	public class DataTableDataSourceModel
	{
		public int draw { get; set; }
		public int recordsTotal { get; set; }
		public int recordsFiltered { get; set; }
		public object data { get; set; }
		public string error { get; set; }
	}

	public enum DataResultTypes
	{
		Success,
		Error,
		Warning
	}
}
