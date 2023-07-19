using logic;
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

namespace logicgui
{
    /// <summary>
    /// Interaction logic for TableWindow.xaml
    /// </summary>
    public partial class TableWindow : Window
    {
        public TreeNode found;
        public char[] operands;
        public string funcName="";
        public TableWindow()
        {
            InitializeComponent();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            var input = TableTextBox.Text;

            bool[][] values;
            bool[] results;
            if (!LogicInterpreter.ReadTable(input, out values, out results)) { MessageBox.Show("Invalid data"); return; }

            found = new BranchNode(null);
            LogicInterpreter.FindFunction(values, results, ref found);

            if (found==null) { MessageBox.Show("None found"); return; }

            var cmdw = new CommandWindow();
            cmdw.FunctionArgumentsText.IsReadOnly = true;
            cmdw.FunctionBodyText.IsReadOnly = true;

            operands = new char[values[0].Length];

            operands[0] = 'a';
            cmdw.FunctionArgumentsText.Text = "a";

            int i;
            for (i = 1; i < values[0].Length; i++)
            {
                operands[i] = (char)('a' + i % 26);
                cmdw.FunctionArgumentsText.Text += ", "+operands[i];
            }

            i = 0;
            string s="";
            LogicInterpreter.DisplayFunctionBody(found, operands, ref i, ref s);
            cmdw.FunctionBodyText.Text = s;

            cmdw.ShowDialog();
            if ((bool)cmdw.DialogResult) funcName = cmdw.funcName;
            else funcName = "";
        }
    }
}
