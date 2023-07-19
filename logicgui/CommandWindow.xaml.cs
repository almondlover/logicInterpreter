using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using logic;

namespace logicgui
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>

    
    public partial class CommandWindow : Window
    {
        public string funcName;
        public CommandWindow()
        {
            InitializeComponent();
            
        }

        public virtual void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string input = FunctionNameText.Text+"("+FunctionArgumentsText.Text+"):"+"\"" +FunctionBodyText.Text+"\"";

            funcName = LogicInterpreter.Define(input);
            if (funcName == null) { MessageBox.Show("Invalid data"); return; }
           
            DialogResult = true;
        }


    }
}
