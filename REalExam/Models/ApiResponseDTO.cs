
using System.Text.Json.Serialization;


namespace RevueCrafters_exam_project.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("revueId")]
        public string? RevueId { get; set; }
    }
}
