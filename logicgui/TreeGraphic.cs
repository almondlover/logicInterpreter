using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Drawing;
using logic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xaml.Schema;
using System.Windows;
using System.ComponentModel.Design.Serialization;
using System.Security.Cryptography;

namespace logicgui
{
	
	internal class TreeGraphic
	{
		//st-sti za razmerite
		private const int nodeSize= 50;
		
		public TreeNode tree;

		private char[] _operands;

		public Canvas drawArea;
		public bool Calculated { get; set; } 
		public TreeGraphic(TreeNode tree, Canvas canvas, char[] operands)
		{
			this.tree = tree;
			this._operands = operands;
			this.drawArea = canvas;
			Calculated = false;
		}

		private bool SpaceCheck(List<TreeNode> nodes, TreeNode current)
		{
			if (nodes.Count == 0) return false;
			var next = new List<TreeNode>();

			
			foreach (var node in nodes)
			{

				if (node.Token.Type == TokenType.Operand) continue;
				var children = ((BranchNode)node).operands;
				if (children.Contains(current))
				{
					//izlishno ako e samo 1 op 
					int neighbouridx = nodes.IndexOf(node) - 1 + 2 * children.IndexOf(current);
					return neighbouridx > -1 && neighbouridx < nodes.Count && (nodes[neighbouridx].Token.Type==TokenType.Operand||((BranchNode)nodes[neighbouridx]).operands.Count < 2);
				}


                next.AddRange(children);

			}
			return SpaceCheck(next, current);
		}
		private string[] _operatorNames = { "|", "&", "!" };
		public void DrawNode(TreeNode root, Point location, double operandSpace, ref int operandidx)
		{
            //risuva vazlite s txt-mo-dobre da se prenapishe s border 
			var visNode = new Ellipse();
            visNode.Width = visNode.Height = nodeSize;
            visNode.StrokeThickness = 2;
            visNode.Stroke = Brushes.DarkSlateBlue;
            visNode.Fill = Brushes.LightSteelBlue;

            //po-dorbe border mai


            Canvas.SetLeft(visNode, location.X);
            Canvas.SetTop(visNode, location.Y);
            drawArea.Children.Add(visNode);

            var nodeText = new TextBlock();
            nodeText.Width = nodeSize;
            nodeText.Height = nodeSize;
			
			//defaulta e auto zatova se setva; trq po dr n-n da stane ili prosto da se mine na border

			nodeText.LineHeight = nodeSize/2;
			nodeText.Padding = new Thickness(nodeSize / 2 - nodeText.FontSize);

            var type = root.Token.Type;
            string text = "";
            if (type != TokenType.Operand) text = _operatorNames[type - TokenType.Or];
            else text += _operands?[operandidx];

			if (Calculated) { text += "\n" + (root.Value ? "1" : "0"); nodeText.Padding = new Thickness(0); }

            nodeText.Text = text;
            nodeText.TextAlignment = TextAlignment.Center;

			
			

			Canvas.SetLeft(nodeText, location.X);
            Canvas.SetTop(nodeText, location.Y);
            drawArea.Children.Add(nodeText);

            if (root.Token.Type == TokenType.Operand) { operandidx++;  return; }

			//reda
			var row = new List<TreeNode> { tree };
			for (int i = 0; i < ((BranchNode)root).operands.Count; i++)
			{
				int count = ((BranchNode)root).operands.Count;
				
				var curSpace = operandSpace / count;
				//var subTreeWidth= ((BranchNode)root).operands[i]
				double center=location.X + nodeSize / 2;

				

				//tuka e schupoeno zadeto se smesva d/bfs
				//trq sas zapomnqne prosto
				if (SpaceCheck(row, ((BranchNode)root).operands[i])) curSpace *= count;

				//vzima istavashtoto prostranstvo
				double newCenter = center + (curSpace) * (count - 1) * (2 * i-1);

                var line = new Line();

				line.X1 = center;
				line.X2 = newCenter;
				line.Y1 = location.Y + nodeSize;
				line.Y2 = line.Y1+nodeSize/2;

				line.Stroke = visNode.Stroke;
				line.StrokeThickness = 2;

                drawArea.Children.Add(line);

				var newLocation = new Point { X = newCenter - nodeSize / 2, Y = line.Y2 };
				//sledvashtiq vazel
                
				//reKURsiq ☠
                DrawNode(((BranchNode)root).operands[i], newLocation, curSpace, ref operandidx);
			}
		}
		public void ResizeCanvas(ref int level, ref int maxNodeCount, List<TreeNode> nodes)
		{
            if (nodes.Count==0) return;
            var next = new List<TreeNode>();

            //mai po-dobre otkolkoto flag dali se splitva(?)
            int maxOp = 1;
            foreach (var node in nodes)
            {

                if (node.Token.Type == TokenType.Operand) continue;
				if (((BranchNode)node).operands.Count > maxOp) maxOp = 2;


                next.AddRange(((BranchNode)node).operands);

            }
			maxNodeCount *= maxOp;
			level++;
			var treeHeight = nodeSize * level * 1.5;
			if (treeHeight > drawArea.Height) drawArea.Height = treeHeight;
			var treeWidth = maxNodeCount * nodeSize;
			if (treeWidth > drawArea.Width) drawArea.Width = treeWidth;
			ResizeCanvas(ref level, ref maxNodeCount, next);

        }
		public void DrawTree()
		{
			drawArea.Width = ((Control)drawArea.Parent).ActualWidth;
            drawArea.Height = ((Control)drawArea.Parent).ActualHeight;

			//var visNode = new Ellipse();
			//visNode.Width = visNode.Height=nodeSize;
			//visNode.StrokeThickness=1;
			//visNode.Stroke = Brushes.Red;
			//Canvas.SetLeft( visNode, drawArea.ActualWidth / 2-nodeSize/2);
			//drawArea.Children.Add(visNode);
			
			int level = 0;
			int hNodeCount = 1;

            ResizeCanvas(ref level, ref hNodeCount,new List<TreeNode>() {tree});

			int opidx = 0;

			
			
			DrawNode(tree, new Point {X= drawArea.Width / 2 - nodeSize / 2, Y=0 }, drawArea.Width/2, ref opidx);
			
			
		}

	}

}
