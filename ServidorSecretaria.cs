using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace SecretariaServer
{
    public class ServidorSecretaria : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<TcpClient> _clientes = new();

        public event EventHandler<RelatorioInspecao>? RelatorioRecebido;

        public ServidorSecretaria(string ip, int porta)
        {
            var endereco = IPAddress.Parse(ip);
            _listener = new TcpListener(endereco, porta);
        }

        public async Task IniciarAsync()
        {
            _listener.Start();
            Console.WriteLine($"Servidor ouvindo em {_listener.LocalEndpoint}");

            while (!_cts.IsCancellationRequested)
            {
                TcpClient cliente;

                try
                {
                    cliente = await _listener.AcceptTcpClientAsync(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao aceitar cliente: {ex.Message}");
                    continue;
                }

                lock (_clientes)
                {
                    _clientes.Add(cliente);
                }

                Console.WriteLine($"Cliente conectado: {cliente.Client.RemoteEndPoint}");

                _ = Task.Run(() => ProcessarClienteAsync(cliente), _cts.Token);
            }
        }

        private async Task ProcessarClienteAsync(TcpClient cliente)
        {
            try
            {
                using var stream = cliente.GetStream();
                var buffer = new byte[4096];

                while (!_cts.IsCancellationRequested && cliente.Connected)
                {
                    int bytesLidos;

                    try
                    {
                        bytesLidos = await stream.ReadAsync(buffer, _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Conexão encerrada pelo cliente.");
                        break;
                    }

                    if (bytesLidos == 0)
                    {
                        break;
                    }

                    var mensagem = Encoding.UTF8.GetString(buffer, 0, bytesLidos).Trim();

                    if (string.IsNullOrWhiteSpace(mensagem))
                    {
                        continue;
                    }

                    Console.WriteLine($"Mensagem recebida: {mensagem}");

                    var relatorio = JsonSerializer.Deserialize<RelatorioInspecao>(mensagem);

                    if (relatorio is not null)
                    {
                        RelatorioRecebido?.Invoke(this, relatorio);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar cliente: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Cliente desconectado: {cliente.Client.RemoteEndPoint}");

                lock (_clientes)
                {
                    _clientes.Remove(cliente);
                }

                cliente.Dispose();
            }
        }

        public void Parar()
        {
            _cts.Cancel();
            _listener.Stop();

            lock (_clientes)
            {
                foreach (var cliente in _clientes)
                {
                    try
                    {
                        cliente.Close();
                    }
                    catch
                    {
                        // Ignora erro ao fechar conexão.
                    }
                }

                _clientes.Clear();
            }
        }

        public void Dispose()
        {
            Parar();
            _cts.Dispose();
        }
    }
}
