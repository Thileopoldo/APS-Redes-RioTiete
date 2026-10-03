using System.Windows.Forms;

namespace SecretariaServer
{
    public class InspetorForm : Form
    {
        private readonly TextBox txtIpServidor;
        private readonly TextBox txtPorta;
        private readonly TextBox txtNomeInspetor;
        private readonly TextBox txtNomeIndustria;
        private readonly ComboBox cmbLocalizacao;
        private readonly NumericUpDown nudNivelPoluicao;
        private readonly TextBox txtObservacoes;
        private readonly Button btnEnviarRelatorio;

        public InspetorForm()
        {
            Text = "Inspetor do Rio Tietê";
            Size = new Size(700, 550);
            StartPosition = FormStartPosition.CenterScreen;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 7,
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            for (int i = 0; i < 7; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            AddLabel(layout, "IP do Servidor:", 0, 0);
            txtIpServidor = new TextBox { Text = "127.0.0.1", Width = 220 };
            layout.Controls.Add(txtIpServidor, 1, 0);

            AddLabel(layout, "Porta:", 0, 1);
            txtPorta = new TextBox { Text = "5000", Width = 220 };
            layout.Controls.Add(txtPorta, 1, 1);

            AddLabel(layout, "Nome do Inspetor:", 0, 2);
            txtNomeInspetor = new TextBox { Width = 220 };
            layout.Controls.Add(txtNomeInspetor, 1, 2);

            AddLabel(layout, "Nome da Indústria:", 0, 3);
            txtNomeIndustria = new TextBox { Width = 220 };
            layout.Controls.Add(txtNomeIndustria, 1, 3);

            AddLabel(layout, "Localização:", 0, 4);
            cmbLocalizacao = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 220
            };
            cmbLocalizacao.Items.AddRange(new object[]
            {
                "Salesópolis",
                "Suzano",
                "Mogi das Cruzes",
                "São Paulo"
            });
            cmbLocalizacao.SelectedIndex = 0;
            layout.Controls.Add(cmbLocalizacao, 1, 4);

            AddLabel(layout, "Nível de Poluição (PPM):", 0, 5);
            nudNivelPoluicao = new NumericUpDown
            {
                Width = 220,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 10000,
                Value = 0
            };
            layout.Controls.Add(nudNivelPoluicao, 1, 5);

            AddLabel(layout, "Observações:", 0, 6);
            txtObservacoes = new TextBox
            {
                Multiline = true,
                Height = 120,
                Width = 220
            };
            layout.Controls.Add(txtObservacoes, 1, 6);

            btnEnviarRelatorio = new Button
            {
                Text = "Enviar Relatório",
                Width = 200,
                Anchor = AnchorStyles.Left
            };
            btnEnviarRelatorio.Click += BtnEnviarRelatorio_Click;

            var painelBotao = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60
            };

            painelBotao.Controls.Add(btnEnviarRelatorio);
            btnEnviarRelatorio.Location = new Point(20, 15);

            Controls.Add(layout);
            Controls.Add(painelBotao);
        }

        private static void AddLabel(TableLayoutPanel layout, string texto, int coluna, int linha)
        {
            var label = new Label
            {
                Text = texto,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 8)
            };

            layout.Controls.Add(label, coluna, linha);
        }

        private async void BtnEnviarRelatorio_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIpServidor.Text) ||
                string.IsNullOrWhiteSpace(txtPorta.Text) ||
                string.IsNullOrWhiteSpace(txtNomeInspetor.Text) ||
                string.IsNullOrWhiteSpace(txtNomeIndustria.Text))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Dados incompletos");
                return;
            }

            if (!int.TryParse(txtPorta.Text, out var porta) || porta <= 0 || porta > 65535)
            {
                MessageBox.Show("Informe uma porta TCP válida.", "Porta inválida");
                return;
            }

            var relatorio = new RelatorioInspecao
            {
                NomeInspetor = txtNomeInspetor.Text.Trim(),
                NomeIndustria = txtNomeIndustria.Text.Trim(),
                Localizacao = cmbLocalizacao.SelectedItem?.ToString() ?? string.Empty,
                NivelPoluicaoPPM = (double)nudNivelPoluicao.Value,
                DataHora = DateTime.Now,
                Observacoes = txtObservacoes.Text.Trim()
            };

            var cliente = new ClienteInspetor(txtIpServidor.Text.Trim(), porta);

            try
            {
                var enviado = await cliente.EnviarRelatorioAsync(relatorio);

                if (enviado)
                {
                    MessageBox.Show("Relatório enviado com sucesso.", "Sucesso");
                }
                else
                {
                    MessageBox.Show("Não foi possível enviar o relatório.", "Erro");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao enviar relatório: {ex.Message}", "Erro");
            }
            finally
            {
                cliente.Dispose();
            }
        }
    }
}
