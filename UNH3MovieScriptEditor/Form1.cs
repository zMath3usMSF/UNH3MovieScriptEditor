using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UNH3MovieScriptEditor
{
    public partial class Form1 : Form
    {
        List<byte[]> scriptBlocks = new List<byte[]>();
        List<string> texts = new List<string>();
        string movieFileName;

        public Form1()
        {
            InitializeComponent();
            listBox1.SelectedIndexChanged +=ListBox1_SelectedIndexChanged;
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;
            richTextBox1.Text = texts[selectedIndex];
            lblCurrentText.Text = $"{selectedIndex}";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "UNH3 Movie Script (*.bin)|*.bin|All Files (*.*)|*.*";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                movieFileName = Path.GetFileNameWithoutExtension(ofd.FileName);
                ReadMovieScript(ofd.FileName);
            }
        }

        private void ReadMovieScript(string fileName)
        {
            scriptBlocks = new List<byte[]>();
            texts = new List<string>();
            listBox1.Items.Clear();
            pictureBox1.Visible = false;
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName));

            byte[] buffer = new byte[4];
            ms.Read(buffer, 0, buffer.Length);
            int textCount = BitConverter.ToInt32(buffer, 0);

            for(int i = 0; i < textCount; i++)
            {
                byte[] scriptBlock = new byte[0x54];
                ms.Read(scriptBlock, 0, scriptBlock.Length);
                scriptBlocks.Add(scriptBlock);
            }

            for(int i = 0; i < textCount; i++)
            {
                List<byte> chars = new List<byte>();
                while(true)
                {
                    int cByte = ms.ReadByte();
                    if(cByte == 0)
                    {
                        texts.Add(Encoding.GetEncoding("ISO-8859-1").GetString(chars.ToArray()));
                        break;
                    }
                    chars.Add((byte)cByte);
                }
            }

            foreach(string text in texts)
            {
                listBox1.Items.Add(text);
            }

            lblNumberOfTexts.Text = $"{texts.Count}";
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            listBox1.SelectedIndexChanged -=ListBox1_SelectedIndexChanged;
            int selectedIndex = int.Parse(lblCurrentText.Text);
            listBox1.Items[selectedIndex] = richTextBox1.Text;
            texts[selectedIndex] = richTextBox1.Text;
            listBox1.SelectedIndexChanged +=ListBox1_SelectedIndexChanged;
        }

        private void exportAllToTXTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = movieFileName;
            sfd.Filter = "Txt File (*.txt)|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllLines(sfd.FileName, texts.ToArray());
            }
        }

        private void importAllFromTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Txt File (*.txt)|*.txt|All Files (*.*)|*.*";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                texts = File.ReadAllLines(ofd.FileName).ToList();
                listBox1.Items.Clear();
                foreach (string text in texts)
                {
                    listBox1.Items.Add(text);
                }
                MessageBox.Show("All texts have been imported successfully!");
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            byte[] textCount = BitConverter.GetBytes(Convert.ToInt32(texts.Count));
            ms.Write(textCount, 0, textCount.Length);
            foreach (byte[] scriptBlock in scriptBlocks)
            {
                ms.Write(scriptBlock, 0, scriptBlock.Length);
            }
            foreach (string text in texts)
            {
                byte[] textBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
                ms.Write(textBytes, 0, textBytes.Length);
                ms.WriteByte(0);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = movieFileName;
            sfd.Filter = "UNH3 Movie Script File (*.bin)|*.bin";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sfd.FileName, ms.ToArray());
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("UNH3 Movie Script Text Editor, version 1.0. \n\nMade by zMath3usMSF.");
        }
    }
}
