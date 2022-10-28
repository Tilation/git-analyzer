using GitAnalyzer.Modules;
using GitAnalyzer.Modules.GitObjects;
using GitAnalyzer.UserControls;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitAnalyzer
{
    public partial class FormMain : Form
    {
        GitRepository Repository { get; set; }
        public FormMain()
        {
            InitializeComponent();
            BuildModuleMenu();
        }

        void BuildModuleMenu()
        {
            var builtIn = ModuleLoader.GetInternalModules();
            foreach(var module in builtIn)
            {
                ToolStripMenuItem button = new ToolStripMenuItem();
                button.Text = module.ModuleName;
                RegisterModuleAction(button, module);
                modulesToolStripMenuItem.DropDownItems.Add(button);
            }
            modulesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            var external = ModuleLoader.GetExternalModules();
            if (external.Length > 0)
            {
                foreach (var module in external)
                {
                    var button = new ToolStripMenuItem();
                    button.Text = module.ModuleName;
                    RegisterModuleAction(button, module);
                    modulesToolStripMenuItem.DropDownItems.Add(button);
                }
            }
            else
            {
                modulesToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem
                {
                    Text = "No external modules found.\nDrop some in /modules",
                    Enabled = false
                });
            }
            modulesToolStripMenuItem.DropDown.AutoSize = false;
            modulesToolStripMenuItem.DropDown.AutoSize = true;
        }

        private void RegisterModuleAction(ToolStripMenuItem button, BaseModule module)
        {
            button.Click += (sender, args) =>
            {
                if (Repository == null)
                {
                    MessageBox.Show("First select a valid GIT repository.");
                    return;
                }
                ModuleControl control = new ModuleControl();
                control.SetupModule(module, Repository);
                control.Dock = DockStyle.Fill;

                var tab = new TabPage(module.ModuleName);
                tab.Controls.Add(control);
                tabControl1.TabPages.Add(tab);
            };
        }

        private void selectGITRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            DialogResult result;
            do
            {
                result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Repository = new GitRepository(ofd.SelectedPath);
                    if (!Repository.Exists())
                    {
                        Repository = null;
                        MessageBox.Show($"There is no repository at {ofd.SelectedPath}");
                    }
                }
            } while (result == DialogResult.OK && Repository == null);
            Text = $"Git Analyzer - Selected Repo: {Repository.RepositoryPath}";
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            GitRepository.CollectGitInfo();
        }
    }
}
