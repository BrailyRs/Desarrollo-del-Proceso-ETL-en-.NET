namespace ETLWorkerService.Application.DTOs
{
    public class ETLConfiguration
    {
        public string DataSource { get; set; } = "csv";
        public string CsvBasePath { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = string.Empty;
        public int BulkInsertBatchSize { get; set; } = 10000;
        public string ExecutionMode { get; set; } = "Automatic";
        public bool RunOnStartup { get; set; } = true;
    }
}
