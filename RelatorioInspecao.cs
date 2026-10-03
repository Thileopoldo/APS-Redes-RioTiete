using System.Text.Json;

namespace SecretariaServer
{
    public class RelatorioInspecao
    {
        public string NomeInspetor { get; set; } = string.Empty;
        public string NomeIndustria { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public double NivelPoluicaoPPM { get; set; }
        public DateTime DataHora { get; set; }
        public string Observacoes { get; set; } = string.Empty;

        public string ParaJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static RelatorioInspecao? DeJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<RelatorioInspecao>(json);
        }
    }
}
