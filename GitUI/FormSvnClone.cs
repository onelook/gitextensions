﻿

using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormSvnClone : GitModuleForm
    {
        private readonly TranslationString _questionOpenRepo =
           new TranslationString("The repository has been cloned successfully." + Environment.NewLine +
                                 "Do you want to open the new repository \"{0}\" now?");
        
        private readonly TranslationString _questionOpenRepoCaption =
            new TranslationString("Open");

        private readonly TranslationString _questionContinueWithoutAuthors =
            new TranslationString("Authors file \"{0}\" does not exists. Continue without authors file?");

        private readonly TranslationString _questionContinueWithoutAuthorsCaption = new TranslationString("Authors file");
        private readonly GitModuleChangedEventHandler GitModuleChanged;

        private FormSvnClone()
            : this(null, null)
        { }

        public FormSvnClone(GitUICommands aCommands, GitModuleChangedEventHandler GitModuleChanged)
            : base(aCommands)
        {
            this.GitModuleChanged = GitModuleChanged;
            InitializeComponent();
            this.Translate();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                var dirTo = this._NO_TRANSLATE_destinationComboBox.Text;
                if (!dirTo.EndsWith(Settings.PathSeparator.ToString()) && !dirTo.EndsWith(Settings.PathSeparatorWrong.ToString()))
                    dirTo += Settings.PathSeparator.ToString();

                dirTo += this._NO_TRANSLATE_subdirectoryTextBox.Text;

                //Repositories.RepositoryHistory.AddMostRecentRepository(_NO_TRANSLATE_From.Text);
                //Repositories.RepositoryHistory.AddMostRecentRepository(dirTo);

                if (!Directory.Exists(dirTo))
                    Directory.CreateDirectory(dirTo);

                var authorsfile = this._NO_TRANSLATE_authorsFileTextBox.Text.Trim();
                bool resetauthorsfile = false;
                if (!String.IsNullOrEmpty(authorsfile) && !File.Exists(authorsfile) && !(resetauthorsfile = AskContinutWithoutAuthorsFile(authorsfile)))
                {
                    return;
                }
                if (resetauthorsfile)
                {
                    authorsfile = null;
                }
                int from;
                if (!int.TryParse(tbFrom.Text, out from))
                    from = 0;
                
                var errorOccurred = !FormProcess.ShowDialog(this, Settings.GitCommand, 
                    GitSvnCommandHelpers.CloneCmd(_NO_TRANSLATE_svnRepositoryComboBox.Text, dirTo,
                    tbUsername.Text, authorsfile, from,
                    cbTrunk.Checked ? tbTrunk.Text : null,
                    cbTags.Checked ? tbTags.Text : null,
                    cbBranches.Checked ? tbBranches.Text : null));
                
                if (errorOccurred || Module.InTheMiddleOfPatch())
                    return;
                if (ShowInTaskbar == false && AskIfNewRepositoryShouldBeOpened(dirTo))
                {
                    if (GitModuleChanged != null)
                        GitModuleChanged(new GitModule(dirTo));
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Exception: " + ex.Message, "Clone failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AskContinutWithoutAuthorsFile(string authorsfile)
        {
            return MessageBox.Show(
                this, string.Format(_questionContinueWithoutAuthors.Text, authorsfile), this._questionContinueWithoutAuthorsCaption.Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private bool AskIfNewRepositoryShouldBeOpened(string dirTo)
        {
            return MessageBox.Show(this, string.Format(_questionOpenRepo.Text, dirTo), _questionOpenRepoCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = this._NO_TRANSLATE_destinationComboBox.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    this._NO_TRANSLATE_destinationComboBox.Text = dialog.SelectedPath;
            }
        }

        private void authorsFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { InitialDirectory = this._NO_TRANSLATE_destinationComboBox.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    this._NO_TRANSLATE_authorsFileTextBox.Text = dialog.FileName;
            }
        }

        private void destinationComboBox_DropDown(object sender, EventArgs e)
        {
            System.ComponentModel.BindingList<Repository> repos = Repositories.RepositoryHistory.Repositories;
            if (this._NO_TRANSLATE_destinationComboBox.Items.Count != repos.Count)
            {
                this._NO_TRANSLATE_destinationComboBox.Items.Clear();
                foreach (Repository repo in repos)
                    this._NO_TRANSLATE_destinationComboBox.Items.Add(repo.Path);
            }
        }

        private void cbTrunk_CheckedChanged(object sender, EventArgs e)
        {
            tbTrunk.Enabled = cbTrunk.Checked;
        }

        private void cbTags_CheckedChanged(object sender, EventArgs e)
        {
            tbTags.Enabled = cbTags.Checked;
        }

        private void cbBranches_CheckedChanged(object sender, EventArgs e)
        {
            tbBranches.Enabled = cbBranches.Checked;
        }

        private void tbFrom_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
