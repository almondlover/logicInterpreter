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
using System.Windows.Navigation;
using System.Windows.Shapes;
using logic;

namespace logicgui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TreeNode curTree = null;
        char[] operandSymbols=null;
        bool calculated = false;
        public MainWindow()
        {
            InitializeComponent();
            LogicInterpreter.BuildTree();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogicInterpreter.ReadFromFile();
            FunctionComboBox.ItemsSource = LogicInterpreter.DictionaryKeys();
        }

        private void DefineButton_Click(object sender, RoutedEventArgs e)
        {
            var cmdw = new CommandWindow();
            cmdw.ShowDialog();
            calculated = false;

            SetCurrent(cmdw.funcName);
            

            
        }
        private void SetCurrent(string name)
        {
            
            if (name == null) return;
            
            TreeLabel.Content = name;

            //tuka izkarva darvoto; po-dobre napravo prez dr prozorec da prati obekt s nujnite danni bruh
            var pair = LogicInterpreter.ReadFromDictionary(name);
            if (pair == null) return;

            curTree = pair.Item2;

            operandSymbols = LogicInterpreter.ArrangeOperands(pair.Item1);
            
            DrawArea.Children.Clear();
            var treeGraphic = new TreeGraphic(curTree, DrawArea, operandSymbols);
            treeGraphic.Calculated = calculated;

            treeGraphic.DrawTree();
        }
        
        private void FunctionComboBox_DropDownClosed(object sender, EventArgs e)
        {
            SetCurrent(((ComboBox)sender).Text);
            calculated = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (curTree == null || operandSymbols == null) return;
            
            DrawArea.Children.Clear();
            var treeGraphic = new TreeGraphic(curTree, DrawArea, operandSymbols);
            treeGraphic.Calculated = calculated;

            treeGraphic.DrawTree();
        }


        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            if (curTree == null || operandSymbols == null) return;

            var cmdw = new SolveWindow();
            cmdw.FunctionNameText.Text = FunctionComboBox.Text;

            int i = 0;
            string bodytxt = "";
            LogicInterpreter.DisplayFunctionBody(curTree, operandSymbols, ref i, ref bodytxt);
            cmdw.FunctionBodyText.Text = bodytxt;

            //argumentite
            var args=LogicInterpreter.ReadFromDictionary(FunctionComboBox.Text)?.Item1;
            cmdw.ArgsText = ((char)args[1,0]).ToString();
            for (int j = 1; j < i; j++) cmdw.ArgsText += $", {(char)args[1,j]}";
            cmdw.FunctionArgumentsText.Text = cmdw.ArgsText;

            cmdw.ShowDialog();

            if (cmdw.funcName != null) calculated = true;
            SetCurrent(cmdw.funcName);
        }

        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            if (curTree == null || operandSymbols == null) return;
            var tbw = new TableWindow();

            tbw.NameTextBox.Text = FunctionComboBox.Text;
            tbw.TableTextBox.Text = LogicInterpreter.GetTable(FunctionComboBox.Text);


            tbw.ShowDialog();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            var tbw = new TableWindow();


            tbw.TableTextBox.IsReadOnly = false;
            tbw.FindButton.Visibility = Visibility.Visible;
            
            tbw.ShowDialog();

            

            if (tbw.found == null || tbw.operands == null) return;
            

            SetCurrent(tbw.funcName);

            //izlishno se povtarq
            if (tbw.funcName != "") return;
            curTree = tbw.found;
            operandSymbols = tbw.operands;

            DrawArea.Children.Clear();
            var treeGraphic = new TreeGraphic(curTree, DrawArea, operandSymbols);
            treeGraphic.Calculated = calculated;

            treeGraphic.DrawTree();
        }
    }
}
