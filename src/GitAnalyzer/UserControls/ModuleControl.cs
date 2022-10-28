using GitAnalyzer.Modules;
using GitAnalyzer.Modules.GitObjects;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitAnalyzer.UserControls
{
    public partial class ModuleControl : UserControl
    {
        public BaseModule Module { get; private set; }
        public GitRepository Repository { get; private set; }
        public ModuleControl()
        {
            InitializeComponent();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                IList results = Module.ModuleResult;
                if (!string.IsNullOrEmpty(textBoxSearch.Text))
                {
                    try
                    {
                        if (Module.ModuleResult.Count > 0)
                        {
                            Regex reg = new Regex(textBoxSearch.Text, RegexOptions.Compiled);
                            ConcurrentBag<object> bag = new ConcurrentBag<object>();
                            var obj = Module.ModuleResult[0];

                            var props = obj.GetType().GetProperties()
                                .Where(x=>x.GetCustomAttribute<SearchableAttribute>() != null)
                                .ToArray();

                            Parallel.For(0, Module.ModuleResult.Count, i =>
                            {
                                foreach(var value in props.Select(x => x.GetValue(Module.ModuleResult[i]).ToString()))
                                { 
                                    if (reg.IsMatch(value))
                                    {
                                        bag.Add(Module.ModuleResult[i]);
                                        break;
                                    }
                                }
                            });
                            results = new List<object>();
                            var type = obj.GetType();
                            foreach (var item in bag.OrderByDescending(x => Module.DefaultSort(x)))
                            {
                                // Not proud of this, please help me T_T
                                results.Add(Convert.ChangeType(item, type));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                dataGridView1.DataSource = results;
            }
        }

        internal void SetupModule(BaseModule module, GitRepository repo)
        {
            Repository = repo;
            Module = module;

            labelModule.Text = Module.ModuleName;
            Module.ModuleProgressChanged += Module_ModuleProgressChanged;
            Module.ModuleExecutionFinished += Module_ModuleExecutionFinished;
        }

        private void Module_ModuleExecutionFinished(BaseModule sender, ExecutionFinishedEventArgs args)
        {
            dataGridView1.DataSource = args.Result;
        }

        private void Module_ModuleProgressChanged(BaseModule sender, ExecutionProgressChangedEventArgs state)
        {
            listBox1.SelectedIndex = listBox1.Items.Add(state.State);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Module.Run(Repository);
        }
    }
}
