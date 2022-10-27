using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                dataGridView1.ClearSelection();
                dataGridView1.SuspendLayout();
                CurrencyManager currencyManager1 = dataGridView1.DataSource != null ? (CurrencyManager)dataGridView1.BindingContext[dataGridView1.DataSource] : null;
                currencyManager1?.SuspendBinding();
                if (string.IsNullOrEmpty(textBoxSearch.Text))
                {
                    dataGridView1.DataSource = repo.Objects;
                }
                else
                {
                    try
                    {
                        Regex reg = new Regex(textBoxSearch.Text, RegexOptions.Compiled);
                        ConcurrentBag<GitObject> bag = new ConcurrentBag<GitObject>();
                        Parallel.ForEach(repo.Objects, i =>
                        {
                            if (reg.IsMatch(i.Path))
                            {
                                bag.Add(i);
                            }
                            else if (reg.IsMatch(i.GitHash))
                            {
                                bag.Add(i);
                            }
                        });
                        dataGridView1.DataSource = bag.OrderByDescending(x=>x.FileSizeInBytes).ToArray();
                    }
                    catch
                    {

                    }
                }
                currencyManager1?.ResumeBinding();
                dataGridView1.ResumeLayout();
            }
        }

        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
            dataGridView1.Columns[nameof(GitObject.FileSizeInBytes)].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }
}
