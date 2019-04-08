using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RE2DFA_syntax
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        internal static int Prec(char ch)
        {
            switch (ch)
            {
                case '.': return 1;
                case '*': return 3;
                case '|':
                case '+': return 2;
            }
            return -1;
        } 
        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            String exp = textBox1.Text + ".#";
            string result = "";

            
            Stack<char> stack = new Stack<char>();

            for (int i = 0; i < exp.Length; ++i)
            {
                char c = exp[i];

                 
                if (char.IsLetterOrDigit(c) || c=='#')
                {
                    result += c;
                }
                else if (c == '(')
                {
                    stack.Push(c);
                }
             
                else if (c == ')')
                {
                    while (stack.Count > 0 && stack.Peek() != '(')
                    {
                        result += stack.Pop();
                    }

                    if (stack.Count > 0 && stack.Peek() != '(')
                    {
                        ;
                    }
                    else
                    {
                        stack.Pop();
                    }
                }
                else 
                {
                    while (stack.Count > 0 && Prec(c) <= Prec(stack.Peek()))
                    {
                        result += stack.Pop();
                    }
                    stack.Push(c);
                }
            }

          
            while (stack.Count > 0)
           {
                result += stack.Pop();
            }
            //arr = result.ToCharArray();
            //Array.Reverse(arr);
            //result = new String(arr);
            MessageBox.Show(result);
            create_parse_tree(result);
        }
        void create_parse_tree(String s)
        {
            int idctr=1;
            Stack<node> st = new Stack<node>();
            foreach (char c in s)
            {
                if (Char.IsLetterOrDigit(c) || c == '#')
                {node tmp = new node(c);
                    tmp.ID=idctr;
                    tmp.lastpos.Add(idctr);
                    tmp.firstpos.Add(idctr++);
                    //tmp.left = null; tmp.right = null;
                    st.Push(tmp);}
                else {
                    if (c == '*')
                    {
                        node d = st.Pop();
                        node now = new node(c);
                        now.firstpos = d.firstpos;
                        now.lastpos = d.lastpos;
                        now.left = d;
                        now.nullable = true;
                        //richTextBox1.Text += "\n*:"+now.lastpos.Count.ToString() + "_" + now.firstpos.Count.ToString();
                        st.Push(now);
                    }
                    if (c == '|'||c=='+')
                    {
                        node n1 = st.Pop();
                        node n2 = st.Pop();
                        node now = new node(c);
                        now.left = n2;
                        now.right = n1;
                        now.firstpos.UnionWith(n1.firstpos);
                        now.firstpos.UnionWith(n2.firstpos);
                        now.lastpos.UnionWith(n1.lastpos);
                        now.lastpos.UnionWith(n2.lastpos);
                        now.nullable = n1.nullable || n2.nullable;
                        //richTextBox1.Text += "\n|:" + now.lastpos.Count.ToString() + "_" + now.firstpos.Count.ToString();
                        st.Push(now);
 
                    }
                    if (c == '.')
                    {
                        node n1 = st.Pop();
                        node n2 = st.Pop();
                        node now = new node(c);
                        now.left = n2;
                        now.right = n1;
                        now.nullable = n1.nullable && n2.nullable;
                        if (n2.nullable)
                        {
                            now.firstpos.UnionWith(n1.firstpos);
                            now.firstpos.UnionWith(n2.firstpos);
                        }
                        else
                            now.firstpos.UnionWith(n2.firstpos);
                        if (n1.nullable)
                        {
                            now.lastpos.UnionWith(n1.lastpos);
                            now.lastpos.UnionWith(n2.lastpos);
                        }
                        else
                            now.lastpos.UnionWith(n1.lastpos);
                        //richTextBox1.Text += "\n.:" + now.lastpos.Count.ToString() + "_" + now.firstpos.Count.ToString();
                        st.Push(now);
                    }
                }

            }
            tree tr = new tree(st.Pop());
            tr.inorder(tr.root);
            tr.followpos = new HashSet<int>[idctr-1];
            for (int p = 0; p < idctr - 1; p++)
            {
                tr.followpos[p] = new HashSet<int>();
            }
            tr.follow_pos(tr.root);
           // MessageBox.Show((idctr - 1).ToString());
            Console.WriteLine();
            richTextBox1.Text += "\nfirstpos of root:  ";
            foreach (int x in tr.root.firstpos)
                richTextBox1.Text += x.ToString() + "_";
            richTextBox1.Text += "\nendpos of root:  ";
            foreach (int x in tr.root.lastpos)
                richTextBox1.Text += x.ToString() + "_";
            String fol="";
            for (int h = 0; h < tr.followpos.Length; h++)
            {
                fol += "\nfollowpos(" + (h + 1) + ")=";
                foreach (var set in tr.followpos[h])
                    fol += set + ",";
            }
            richTextBox1.Text += fol+"\n";
            gen_dfa(tr);
        }
        public void gen_dfa(tree t)
        {
            t.get_leaves(t.root);
            //MessageBox.Show(t.leaves.Count.ToString());
            int ctr = 1;
            dfa r = new dfa();
            state beg = new state();
            beg.name = t.root.firstpos;
            r.begin = beg;
            r.states.Add(beg);
            Stack<state> steck = new Stack<state>();
            String transit="";
            steck.Push(beg);
            while (steck.Count > 0)
            {
                //state dd = new state();
                state see = steck.Pop();
                foreach (var v in new HashSet<char>(t.leaves.Values))//for all symbols
                {
                    state dd = new state();
                    foreach (int elem in see.name)
                    {
                        if(t.leaves[elem]==v)
                        {
                            dd.name.UnionWith(t.followpos[elem - 1]);
                            //richTextBox1.Text += STR(t.followpos[elem - 1]) + "|||";
                        }
                    }
                    if(v!='#')
                        richTextBox1.Text += "d(" + STR(see.name) + "," + v + ")->" + STR(dd.name)+"\n";
                    bool b = r.states.Add(dd);
                    if(b)
                        steck.Push(dd);
                }
            }
           // richTextBox1.Text += transit;
         }
        String STR(HashSet<int> a)
        {
            String result = "";
            foreach (int i in a)
                result += i;
            if (result != "")
                result = "S" + result;
            return result;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
    public class node 
    {
        public int ID;
        public char symbol;
        public node left,right;
        public bool nullable;
        public HashSet<int> firstpos;
        public HashSet<int> lastpos;
        public node(char s)
        {
            symbol = s;
            ID = -1;
            nullable = false;
            left = null; right = null;
            firstpos = new HashSet<int>();
            lastpos = new HashSet<int>();

        }

    }
    public class tree
    {
        public node root;
        public HashSet<int> []followpos;
        public Dictionary<int, char> leaves = new Dictionary<int, char>();
        public tree(node n)
        {
            root = n;
        }
        public void inorder(node n)
        {
            if (n == null)
                return;
            inorder(n.left);
            Console.Write(n.symbol + "_");
            inorder(n.right);
            //Console.Write(n.symbol + "_");
        }
        public void get_leaves(node n)
        {
            if (n == null)
                return;
            if (n.right == null && n.left == null)
            {
                leaves.Add(n.ID, n.symbol);
            }
            get_leaves(n.left);
            get_leaves(n.right);
            return;
        }
        public void follow_pos(node n)
        {
            if (n == null)
                return;
            node c1 = n.left;
            node c2 = n.right;
            if (n.symbol == '.')
            {
                //node c1 = n.left;
                //node c2 = n.right;
                foreach (int i in c1.lastpos)
                {
                    foreach (int x in c2.firstpos)
                    {
                        followpos[i - 1].Add(x);
                    }
                }
            }
            if (n.symbol == '*')
            {
                foreach (int i in n.lastpos)
                {
                    foreach (int x in n.firstpos)
                    {
                        followpos[i - 1].Add(x);
                    }
                }
            }
            follow_pos(c1);
            follow_pos(c2);
        }
     }
    public class state
    {
        bool is_marked;
        bool is_final;
        public Dictionary<Char, state> trans = new Dictionary<char, state>();
        public HashSet<int> name= new HashSet<int>();
        public state()
        {
            is_marked = false;
        }
    }
    public class dfa
    {
        public state begin;
        //public HashSet<state> states = new HashSet<state>(new stateComparer());
        public SortedSet<state> states = new SortedSet<state>(new stateComparer());
    }
    public class stateComparer : System.Collections.Generic.IComparer<state>
    {
        public int Compare(state s1, state s2)
        {
            if (((s1.name.Except(s2.name)).ToList()).Count==0  )
                return 0;
            else
                return 1;
        }
    }
}
