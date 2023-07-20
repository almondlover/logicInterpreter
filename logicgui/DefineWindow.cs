using logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace logicgui
{
    internal class DefineWindow:CommandWindow
    {
        
        public override void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string input = FunctionNameText.Text + "(" + FunctionArgumentsText.Text + "):" + "\"" + FunctionBodyText.Text + "\"";

            funcName = LogicInterpreter.Define(input);
            if (funcName == null) { MessageBox.Show("Invalid data"); return; }

            DialogResult = true;
        }
    }
}
