namespace Lyapota
{
    public class ApiResult
    {
        public int? StatusCode { get; set; }
        public string StatusText { get; set; } = "-";
        public string Body { get; set; } = string.Empty;
        public string? Error { get; set; }

        public bool IsSuccess()
        {
            return StatusCode.HasValue && StatusCode.Value >= 200 && StatusCode.Value < 300;
        }
    }
}
