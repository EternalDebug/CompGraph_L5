using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

namespace Laba5
{
	public partial class Form1 : Form
	{
		private Graphics g;
		public GraphicsPath Path = null;
		List<Tuple<double, double, double, double, int>> Lines; // x,y,x1,y1,deep

		//Структура для обозначения состояния. Координаты точки и угол
		public class Cond
		{
			public double angle;
			public double x;
			public double y;
			public double x1;
			public double y1;
			public double p_ang;
			public double rp_ang;
			public int line_deep;

			public Cond(double xx, double yy, double xx1, double yy1, double angle1, double pov_ang, double rpov_ang, int l_deep)
			{
				x = xx;
				y = yy;
				x1 = xx1;
				y1 = yy1;
				angle = angle1;
				p_ang = pov_ang;
				rp_ang = rpov_ang;
				line_deep = l_deep;
			}
		}

		public Form1()
		{
			
			InitializeComponent();
			g = this.CreateGraphics();
			Path = new GraphicsPath();
			Lines = new List<Tuple<double, double, double, double,int>>();
		}

		public void DrawF(double x, double y, Frac f)
		{
			//Стек состояний
			Stack<Cond> Con = new Stack<Cond>();
			double x1 = x;
			double y1 = y;
			//Graphics g = e.Graphics;
			double angleP = ((double)f.angle * Math.PI) / 180;
			double angle = ((double)(f.stAngle + 270 )* Math.PI) / 180;
			int step = f.F;
			string rules = f.res;
			Random rnd = new Random();
			int deep = 1;

			double reserveAngle = angleP;

			foreach (char a in rules)
			{
				if (a.Equals('F'))
				{
					x1 += step * Math.Sin(angle);
					y1 += step * Math.Cos(angle);
					Point cur = new Point((int)x, (int)y);
					Point next = new Point((int)x1, (int)y1);
					
					Path.AddLine(cur, next);
					Lines.Add(new Tuple<double, double, double, double,int>(x,y,x1,y1,deep));
					//g.DrawLine(new Pen(Color.Blue), x, y, (int)x1, (int)y1);
					x = x1;
					y = y1;

				}
				/*if (a.Equals('B'))
				{
					x1 += step * Math.Sin(angle);
					y1 += step * Math.Cos(angle);

					x = (int)x1;
					y = (int)y1;
				}*/
				if (a.Equals('-'))
				{
					angle -= angleP;
					
				}
				if (a.Equals('+'))
				{
					angle += angleP;
					
				}
				if (a.Equals('['))
				{
					deep++;
					Con.Push(new Cond(x, y,x1,y1, angle, angleP, reserveAngle, deep));
				}
				if (a.Equals(']'))
				{
					var cnd = Con.Pop();
					x = cnd.x;
					y = cnd.y;
					x1 = cnd.x1;
					y1 = cnd.y1;
					angle = cnd.angle;
					angleP = cnd.rp_ang;
					deep = cnd.line_deep;
				}
				if (a.Equals('@'))
				{
					reserveAngle = angleP;
					angleP = ((double)rnd.Next(-45, 45) * Math.PI) / 180;  
				}
			}
		}

		public class Frac
		{
			public int F;
			public int angle;
			public int stAngle;
			public string Axiom;
			public Dictionary<string, string> rules;

			public string res;

			public Frac(string pth, int depth)
			{
				StringBuilder sb = new StringBuilder();
				rules = new Dictionary<string, string>();
				StreamReader sr = new StreamReader(pth);
				string line;
				if (!sr.EndOfStream)
				{
					line = sr.ReadLine();
					var sp = line.Split(' ');

					F = int.Parse(sp[0]);
					angle = int.Parse(sp[1]);
					stAngle = int.Parse(sp[2]);
				}

				if (!sr.EndOfStream)
				{
					line = sr.ReadLine();
					Axiom = line;
				}

				while (!sr.EndOfStream)
				{
					line = sr.ReadLine();
					var sp = line.Split(' ');
					
					rules[sp[0]] = sp[1];
				}
				sr.Close();

				res = Axiom;
				//получить шаблон, по которому пойдёт отрисовка
				for (int i = 0; i < depth; i++)
				{
					
					for (int j = 0; j < res.Length; j++)
					{
						if (res[j] >= 'A' && res[j] <= 'Z' && rules.ContainsKey(res[j].ToString()))
						{
							sb.Append(rules[res[j].ToString()]);
						}
						else
							sb.Append(res[j]);
					}

					res = sb.ToString();
					sb = new StringBuilder();
				}

				//Удалить нетерминалы
				foreach (var key in rules.Keys)
				{
					if (key != "F")
						res = res.Replace(key, "");
				}

				res = res.Replace("[+]", "");
				res = res.Replace("[-]", "");
				res = res.Replace("[@+]", "");
				res = res.Replace("[@-]", "");
			}
		}

		private void Button1_Click(object sender, EventArgs e)
		{
			int iters = int.Parse(textBox1.Text);
			Frac f = new Frac("test4.txt", iters);
			DrawF(0, 0, f);

			var rect = Path.GetBounds();
			var scale = Math.Min(
				(float)(ClientSize.Width - 50) / rect.Width,
				(float)(ClientSize.Height - 50) / rect.Height);

			// Перемещаем (0, 0) в центр окна и устанавливаем масштаб
			g.TranslateTransform(ClientSize.Width / 2, ClientSize.Height / 2);
			g.ScaleTransform(scale, scale);
			g.TranslateTransform(-rect.X - rect.Width / 2, -rect.Y - rect.Height / 2);

			UInt32 a = 0x4D220E; //коричневый (старт)
			UInt32 b = 0x00FF00; //зелень
			UInt32 c = 0x4D220E; //текущий
			int line_width = 6; //Начальная толщина линии при глубине 0
			Pen p = new Pen(Color.FromArgb((int)a),line_width);

			if (!checkBox1.Checked)
			{
				p = new Pen(Color.Black);
			}
			// Рисуем фрактал
			//g.DrawPath(new Pen(Color.Black, 1 / scale), Path);
			int max_deep = 0;
			foreach (var t in Lines)
			{
				if (t.Item5 > max_deep)
					max_deep = t.Item5;
			}

			foreach (var t in Lines)
			{
				//g.DrawLine(p, (int) t.)
				if (checkBox1.Checked)
				{
					var l_width = (int)((double)line_width * Math.Pow(1.4, iters) / (double)t.Item5);
					c = a;
					c = (UInt32)0xFF000000 | (UInt32)((UInt32)(((c & 0x00FF0000) >> 16) / t.Item5 * 1.5) << 16) | (UInt32)((UInt32)(((c & 0x0000FF00) >> 8) / t.Item5 * 1.5) + (UInt32)((0x00FF00 >> 8) * (1.0 - 1.0 / t.Item5)) << 8) | (UInt32)((c & 0x000000FF) / t.Item5 * 1.5);
					//c = (UInt32)0xFF000000 | (UInt32)((UInt32)(((c & 0x00FF0000) >> 16) * (1.0 - t.Item5 / max_deep)) << 16) | (UInt32)((UInt32)(((c & 0x0000FF00) >> 8) * (1.0 - t.Item5 / max_deep)) << 8 | (UInt32)((0x00FF00 >> 8) * (t.Item5 / max_deep)) << 8) | (UInt32)((c & 0x000000FF) * (1.0 - t.Item5 / max_deep));
					p = new Pen(Color.FromArgb((int)c), l_width);
					g.DrawLine(p, (int)t.Item1, (int)t.Item2, (int)t.Item3, (int)t.Item4);
				}
				else
				{
					p = new Pen(Color.Black);
					g.DrawLine(p, (int)t.Item1, (int)t.Item2, (int)t.Item3, (int)t.Item4);
				}
			}
			
		}
	}
}
