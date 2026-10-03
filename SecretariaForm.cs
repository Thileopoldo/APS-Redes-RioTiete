using System.ComponentModel;
using System.Windows.Forms;

namespace SecretariaServer
{
    public class SecretariaForm : Form
    {
        private readonly TextBox txtPorta;
        private readonly Button btnIniciarServidor;
        private readonly Button btnAbrirInspetor;
        private readonly DataGridView dgvRelatorios;
        private readonly BindingList<RelatorioInspecao> _relatorios;
        private ServidorSecretaria? _servidor;

        public SecretariaForm()
        {
            Text = "Secretaria do Meio Ambiente";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;

            _relatorios = new BindingList<RelatorioInspecao>();

            var painelTopo = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(10)
            };

            var lblPorta = new Label
            {
                Text = "Porta TCP:",
                AutoSize = true,
                Margin = new Padding(0, 8, 10, 0)
            };

            txtPorta = new TextBox
            {
                Width = 120,
                Text = "5000",
                Margin = new Padding(0, 5, 10, 0)
            };

            btnIniciarServidor = new Button
            {
                Text = "Iniciar Servidor",
                Width = 150,
                Margin = new Padding(0, 3, 0, 0)
            };

            btnAbrirInspetor = new Button
            {
                Text = "Abrir Inspetor",
                Width = 150,
                Margin = new Padding(0, 3, 0, 0)
            };

            btnIniciarServidor.Click += BtnIniciarServidor_Click;
            btnAbrirInspetor.Click += BtnAbrirInspetor_Click;

            painelTopo.Controls.Add(lblPorta);
            painelTopo.Controls.Add(txtPorta);
            painelTopo.Controls.Add(btnIniciarServidor);
            painelTopo.Controls.Add(btnAbrirInspetor);

            dgvRelatorios = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvRelatorios.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Nome do Inspetor",
                DataPropertyName = nameof(RelatorioInspecao.NomeInspetor)
            });

            dgvRelatorios.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Nome da Indústria",
                DataPropertyName = nameof(RelatorioInspecao.NomeIndustria)
            });

            dgvRelatorios.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Localização",
                DataPropertyName = nameof(RelatorioInspecao.Localizacao)
            });

            dgvRelatorios.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Nível de Poluição (PPM)",
                DataPropertyName = nameof(RelatorioInspecao.NivelPoluicaoPPM)
            });

            dgvRelatorios.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Data/Hora",
                DataPropertyName = nameof(RelatorioInspecao.DataHora)
            });

            dgvRelatorios.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Observações",
                DataPropertyName = nameof(RelatorioInspecao.Observacoes)
            });

            dgvRelatorios.DataSource = _relatorios;

            Controls.Add(dgvRelatorios);
            Controls.Add(painelTopo);
        }

        private async void BtnIniciarServidor_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPorta.Text.Trim(), out var porta) || porta <= 0 || porta > 65535)
            {
                MessageBox.Show("Informe uma porta TCP válida entre 1 e 65535.", "Porta inválida");
                return;
            }

            if (_servidor is not null)
            {
                MessageBox.Show("O servidor já está em execução.", "Aviso");
                return;
            }

            try
            {
                btnIniciarServidor.Enabled = false;

                _servidor = new ServidorSecretaria("0.0.0.0", porta);
                _servidor.RelatorioRecebido += ServidorSecretaria_RelatorioRecebido;

                await Task.Run(() => _servidor.IniciarAsync());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao iniciar o servidor: {ex.Message}", "Erro");
            }
        }

        private void BtnAbrirInspetor_Click(object sender, EventArgs e)
        {
            var formInspetor = new InspetorForm();
            formInspetor.Show();
        }

        private void ServidorSecretaria_RelatorioRecebido(object? sender, RelatorioInspecao relatorio)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<RelatorioInspecao>(ServidorSecretaria_RelatorioRecebido), sender, relatorio);
                return;
            }

            _relatorios.Add(relatorio);

            if (dgvRelatorios.Rows.Count > 0)
            {
                dgvRelatorios.FirstDisplayedScrollingRowIndex = dgvRelatorios.Rows.Count - 1;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _servidor?.Parar();
            _servidor?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
