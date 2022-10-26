using Newtonsoft.Json;
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

namespace GitAnalyzer
{
    public partial class Form2 : Form
    {
        GitRepo repo;
        string Path;
        bool IsPureData;
        public Form2(string path, bool isPureData)
        {
            Path = path;
            IsPureData = isPureData;
            InitializeComponent();
        }

        private void Repo_UpdateProgress(string obj)
        {
            listBox1.Items.Add($"{DateTime.Now.ToString("HH:mm:ss")}: {obj}");
        }

        private void SaveDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (repo == null) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            if (!string.IsNullOrWhiteSpace(sfd.FileName))
            {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(repo));
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            GitRepo.CollectGitInfo();
            if (IsPureData)
            {
                repo = JsonConvert.DeserializeObject<GitRepo>(File.ReadAllText(Path));
                dataGridView1.DataSource = repo.Objects;
            }
            else
            {
                repo = new GitRepo(Path);
                repo.UpdateProgress += Repo_UpdateProgress;
                repo.Finished += Repo_Finished;
                repo.GetGitObjects();
            }
        }

        private void Repo_Finished()
        {
            dataGridView1.DataSource = repo.Objects;
        }
    }
}
