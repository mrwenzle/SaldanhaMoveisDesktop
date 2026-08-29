using System.Windows;
using System.Windows.Input;

namespace SaldanhaMoveisDesktop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

           
            inputUsuario.Focus();
        }

        private void ClicouEntrar(object sender, RoutedEventArgs e)
        {
            RealizarLogin();
        }

        private void ClicouSair(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Permite logar apertando a tecla ENTER
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter)
            {
                RealizarLogin();
            }
        }

        private void RealizarLogin()
        {
            string usuario = inputUsuario.Text;
            string senha = inputSenha.Password;

            // Validação simples de credenciais
            if (usuario == "admin" && senha == "1234")
            {
                // Instancia e abre a tela principal do ERP
                SistemaERP telaPrincipal = new SistemaERP();
                telaPrincipal.Show();

                // Fecha a tela de login
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuário ou senha incorretos!", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                inputSenha.Clear();
                inputSenha.Focus();
            }
        }
    }
}