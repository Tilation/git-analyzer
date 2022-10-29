using GitAnalyzer.Modules;
using GitAnalyzer.Modules.GitObjects;
using GitAnalyzer.UserControls;
using System;
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
            foreach (var module in builtIn)
            {
                ToolStripMenuItem button = new ToolStripMenuItem();
                button.Text = module.ModuleName;
                RegisterModuleAction(button, module.GetType());
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
                    RegisterModuleAction(button, module.GetType());
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

        private void RegisterModuleAction(ToolStripMenuItem button, Type moduleType)
        {
            button.Click += (sender, args) =>
            {
                if (Repository == null)
                {
                    Repository = BrowseForRepository();
                    if (Repository == null) return;
                }
                else
                {
                    var dr = MessageBox.Show($"Use last GIT repository? {Repository.GetName()}", "Git Analyzer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                    {
                        Repository = BrowseForRepository();
                        if (Repository == null) return;
                    }
                }
                ModuleControl control = new ModuleControl();
                var module = (BaseModule)Activator.CreateInstance(moduleType);
                control.SetupModule((BaseModule)Activator.CreateInstance(moduleType), Repository);
                control.Dock = DockStyle.Fill;

                var tab = new TabPage(module.ModuleName);
                tab.Controls.Add(control);
                tabControl1.TabPages.Add(tab);

                control.Disposed += (s, a) =>
                {
                    tabControl1.TabPages.Remove(tab);
                };
            };
        }

        GitRepository BrowseForRepository()
        {
            GitRepository repo = null;
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            DialogResult result;
            do
            {
                result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    repo = new GitRepository(ofd.SelectedPath);
                    if (!repo.Exists())
                    {
                        repo = null;
                        MessageBox.Show($"There is no repository at {ofd.SelectedPath}");
                    }
                }
            } while (result == DialogResult.OK && repo == null);
            return repo;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            GitRepository.CollectGitInfo();
        }
    }
}
