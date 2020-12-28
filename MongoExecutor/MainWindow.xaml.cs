using System;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace MongoExecutor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Mongo.MongoExecutor _mongo;
		public MainWindow()
		{
			InitializeComponent();

			this.Loaded += this.OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (this._mongo == null)
			{
				this._mongo = new Mongo.MongoExecutor();
			}

			this.xCodeEditor.Document.Text = this._mongo.Script;
		}

		public void OnExecuteScript(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(this.xHost.Text))
			{
				MessageBox.Show(this, "The host string must be not empty.");
				return;
			}

			this._mongo.Script = this.xCodeEditor.Document.Text;
			var result = this._mongo.ExecuteCommand(this.xHost.Text, string.Empty, string.Empty);
			this.xResultBox.Text = result;
		}

		protected override void OnClosed(EventArgs e)
		{
			this._mongo?.Dispose();

			base.OnClosed(e);
		}
	}
}
