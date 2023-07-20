using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Reflection.Emit;
//using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace logic
{
    
    public abstract class TreeNode
    {
        
        public abstract bool Value { get; set; }
        public Token Token { get; set;  }
        public TreeNode(Token tk)
        {
            Token = tk;
        }
        
    }
    public class BranchNode : TreeNode 
    {
        private delegate bool BasicOperationDelegate();
        private BasicOperationDelegate[] _basicOperationTable;
        public override bool Value
        {   
            get
            {
                //delegate table
                return _basicOperationTable[Token.Type - TokenType.Or].Invoke();
            }
            set { Value = value; }
        }
        public BranchNode(Token tk) : base(tk)
        { 
            _basicOperationTable=new BasicOperationDelegate[3]{
            ()=>{ return operands[0].Value || operands[1].Value; },
            ()=>{ return operands[0].Value && operands[1].Value;},
            ()=>{ return !operands[0].Value;}
            };
        }
        public List<TreeNode> operands=new List<TreeNode>(2);
        //public override bool Value { get { return false; } set { } }
    }
    public class LeafNode : TreeNode
    {
        public override bool Value { get; set; }
        public LeafNode(Token tk) : base(tk) { }
    }
    //chet

    public static class LogicInterpreter
    {

        
        static List<Token> tokens = new List<Token>();
        
        static Hashtable<Tuple<int[,], TreeNode>> functions = new Hashtable<Tuple<int[,], TreeNode>>();
        public static Tuple<int[,], TreeNode> ReadFromDictionary(string funcName)
        {
            Tuple<int[,], TreeNode> data;
            if(functions.TryGetValue(funcName, out data)) return data;
            return null;
        }
        public static string[] DictionaryKeys()
        {
            return functions.Keys;
        }
        static void Tokenize(string input, out bool success, ref string funcName, ref int[,] argidx)
        {
            tokens = new List<Token>();
            success = true;
            bool openq;
            //int[,] argidx;
            string args = "";
            int brdif = 0, argdif = 0;
            funcName = "";
            int i = 0;
            //dokato nameri imeto
            if (input.Length<1) { success = false; return; }
            while (input[i] == ' ') { if (i++ == input.Length) { success = false; return; } }  
            //vzima imeto ako e legalno
            bool inFuncName = true;
            while (input[i] != '(')
            {
                if (input[i] >= '0' && input[i] <= '9' || input[i] >= 'A' && input[i] <= 'Z' || input[i] >= 'a' && input[i] <= 'z')
                { if (!inFuncName) { success = false; return; } funcName += input[i]; }
                else if (input[i] == ' ') inFuncName = false;
                else { success = false; return; }
                
                if (++i==input.Length) { success = false; return; }
            }
            if (++i == input.Length||functions.ContainsKey(funcName)) { success = false; return; }
            //arg na f-iqta
            var cflag = false;
            while (input[i] != ')')
            {
                cflag = false;
                if (input[i] >= 'A' && input[i] <= 'Z' || input[i] >= 'a' && input[i] <= 'z')
                {

                    foreach (var ch in args) if (ch == input[i]) { success = false; return; }
                    argdif++;
                    args += input[i];
                    if (++i == input.Length) { success = false; return; }
                    while (input[i] != ',')
                    {
                        if (i == input.Length) { success = false; return; }
                        if (input[i] == ')') break;
                        if (input[i] != ' ') { success = false; return; }
                        if (++i == input.Length) { success = false; return; }
                    }
                    
                    //bruh
                    if (input[i] == ')') break;cflag = true;
                }
                else if (input[i] != ' ') { success = false; return; }
                if (++i == input.Length) { success = false; return; }
            }
            if (++i == input.Length||cflag) { success = false; return; }
            while (input[i] != ':')
            {

                if (input[i] != ' ') { success = false; return; }
                if (++i == input.Length) { success = false; return; }
            }
            if (++i == input.Length) { success = false; return; }
            while (input[i] != '\"')
            {
                
                if (input[i] != ' ') { success = false; return; }
                if (++i == input.Length) { success = false; return; }
            }
            //v tqloto na f-iqta
            openq = true;
            bool operatorFlag = true;
            string innerFuncName = "";
            argidx = new int[2, argdif];
            int bodyargs = 0;
            if (++i == input.Length) { success = false; return; }
            //t za da moje da pochne s bukva
            while (i < input.Length)
            {
                switch (input[i])
                {
                    case '\"': { if (operatorFlag == true || tokens.Count < 3) { success = false; return; } openq = false; } break;
                    case ' ': break;
                    case '(': { if (!operatorFlag) { success = false; return; } brdif++; tokens.Add(new Token(TokenType.OpenBracket)); } break;
                    case ')': { if (operatorFlag) { success = false; return; } brdif--; tokens.Add(new Token(TokenType.CloseBracket)); } break;
                    case '!': { if (!operatorFlag) { success = false; return; } operatorFlag = true; tokens.Add(new Token(TokenType.Not)); } break;
                    case '&': { if (operatorFlag) { success = false; return; } operatorFlag = true; tokens.Add(new Token(TokenType.And)); } break;
                    case '|': { if (operatorFlag) { success = false; return; } operatorFlag = true; tokens.Add(new Token(TokenType.Or)); } break;

                    default: {
                            if (input[i] >= '0' && input[i] <= '9' || input[i] >= 'A' && input[i] <= 'Z' || input[i] >= 'a' && input[i] <= 'z')
                            {
                                innerFuncName = "";
                                if (!operatorFlag) { success = false; return; } operatorFlag = false;
                                int fi = i;
                                while (input[fi] >= '0' && input[fi] <= '9' || input[fi] >= 'A' && input[fi] <= 'Z' || input[fi] >= 'a' && input[fi] <= 'z')
                                {
                                    innerFuncName += input[fi];
                                    if (++fi == input.Length) { success = false; return; }
                                }

                                if (functions.ContainsKey(innerFuncName))

                                {
                                    i = fi;
                                    tokens.Add(new Token(innerFuncName));


                                    Tuple<int[,], TreeNode> pair;
                                    if (!functions.TryGetValue(innerFuncName, out pair)) { success = false; return; }
                                    var innerFuncArgs = pair.Item1;
                                    var fbodyargs = bodyargs;

                                    //arg na vatreshnat f-iq
                                    while (i < input.Length && input[i] != '(') { if (input[i] != ' ') { success = false; return; } i++; }
                                    while (input[i] != ')')
                                    {
                                        cflag = false;
                                        if (++i == input.Length) { success = false; return; }
                                        if (input[i] >= 'A' && input[i] <= 'Z' || input[i] >= 'a' && input[i] <= 'z')
                                        {

                                            int k = 0;
                                            //vzima idx i ime
                                            while (k < args.Length)
                                            {
                                                if (input[i] == args[k])
                                                {
                                                    //MAI NE SOCHI KAM PRAVILNITE ARG PO RED    ddz
                                                    if (bodyargs-fbodyargs == innerFuncArgs.GetLength(1)) { success = false; return; }
                                                    argdif--; argidx[0, k] = fbodyargs+innerFuncArgs[0, bodyargs-fbodyargs]; argidx[1, k] = args[k]; bodyargs++;
                                                    if (++i == input.Length) { success = false; return; }
                                                    while (input[i] != ',')
                                                    {

                                                        if (input[i] == ')') break;
                                                        if (input[i] != ' ') { success = false; return; }
                                                        if (++i == input.Length) { success = false; return; }
                                                    }
                                                    break;
                                                }
                                                k++;
                                            }
                                            if (input[i] == ')') break; cflag = true;
                                            //???moebi
                                            //i++;
                                            //success = false; return;

                                        }
                                        else if (input[i] != ' ') { success = false; return; }
                                        //i++;
                                    }
                                    
                                    //razmestva teq v t. na vatreshnata f-iq
                                    if (bodyargs - fbodyargs != innerFuncArgs.GetLength(1)) { success = false; return; }
                                    //var newIdx = new int[innerFuncArgs.GetLength(1)];
                                    //for (int k = 0; k < innerFuncArgs.GetLength(1); k++)
                                    //{
                                    //    newIdx[k] = argidx[0, fbodyargs + innerFuncArgs[0, k]];
                                    //}
                                    //for (int k = 0; k < innerFuncArgs.GetLength(1); k++)
                                    //{
                                    //    argidx[0, fbodyargs + k] = newIdx[k];
                                    //    argidx[1, fbodyargs + k] = args[newIdx[k]];
                                    //}
                                }

                                else
                                {
                                    if (innerFuncName.Length > 1) { success = false; return; }
                                    innerFuncName = "";
                                    //tarsi arg
                                    int j = 0;
                                    //vzima idx i ime
                                    success = false;
                                    while (j < args.Length)
                                    {
                                        if (input[i] == args[j]) { for(int k=0; k<argidx.GetLength(1); k++) if (input[i]==argidx[1,k]) success = false; argdif--; argidx[0, j] = bodyargs; argidx[1, j] = args[j]; bodyargs++; success = true; tokens.Add(new Token(TokenType.Operand)); break; }
                                        j++;
                                    }
                                    if (success) break;
                                    return;
                                }
                            }
                            else { success = false; return; }
                        }break;
                }
                i++;
            }
            if (brdif != 0 || argdif != 0 || openq) { success = false; return; }
            while (i < input.Length) { if (input[i] != ' ') { success = false; return; } i++; }
        }
        public static TreeNode BuildTree()
        {

            var treeStack = new Stack<TreeNode>();
            var root = new BranchNode(null);
            //var current = new BranchNode(null);
            treeStack.Push(root);
            var current = root;
            if (tokens.Count == 0) return null;

            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].Type)
                {
                    case TokenType.OpenBracket: { current.operands.Add(new BranchNode(tokens[i])); treeStack.Push(current); current = (BranchNode)current.operands[current.operands.Count - 1]; } break;
                    case TokenType.CloseBracket: { while (current.Token.Type>TokenType.OpenBracket) current = (BranchNode)treeStack.Pop(); var temp = (BranchNode)treeStack.Pop();
                            temp.operands[temp.operands.Count - 1] = current.operands[current.operands.Count - 1];
                            current = temp;
                        } break;
                    case TokenType.And:
                    case TokenType.Or:
                        {
                            if (current.Token == null) { current.Token = tokens[i]; }
                            else
                            {
                                do
                                {

                                    if (current.Token.Type <= tokens[i].Type)
                                    {
                                        var temp = new BranchNode(tokens[i]);
                                        temp.operands.Add(current.operands[current.operands.Count - 1]);
                                        current.operands[current.operands.Count - 1] = temp;
                                        treeStack.Push(current);
                                        current = temp;
                                        break;
                                    }
                                    current = treeStack.Count > 0?(BranchNode)treeStack.Pop():current;
                                }
                                while (treeStack.Count > 0);
                                if (treeStack.Count == 0)
                                {
                                    root = new BranchNode(tokens[i]);
                                    root.operands.Add(current);
                                    current = root;
                                }
                            }

                        }
                        break;

                    case TokenType.Not:
                        {
                            if (current.Token == null) { current.Token = tokens[i]; }
                            else
                            {
                                current.operands.Add(new BranchNode(tokens[i]));
                                treeStack.Push(current);
                                current = (BranchNode)current.operands[current.operands.Count - 1];
                            }
                        }
                        break;
                    case TokenType.Function:
                        {
                            Tuple<int[,], TreeNode> pair;
                            if (!functions.TryGetValue(tokens[i].Name, out pair)) return null;
                            current.operands.Add(pair.Item2);
                        } break;
                    default:
                        {
                            
                            current.operands.Add(new LeafNode(tokens[i]));
                        }
                        break;
                }
            }
            while (root.Token == null) root = (BranchNode)root.operands[0];
            return root;
        }
        static void PutInDicTionary(string name, int[,] args, TreeNode tree)
        {
            Tuple<int[,], TreeNode> argBodyPair = new Tuple<int[,], TreeNode>(args, tree);
            functions.Add(name, argBodyPair);
        }
        //wtf tova chete i st-sti na parametrite zatova go nqma gore 
        static void ReadFunctionName(string input, ref string name, ref bool success, ref bool[] values)
        {
            //success;
            string funcName = "";
            int i = 0;
            //dokato nameri imeto
            if (input.Length == 0) { success = false; return; }
            while (input[i] == ' ') { if (++i == input.Length) { success = false; return; } }
            //vzima imeto ako e legalno
            bool inFuncName = true;
            while (input[i] != '(')
            {
                if (input[i] >= '0' && input[i] <= '9' || input[i] >= 'A' && input[i] <= 'Z' || input[i] >= 'a' && input[i] <= 'z')
                { if (!inFuncName) { success = false; return; } funcName += input[i]; }
                else if (input[i] == ' ') inFuncName = false;
                else { success = false; return; }

                if (++i == input.Length) { success = false; return; }
            }
            Tuple<int[,], TreeNode> pair;
            if (++i == input.Length || !functions.TryGetValue(funcName, out pair)) { success = false; return; }
            name = funcName;
            //substring
            string s = "";
            for (int k = i; k < input.Length; k++) s += input[k];
            //arg na f-iqta
            
            values = new bool[pair.Item1.GetLength(1)];
            
            var cflag = false;
            int j = 0;
            while (input[i] != ')'&&j<values.Length)
            {
                cflag = false;
                if (input[i] =='0'||input[i]=='1')
                {
                    if (j == input.Length) { success = false; return; }
                    values[j++] = input[i] == '1';
                    

                    if (++i == input.Length) { success = false; return; }
                    while (input[i] != ',')
                    {
                        if (i == input.Length) { success = false; return; }
                        if (input[i] == ')') break;
                        if (input[i] != ' ') { success = false; return; }
                        if (++i == input.Length) { success = false; return; }
                    }

                    //bruh
                    if (input[i] == ')') break; cflag = true;
                }
                else if (input[i] != ' ') { success = false; return; }
                if (++i == input.Length) { success = false; return; }
            }
            if (cflag||j!=values.Length) { success = false; return; }
        }
        //bachka samo s konzolen iface :(
        static void InputTable(string input)
        {

            if (input == "") return; 
            bool success = true;

            List<bool> values = new List<bool>();
            for (int i = 0; i < input.Length; i++)
            {
                bool cflag = false;
                if (input[i] == '0' || input[i] == '1')
                {

                    values.Add(input[i] == '1');

                    if (++i == input.Length) { success = false; return; }
                    while (input[i] != ',' && input[i] != ':')
                    {
                        
                        if (input[i] != ' ') { success = false; return; }
                        if (++i == input.Length) { success = false; return; }
                    }

                    //bruh
                    if (input[i] == ':')
                    {

                        if (++i == input.Length) { success = false; return; }
                        while (input[i] == ' ')
                        {
                            if (++i == input.Length) { success = false; return; }
                        } if (input[i] == '0' || input[i] == '1') values.Add(input[i] == '1');
                        else { success = false; return; }
                        break;
                    }
                cflag = true;
                }
                else if (input[i] != ' ') { success = false; return; }
                
            }
            var lineCollection = new List<bool[]>();
            var resultCollection = new List<bool>();
            resultCollection.Add(values[values.Count-1]);
            values.RemoveAt(values.Count-1);
            lineCollection.Add(values.ToArray());



            string line = Console.ReadLine();
            while (line!="")
            {
                int i = 0;
                while (line[i]==' ') if (++i == input.Length) { success = false; return; }
                var val = new bool[values.Count];
                bool res; int j = 0;
                while ( i < line.Length)
                {
                    bool cflag = false;

                    if (line[i] == '0' || line[i] == '1')
                    {

                        if (j == val.Length) { success = false; return; }
                        val[j++]=line[i] == '1';

                        if (++i == line.Length) { success = false; return; }
                        while (line[i] != ',' && line[i] != ':')
                        {
                            //if (i == line.Length) { success = false; return; }
                            //if (line[i] == ')') break;
                            if (line[i] != ' ') { success = false; return; }
                            if (++i == line.Length) { success = false; return; }
                        }

                        //bruh
                        if (line[i] == ':')
                        {

                            if (++i == line.Length||cflag) { success = false; return; }
                            while (line[i] == ' ')
                            {
                                if (++i == line.Length) { success = false; return; }
                            }
                            if (line[i] == '0' || line[i] == '1') { res = line[i] == '1'; resultCollection.Add(res); }
                            else { success = false; return; }
                            break;
                        }
                        cflag = true; 
                    }
                    else if (line[i] != ' ') { success = false; return; } if (++i == line.Length) { success = false; return; }
                }
                lineCollection.Add(val);
                line = Console.ReadLine();
                
            }
            bool[][] finalvalues = lineCollection.ToArray();
            bool[] finalresults = resultCollection.ToArray();
            TreeNode found=new BranchNode(null);
            FindFunction(finalvalues, finalresults, ref found);
            if (found.Token == null) { Console.WriteLine("None found");  return; }
            char[] onames = new char[values.Count];
            string s = "foundfunction (";
            for (int c = 0; c < values.Count; c++)
            { onames[c] = (char)('a' + c % 26); s += onames[c];s+= c==values.Count-1?"): ":", "; }
            
            int idx = 0;
            DisplayFunctionBody(found, onames, ref idx, ref s);
            Console.WriteLine(s);

        }
        public static bool ReadTable(string input, out bool[][]values, out bool[]results)
        {
            values = null;
            results = null;
            string line = "";
            int i;
            for (i=0; i < input.Length && input[i]!='\n'; i++) line += input[i];
            List<bool> rowvalues;
            if (!TryReadTableRow(line, out rowvalues)) return false;
            int n = rowvalues.Count;
            
            //
            List<bool[]> valuesCollection = new List<bool[]>();
            List<bool> resultsCollection = new List<bool>();
            resultsCollection.Add(rowvalues[n-1]);
            rowvalues.RemoveAt(n - 1);
            valuesCollection.Add(rowvalues.ToArray());

            line = "";
            for (i = ++i; i < input.Length && input[i] != '\n'; i++) line += input[i];
            valuesCollection = new List<bool[]>();
            resultsCollection = new List<bool>();
            while (line != "")
            {
                if (!TryReadTableRow(line, out rowvalues)||rowvalues.Count!=n) return false;

                
                resultsCollection.Add(rowvalues[n - 1]);
                rowvalues.RemoveAt(n - 1);
                valuesCollection.Add(rowvalues.ToArray());
                line = "";
                for (i = ++i; i<input.Length&&input[i] != '\n'; i++) line += input[i];
                
            }
            values = valuesCollection.ToArray();
            results = resultsCollection.ToArray();
            return true;

            
        }
        //trq se zameni i  gore v input
        static bool TryReadTableRow(string input, out List<bool> values)
        {
            values = new List<bool>();
            if (input == "") return false;

            for (int i = 0; i < input.Length; i++)
            {

                if (input[i] == '0' || input[i] == '1')
                {

                    values.Add(input[i] == '1');

                    if (++i == input.Length) return false;
                    while (input[i] != ',' && input[i] != ':')
                        if (input[i] != ' ' || ++i == input.Length) return false;


                    //bruh
                    if (input[i] == ':')
                    {

                        if (++i == input.Length) return false;
                        while (input[i] == ' ')
                        {
                            if (++i == input.Length) return false;
                        }
                        if (input[i] == '0' || input[i] == '1') values.Add(input[i] == '1');
                        else return false;
                        break;
                    }

                }
                else if (input[i] != ' ') return false;

            }
            return true;
        }
        static void ReadCommand()
        {
            string input="";
            bool process=false;
            while (true)
            {
                bool success = true;
                
                if (!process) input = Console.ReadLine();
                string curCommand = "";
                
                int i;
                for (i = 0; i < input.Length; i++)
                {
                    if (curCommand == "") { if (input[i] == ' ') continue; }
                    else if (input[i] == ' ') break;
                    curCommand += input[i];
                }
                string s="";
                for (int j = i; j < input.Length; j++) s += input[j];
                process = true;
                switch (curCommand)
                {
                    case "DEFINE": { Define(s); } break;
                    case "SOLVE": { SolveFunction(s); } break;
                    case "ALL": { GetTable(s); } break;
                    case "FIND": { InputTable(s); } break;
                    default: { Console.WriteLine("Incorrect command!"); } break;

                }
                process = false;
                //success = false;
            }
        }
        public static string Define(string input)
        {
            bool success;
            string funcName="";
            int[,] argidx = null;
            //input = Console.ReadLine();
            Tokenize(input, out success, ref funcName, ref argidx);
            if (!success) return null;
            
            var tree=BuildTree();
            PutInDicTionary(funcName, argidx, tree);
            AppendToFile(funcName, argidx, tree);
            return funcName;
        }
        public static void ReadFromFile()
        {
            if (!File.Exists("functions.txt")) { File.Create("functions.txt"); return; }
            using (StreamReader sr = new StreamReader("functions.txt"))
            {
                bool success=true;
                string line = sr.ReadLine();
                string funcName = "";
                int[,] argidx = null;
                TreeNode tree;
                while (line != null&&success)
                {
                    
                    Tokenize(line, out success, ref funcName, ref argidx);
                    if (!success) { if (line == "") { success = true; line = sr.ReadLine(); continue; } foreach (var ch in line) { if (ch != ' ') { Console.WriteLine("Incorrect data!"); break; } } }
                    else { tree = BuildTree(); PutInDicTionary(funcName, argidx, tree); }
                    line = sr.ReadLine();
                }
            
            }
            
        }
        static void GetWidth(TreeNode root, ref int ind)
        {
            if (root.Token.Type == TokenType.Operand) ind++;
            else for (int i = 0; i < ((BranchNode)root).operands.Count; i++)
                GetWidth(((BranchNode)root).operands[i], ref ind);
        }
        static void SetLeafNodeValues(TreeNode root, bool[] values, ref int ind)
        {
            if (root.Token.Type == TokenType.Operand) { root.Value = values[ind]; ind++; }
            else for (int i = 0; i< ((BranchNode)root).operands.Count&&ind<values.Length; i++)
                SetLeafNodeValues(((BranchNode)root).operands[i], values, ref ind);
        }
        public static void DisplayFunctionBody(TreeNode root, char[] names, ref int ind, ref string output)
        {
            if (root.Token.Type == TokenType.Operand) { output += names[ind].ToString(); ind++; }
            //string open ="", close = "";
            else for (int i = 0; i < ((BranchNode)root).operands.Count && ind < names.Length; i++)
                {
                    bool brackets = ((BranchNode)root).operands[i].Token.Type < root.Token.Type;
                    if (root.Token.Type == TokenType.Not) output += "!";
                    output += brackets ? "(" : "";
                    DisplayFunctionBody(((BranchNode)root).operands[i], names, ref ind, ref output);
                    output += brackets ? ")" : "";
                    if (i==0)
                    {
                        if (root.Token.Type == TokenType.And) output += "&";
                        else if (root.Token.Type == TokenType.Or) output += "|";
                    }
                }
            //else
            //{
            //    bool brackets = root.operands[0]
            //if (root.)
            //        if (((BranchNode)root).operands[i].Token.Type < root.Token.Type) { open = "("; close = ")"; }

                    //}

        }
        public static string SolveFunction(string input /*string name, bool[] values, ref bool result,*/)
        {
            bool success = true;
            bool result;
            bool[] values=new bool[0];
            string name = "";
            ReadFunctionName(input, ref name, ref success, ref values);
            if (!success)  return null; 
            
            //success = true;
            
            Tuple<int[,], TreeNode> pair;
            if (!functions.TryGetValue(name, out pair)) return null;
            
            //zadava posledovatelnostta za tqloto na f-iqta
            var newValues = new bool[values.Length];
            for (int i = 0; i < pair.Item1.GetLength(1); i++)
            {
                newValues[pair.Item1[0, i]] = values[i];
            }
            int ind = 0;
            SetLeafNodeValues(pair.Item2, newValues, ref ind);
            return name;

            //result = pair.Item2.Value;

            //string message = "Result: "; message += result ? "1" : "0";
            //Console.WriteLine(message);
        }
        //obrashta reda kam tozi v tqloto spored dadenite indeksi 
        public static char[] ArrangeOperands(int[,] args)
        {
            var newNames = new char[args.GetLength(1)];
            for (int i = 0; i < args.GetLength(1); i++)
            {
                newNames[args[0, i]] = (char)args[1, i];
            }
            return newNames;
        }
        static void AppendToFile(string name, int[,] args, TreeNode tree)
        {
            //Tuple<int[,], TreeNode> pair;
            //if (!functions.TryGetValue(name, out pair)) return;
            //zadava posledovatelnostta za tqloto na f-iqta
            var newNames = ArrangeOperands(args);
            int ind = 0;
            string s = name + "(";
            for (int i = 0; i < args.GetLength(1); i++)
            {
                s += (char)args[1, i];
                s+= (i + 1) == args.GetLength(1) ? "): \"" : ", ";
            }
            //string body = "";
            DisplayFunctionBody(tree, newNames, ref ind, ref s);
            s += "\"";
            using (StreamWriter sw = new StreamWriter("functions.txt", true))
                sw.WriteLine(s);

        }
        
        
        public static string GetTable(string input)
        {
            string funcName = "";
            int i = 0;
            if (input.Length == 0) return null; 
            while (input[i] == ' ') { if (++i == input.Length) return null; }
            
            while (i < input.Length && input[i] != ' ')
            {
                if (input[i] >= '0' && input[i] <= '9' || input[i] >= 'A' && input[i] <= 'Z' || input[i] >= 'a' && input[i] <= 'z')
                    funcName += input[i++];
                else return null;

            }
            while (i < input.Length) { if (input[++i] != ' ') return null; } 
            Tuple<int[,], TreeNode> pair;
            if (!functions.TryGetValue(funcName, out pair)) return null; 

            var root = pair.Item2;
           

            bool[] curvalues = new bool[pair.Item1.GetLength(1)];

            bool[] results = new bool[(int)Math.Pow(2, curvalues.Length)];
            int ind = 0;
            SetLeafNodeValues(root, curvalues, ref ind);

            
            string s = "";
            for (int j = 0; j < pair.Item1.GetLength(1); j++) {s += j == 0 ? "" : " | "; s += (char)pair.Item1[1, j]; }
            //Console.WriteLine(s);
            

            int n = 0;

            for (int j = 0; j < results.Length; j++)
            {

                ind = 0;
                //variaciite
                s += "\n";
                for (int l = 0; l<pair.Item1.GetLength(1);  l++) { s += l == 0 ? "" : " | "; s += curvalues[l] ? "1" : "0"; }

                var newValues = new bool[curvalues.Length];
                for (int v = 0; v < pair.Item1.GetLength(1); v++)
                {
                    newValues[pair.Item1[0, v]] = curvalues[v];
                }

                SetLeafNodeValues(root, newValues, ref ind);
                results[j] = root.Value;
                
                int k;
                for (k = curvalues.Length-1; k >= 0 && curvalues[k]; k--) curvalues[k] = false;
                if (k>=0) curvalues[k] = true;
                               
                s += " : ";  s += results[j] ? "1" : "0";
                //Console.WriteLine(s);
            }
            return s;

        }
        //vreme e za smart

       //sh sa gramna
        static void GenerateTree(int operandcount,  Stack<TreeNode> stin,  Stack<TreeNode> stout, bool[][] values, bool[] results, ref TreeNode found, ref bool success)
        {
            
            if (success) return;
            if (stin == null || stout == null) return;
            if (stin.Count == 0 && stout.Count == 1) return;
            if (stin.Count == 0 && stout.Count ==0)
            {

                for (int i = 0; i < operandcount; i++)
                    stin.Push(new LeafNode(new Token(TokenType.Operand)));
            }
            //if (stin.Count == 0&&stout.Count>1) { var temp = stin; stin = stout; stout = temp; }
            //da si hodi
            
            for (var type = TokenType.Or; type < TokenType.Operand; type++)
            {
                
                if (stin.Count == 0 && stout.Count > 1) { var temp = stin; stin = stout; stout = temp; }
                //da si hodi
                Stack<TreeNode> cstin=new Stack<TreeNode>(stin.ToArray());
                    cstin = new Stack<TreeNode>(cstin.ToArray());

                    Stack<TreeNode> cstout=new Stack<TreeNode>(stout.ToArray());
                    cstout = new Stack<TreeNode>(cstout.ToArray());

                //if (stin.Count == 0 && stout.Count == 0) return;
                if (type < TokenType.Not)
                {
                    if (stout.Count > 0)
                    {
                        var cstin1 = new Stack<TreeNode>((new Stack<TreeNode>(stin.ToArray())).ToArray());
                        var cstout1 = new Stack<TreeNode>((new Stack<TreeNode>(stout.ToArray())).ToArray());
                        cstin1.Push(cstout1.Pop());

                        GenerateTree(operandcount, cstin1, cstout1, values, results, ref found, ref success);
                    }
                    
                    
                    var tree = new BranchNode(new Token(type));
                    tree.operands.Add(stin.Pop());
                    if (stin.Count == 0) tree.operands.Add(stout.Pop());
                    else tree.operands.Add(stin.Pop());
                    stout.Push(tree);

                }
                else if (type == TokenType.Not)
                {
                    while (stin.Peek().Token.Type == type) { if (stin.Count == 1) return; stout.Push(stin.Pop()); }
                        var tree = new BranchNode(new Token(type));
                        
                        tree.operands.Add(stin.Pop());
                        stout.Push(tree);
                        
                }
                //checkva
                if (success) return;
                if (stin.Count == 0 && stout.Count == 1)
                {
                    success = true;
                    var generated = stout.Pop();
                    

                    for (int j = 0; j < results.Length; j++)
                    {
                        int ind = 0;
                        SetLeafNodeValues(generated, values[j], ref ind);
                        if (generated.Value != results[j]) { success = false; break; }
                    }
                    if (success) { found = generated; return; }
                    
                    stin = cstin; stout = cstout;
                    

                    continue;
                    
                }
                else
                {
                    if (cstin.Count == operandcount&&type!=TokenType.Not) {
                        //stin = cstin; stout = cstout;
                        var temp1 = cstin; var temp2 = cstout;
                        cstin = new Stack<TreeNode>(stin.ToArray());
                        cstin = new Stack<TreeNode>(cstin.ToArray());
                        cstout = new Stack<TreeNode>(stout.ToArray());
                        cstout = new Stack<TreeNode>(cstout.ToArray());
                        GenerateTree(operandcount, cstin, cstout, values, results, ref found, ref success);
                        stin = temp1; stout = temp2;
                        continue; }
                    cstin = new Stack<TreeNode>(stin.ToArray());
                    cstin = new Stack<TreeNode>(cstin.ToArray());
                    cstout = new Stack<TreeNode>(stout.ToArray());
                    cstout = new Stack<TreeNode>(cstout.ToArray());
                    GenerateTree(operandcount, cstin, cstout, values, results, ref found, ref success);
                }
                    
            }
            

        }
        public static void FindFunction(bool[][] values, bool[] results, ref TreeNode found)
        {
            TreeNode generated = new BranchNode(null);
            BranchNode empty = new BranchNode(null);
            int n = values[0].Length;
            //TreeVariation(ref generated, /*ref empty, generated,*/ ref n);
            bool success = false;
            Stack<TreeNode> stin = new Stack<TreeNode>();
            Stack<TreeNode> stout = new Stack<TreeNode>();
            GenerateTree(n,  stin,  stout, values, results, ref found, ref success);

            if (!success) found = null; 
            
        }

        //gone gold!!! (kinda)💪
        

        
    }
}
