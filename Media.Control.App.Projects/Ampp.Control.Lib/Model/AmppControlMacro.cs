using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ampp.Control.lib.Model
{
    /// <summary>
    /// DataModel for accessing a Macro over RESTApi
    /// </summary>
    public class AmppControlMacro
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("macroSchema")]
        public string MacroSchema { get; set; }

        [JsonPropertyName("commands")]
        public List<Command> Commands { get; set; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; }
    }

    public class Command
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        // "command"라는 이름은 클래스 내에서는 프로퍼티명으로 사용해도 무방합니다.
        [JsonPropertyName("command")]
        public string CommandText { get; set; }

        [JsonPropertyName("application")]
        public string Application { get; set; }

        [JsonPropertyName("workloads")]
        public List<string> Workloads { get; set; }

        // payload는 JSON 내부 값에 따라 구조가 달라질 수 있으므로 Dictionary 형태로 선언합니다.
        [JsonPropertyName("payload")]
        public Dictionary<string, object> Payload { get; set; }

    }
}
