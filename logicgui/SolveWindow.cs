using logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace logicgui
{
    internal class SolveWindow:CommandWindow
    {
        //Label placeholder;
        public string ArgsText { get; set; }
        public SolveWindow():base()
        {
            FunctionNameText.IsReadOnly = true;
            FunctionBodyText.IsReadOnly = true;
            FunctionArgumentsText.GotFocus += FunctionArgumentsText_GotFocus;
            FunctionArgumentsText.LostFocus += FunctionArgumentsText_LostFocus;
            
        }
        public override void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string input = FunctionNameText.Text + "(" + FunctionArgumentsText.Text + ")";

            funcName=LogicInterpreter.SolveFunction(input);
            if (funcName == null) { MessageBox.Show("Invalid data"); return; }

            DialogResult = true;
        }
        //trq se opravi yerarhiqta
        
        public void FunctionArgumentsText_GotFocus(object sender, RoutedEventArgs e)
        {
            var tbox = ((TextBox)sender);
            if (tbox.Text == ArgsText) tbox.Text = "";
        }
        public void FunctionArgumentsText_LostFocus(object sender, RoutedEventArgs e)
        {
            var tbox = ((TextBox)sender);
            if (tbox.Text == "") tbox.Text = ArgsText;
        }
    }
}
