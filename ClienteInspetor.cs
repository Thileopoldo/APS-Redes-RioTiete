using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace SecretariaServer
{
    public class ClienteInspetor : IDisposable
    {
        private readonly string _servidorIp;
        private readonly int _porta;
        private TcpClient? _cliente;

        public ClienteInspetor(string servidorIp, int porta)
        {
            _servidorIp = servidorIp;
            _porta = porta;
        }

        public async Task<bool> ConectarAsync()
        {
            try
            {
                _cliente = new TcpClient();
                await _cliente.ConnectAsync(_servidorIp, _porta);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar ao servidor: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarRelatorioAsync(RelatorioInspecao relatorio)
        {
            if (_cliente is null || !_cliente.Connected)
            {
                var conectado = await ConectarAsync();
                if (!conectado)
                {
                    return false;
                }
            }

            try
            {
                var json = JsonSerializer.Serialize(relatorio);
                var dados = Encoding.UTF8.GetBytes(json);

                using var stream = _cliente!.GetStream();
                await stream.WriteAsync(dados, 0, dados.Length);
                await stream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar relatório: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _cliente?.Close();
            _cliente?.Dispose();
        }
    }
}
