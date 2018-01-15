using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace QuizApp
{
    public partial class Form1 : Form
    {     
        #region TODO's
        // setup the timed quiz 
        // use a background worker, that will tick every so time(used in the quiz file)
        // if the progressbar fills up before the user inputs an answer the question is skipped
        // (thus missing the increment correct answer step)
        // cover edge case for last question (don't want to go out of bounds)

        // In the quiz group selection menu possibly don't show the quiz group if it 
        // doesn't have any files in it. Or if selected show that the group has no quizes.

        // add quiz history menu (good way to start with graphs)
        // this would require a custom quiz result class 
        // and possibly some serialization

        // Create column based listboxes that has titles per row for
        // data organization.
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        #region Private Fields
        // settings menu will be able to change this value in runtime
        private string _DefaultPath = (@"C:\Users\"+ Environment.UserName+ @"\Documents\QuizApp\");
        private string _loadedQuizPath;
        private string _WorkingPath;
        private TermSet _WorkingTermSet;
        private Dictionary<string, string> _WorkingTermSetDictionary = new Dictionary<string, string>();
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            // temp
            buttonQuizTimed.Enabled = false;
            //
            
            #region Event Subscription

            // notify UpdateContinueButtonStatusHandler
            textBoxSecondsPerQuestion.TextChanged += UpdateContinueButtonStatusHandler;
            textBoxQuizName.TextChanged += UpdateContinueButtonStatusHandler;
            radioButtonTimedQuiz.CheckedChanged += UpdateContinueButtonStatusHandler;
            radioButtonUntimedQuiz.CheckedChanged += UpdateContinueButtonStatusHandler;
            textBoxTermName.TextChanged += UpdateContinueButtonStatusHandler;
            listBoxQuizGroupsCreationAvaliable.SelectedIndexChanged += UpdateContinueButtonStatusHandler;

            // notify UpdateAddGroupButtonStatusHandler
            textBoxQuizName.TextChanged += UpdateAddGroupButtonStatusHandler;
            listBoxQuizGroupsCreationAvaliable.SelectedIndexChanged += UpdateAddGroupButtonStatusHandler;


            // notify UpdateAddTermButtonStatusHandler
            textBoxTermDefinition.TextChanged += UpdateAddTermButtonStatusHandler;

            // notify UpdateStartQuizButtonStatusHandler
            listBoxAvaliableQuizes.SelectedIndexChanged += UpdateStartQuizButtonStatusHandler;
            listBoxShowQuizForSelection.SelectedIndexChanged += UpdateStartQuizButtonStatusHandler;
            // misc
            panelActiveQuiz.VisibleChanged += PanelActiveQuiz_VisibleChanged;
            textBoxActiveQuizTerm.KeyDown += TextBoxActiveQuizTerm_KeyDown;

            // file system watcher events
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            fileSystemWatcherLoadedQuizesDirectory.Created += FileSystemWatcherLoadedQuizesDirectory_Created;
            fileSystemWatcherLoadedQuizesDirectory.Deleted += fileSystemWatcherLoadedQuizesDirectory_Deleted;

            // add menu popup for active quiz menu

            listBoxShowQuizForSelection.MouseDown += ListBoxShowQuizForSelection_MouseDown;
            listBoxShowQuizForSelection.MouseDoubleClick += ListBoxShowQuizForSelection_MouseDoubleClick;

            // add listeners for start active quiz button (if group is selected and quiz isn't..)
            
            #endregion

            #region Initialization / file loading / creation / FSW configuration
            // load loaded path
            _loadedQuizPath = Path.Combine(_DefaultPath, @"loaded_quizes\");
            //
            // init working termset
            _WorkingTermSet = new TermSet();
            // Check if directories are created.
            if (!Directory.Exists(_DefaultPath))
            {
                Directory.CreateDirectory(_DefaultPath);
            }
            if (!Directory.Exists(_loadedQuizPath))
            {
                Directory.CreateDirectory(_loadedQuizPath);
            }
             
            // load the files already in the Default operating path
            // (load the quiz groups)
            foreach (var file in Directory.GetDirectories(_DefaultPath))
            {
                // instead of adding directoryInfo objects we are now adding
                // the CustomFileObject objects
                listBoxQuizGroupsCreationAvaliable.Items.Add(new CustomFileObject()
                {
                    FilePath = file
                   
                });
                listBoxAvaliableQuizes.Items.Add(new CustomFileObject()
                {
                    FilePath = file
                });
            }
            //
            
            // default path file system watcher
            // allow the watcher to raise events (directory changed / last written ..)
            //fileSystemWatcher.EnableRaisingEvents = true;
            // watch the default path
            fileSystemWatcher.Path = _DefaultPath;
            // search for all files
            fileSystemWatcher.Filter = "";

            // loaded path file system watcher
            //fileSystemWatcherLoadedQuizesDirectory.EnableRaisingEvents = true;
            fileSystemWatcherLoadedQuizesDirectory.Path = _loadedQuizPath;
            fileSystemWatcherLoadedQuizesDirectory.Filter = "";


            // Initialize the PanelVisibility class Panel List
            PanelVisibility.PanelList = new List<Panel>
            {
                panelMainMenu, panelCreate, panelNoQuizesFound, panelQuizCreation,
                panelQuizStartSelection, panelSettings, panelActiveQuiz, panelActiveQuizSelectionContextMenu,
                panelGradeResultsMenu
            };
            #endregion
        }

       
        #region File System watchers
        #region File System watcher on the operating directory
        private CustomFileObject _customFileObject = new CustomFileObject();
        //update both listboxes when a file is added to / deleted from the default path
        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _customFileObject.FilePath = e.FullPath;
            listBoxQuizGroupsCreationAvaliable.Items.Remove(_customFileObject);
            listBoxAvaliableQuizes.Items.Remove(_customFileObject);
            
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _customFileObject.FilePath = e.FullPath;
            listBoxQuizGroupsCreationAvaliable.Items.Add(_customFileObject);
            listBoxAvaliableQuizes.Items.Add(_customFileObject);
            
        }
        // nothing here for now
        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            
        }
        #endregion

        #region Loaded Quizes file system watcher
        private void fileSystemWatcherLoadedQuizesDirectory_Deleted(object sender, FileSystemEventArgs e)
        {
            // remove item in the quiz list
        }

        private void FileSystemWatcherLoadedQuizesDirectory_Created(object sender, FileSystemEventArgs e)
        {
            // add item to the quiz list
            Debug.WriteLine("Loaded Quiz File Directory:");
            Debug.WriteLine(e.Name+" was added");
            // will have to check if the directory is selected prior to populating that list box
        }
        #endregion
        #endregion

        #region Subscribers
        // when the event is raised (any of the events this method subscribes to)
        // it will update the listBoxShowQuizForSelection listbox with the current files
        private void UpdateStartQuizButtonStatusHandler(object sender, EventArgs e)
        {
            // automatically select an index in listBoxShowQuizForSelection to prevent errors from right click not selecting an index.

            if (listBoxAvaliableQuizes.SelectedIndex >= 0 && listBoxShowQuizForSelection.SelectedIndex >= 0)
            {
                buttonStartActiveQuiz.Enabled = true;
            }
            else
            {
                buttonStartActiveQuiz.Enabled = false;
            }

            if (sender == listBoxAvaliableQuizes)
            {
                listBoxShowQuizForSelection.Items.Clear();
                // var dir = ((DirectoryInfo)listBoxAvaliableQuizes.SelectedItem);
                var dir = ((CustomFileObject)listBoxAvaliableQuizes.SelectedItem);
                foreach (var file in Directory.GetFiles(dir.FilePath))
                {
                    listBoxShowQuizForSelection.Items.Add(new CustomFileObject()
                    {
                        FilePath = file
                    });
                }
            }
        }

        // when the event(s) are raised that this method subscribes to
        // it will check if the textboxtermname or the textboxtermdefinition textboxes are empty
        // and will enable / disable accordingly.
        private void UpdateAddTermButtonStatusHandler(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxTermName.Text) && !String.IsNullOrEmpty(textBoxTermDefinition.Text))
            {
                buttonAddTerm.Enabled = true;
            }
            else
                buttonAddTerm.Enabled = false;
        }

        // when the event(s) are raised that this method subscribes to
        // it will check if listBoxQuizGroupsCreationAvaliable has any selected indexes
        // and checks if the textBoxQuizName textbox is empty
        private void UpdateAddGroupButtonStatusHandler(object sender, EventArgs e)
        {
            if(listBoxQuizGroupsCreationAvaliable.SelectedIndex >= 0)
                buttonRemoveGroup.Enabled = true;
            else
                buttonRemoveGroup.Enabled = false;


            if (!String.IsNullOrEmpty(textBoxQuizName.Text))
                buttonAddGroup.Enabled = true;
            else
                buttonAddGroup.Enabled = false;
        }
       

        // Updates the continue button in the creation menu based on criteria.
        private void UpdateContinueButtonStatusHandler(object sender, EventArgs e)
        {
            if (radioButtonTimedQuiz.Checked)
                panelSecondsPerQuestion.Enabled = true;
            else
                panelSecondsPerQuestion.Enabled = false;

            // may eventually run events on the textBoxSecondsPerQuestion to check for a valid entry
            if (radioButtonUntimedQuiz.Checked && listBoxQuizGroupsCreationAvaliable.SelectedIndex >= 0 && !String.IsNullOrEmpty(textBoxQuizName.Text) 
                || radioButtonTimedQuiz.Checked && !String.IsNullOrEmpty(textBoxSecondsPerQuestion.Text) && !String.IsNullOrEmpty(textBoxQuizName.Text) 
                && listBoxQuizGroupsCreationAvaliable.SelectedIndex >= 0)
            {
                buttonQuizCreationContinue.Enabled = true;
            }
            else
                buttonQuizCreationContinue.Enabled = false;
        }
        #endregion

        #region Main menu buttons
        private void buttonQuizTimed_Click(object sender, EventArgs e)
        {
            // now set to greater than one to not include the loaded_quizes group (dir)
            if (Directory.GetDirectories(_DefaultPath).Length > 1)
                PanelVisibility.Show(panelQuizStartSelection);
            else
                PanelVisibility.Show(panelNoQuizesFound);

            radioButtonTimedQuiz.Checked = true;
            panelSecondsPerQuestion.Enabled = true;

            // from timed and untimed you can populate the two list boxes.
            //listBoxAvaliableQuizes <-- Quiz Groups which will expand into the individual quizes
            //listBoxShowQuizForSelection <-- here is where the individual quizes will be shown
        }

        private void buttonUntimedQuiz_Click(object sender, EventArgs e)
        {

            if (Directory.GetDirectories(_DefaultPath).Length >= 1)
                PanelVisibility.Show(panelQuizStartSelection);
            else
                PanelVisibility.Show(panelNoQuizesFound);

            radioButtonUntimedQuiz.Checked = true;
            panelSecondsPerQuestion.Enabled = false;
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // move selected file to the loaded_quizes directory
                File.Move(openFileDialog.FileName, _loadedQuizPath+openFileDialog.SafeFileName);
             
            }
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            textBoxQuizName.Clear();
            PanelVisibility.Show(panelQuizCreation);
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            PanelVisibility.Show(panelSettings);
        }
        #endregion

        #region panel (where terms are added and removed)
        private void buttonAddTerm_Click(object sender, EventArgs e)
        {
            listBoxPreviewTerm.Items.Add($"{textBoxTermName.Text}_\"{textBoxTermDefinition.Text}\"");
            textBoxTermName.Clear();
            textBoxTermDefinition.Clear();
        }

        private void buttonRemoveTerm_Click(object sender, EventArgs e)
        {
            listBoxPreviewTerm.Items.RemoveAt(listBoxPreviewTerm.SelectedIndex);
        }

        private void buttonQuizCreationFinished_Click(object sender, EventArgs e)
        {
            // here add all the files to the quiz group
           
            
            CreateQuiz(_WorkingPath);
            PanelVisibility.Show(panelQuizStartSelection);

            listBoxPreviewTerm.Items.Clear();
            textBoxTermName.Clear();
            textBoxTermDefinition.Clear();

            
        }
        private void buttonCreationMenuCancel_Click(object sender, EventArgs e)
        {
            PanelVisibility.Show(panelMainMenu);
        }

        // Check for keydown Keys.Enter
        private void TextBoxActiveQuizTerm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                if (!String.IsNullOrEmpty(textBoxActiveQuizTerm.Text))
                {
                    buttonSubmitAnswer_Click(null, null);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }

            }
        }

        #endregion

        #region panel (case where there are less than 2 groups)
        private void buttonCreateAQuiz_Click(object sender, EventArgs e)
        {
            PanelVisibility.Show(panelQuizCreation);
        }

        private void buttonGoToMainMenu_Click(object sender, EventArgs e)
        {
            _WorkingTermSetDictionary.Clear();
            PanelVisibility.Show(panelMainMenu);
        }

        #endregion

        #region panel (Choose from group then select individual quiz)

        private void buttonStartActiveQuiz_Click(object sender, EventArgs e)
        {
            // top: listBoxAvaliableQuizes
            // bottom: listBoxShowQuizForSelection

            var file = ((CustomFileObject)listBoxShowQuizForSelection.SelectedItem).FilePath;
            //DirectoryInfo dirInfo = new DirectoryInfo(file);
            // assign the _WorkingTermSet object based on the data read from the file
            ReadQuiz(file);
            PanelVisibility.Show(panelActiveQuiz);
        }

        private void buttonCancelActiveQuiz_Click(object sender, EventArgs e)
        {
            
            PanelVisibility.Show(panelMainMenu);

        }
        private void ListBoxShowQuizForSelection_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Fix this, make a secondary click activate a control in the list 
                // box
                try
                {
                    labelQuizActionQuizName.Text = (listBoxShowQuizForSelection.SelectedItem as CustomFileObject).FileName;
                    PanelVisibility.ShowWith(panelQuizStartSelection, panelActiveQuizSelectionContextMenu);
                }
                catch (Exception)
                {

                    
                }
               
            }
        }
        // -------------------------------------------------------
        // Active quiz selection menus
        #region Addition menu with buttons (right click menu on quiz item)
        private void ListBoxShowQuizForSelection_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonStartActiveQuiz_Click(null, null);
        }

        private void buttonCopyQuiz_Click(object sender, EventArgs e)
        {
            //Clipboard.SetData(DataFormats.Serializable, (listBoxShowQuizForSelection.SelectedItem as CustomFileObject));
            PanelVisibility.Show(panelQuizStartSelection);
        }

        private void buttonDeleteQuiz_Click(object sender, EventArgs e)
        {
            var dir = listBoxShowQuizForSelection.SelectedItem as CustomFileObject;
            File.Delete(dir.FilePath);
            listBoxShowQuizForSelection.Items.Remove(listBoxShowQuizForSelection.SelectedItem);
            PanelVisibility.Show(panelQuizStartSelection);
        }

        private void buttonCancelQuizAction_Click(object sender, EventArgs e)
        {
            PanelVisibility.Show(panelQuizStartSelection);
        }
        #endregion

        #endregion

        #region panel settings buttons
        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {

        }

        private void buttonSettingsCancel_Click(object sender, EventArgs e)
        {
            PanelVisibility.Show(panelMainMenu);
        }

        #endregion

        #region Panel Quiz Creation (Add / Remove groups and quizes / set timed / not timed)
        private void buttonQuizCreationCancel_Click(object sender, EventArgs e)
        {
            PanelVisibility.Show(panelMainMenu);
        }
        private void buttonQuizCreationContinue_Click(object sender, EventArgs e)
        {
            if (listBoxQuizGroupsCreationAvaliable.SelectedIndex >= 0)
            {
                _WorkingPath = ((CustomFileObject)listBoxQuizGroupsCreationAvaliable.SelectedItem).FilePath;
            }
            
            PanelVisibility.Show(panelCreate);
        }

        private void buttonAddGroup_Click(object sender, EventArgs e)
        {
           
            var path = Path.Combine(_DefaultPath, textBoxQuizName.Text+@"\");
           
            if (Directory.Exists(path))
            {
                MessageBox.Show("A group with that name already exists!", "File creation error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                Directory.CreateDirectory(path);
                textBoxQuizName.Clear();
            }
        }
        private void buttonRemoveGroup_Click(object sender, EventArgs e)
        {
            if (listBoxQuizGroupsCreationAvaliable.SelectedIndex >= 0)
            {
                var dir = (CustomFileObject)(listBoxQuizGroupsCreationAvaliable.SelectedItem);
                listBoxQuizGroupsCreationAvaliable.Items.RemoveAt(listBoxQuizGroupsCreationAvaliable.SelectedIndex);
                Directory.Delete(dir.FilePath, true);
            }
        }

        #endregion

        #region Quiz Read / Write
        private void CreateQuiz(string path)
        {
            var file = Path.Combine(path, textBoxQuizName.Text+".txt");
            File.Create(file).Close();
            using (var fs = File.AppendText(file))
            {
                if (!String.IsNullOrEmpty(textBoxSecondsPerQuestion.Text))
                {
                    fs.Write($"Delay&{textBoxSecondsPerQuestion.Text}");
                }
                else
                {
                    fs.Write($"Delay&0");
                }
                fs.WriteLine();
                foreach (var s in listBoxPreviewTerm.Items)
                {
                    fs.Write(s);
                    fs.WriteLine();
                }
            }
        }
        int[] correctAnswersArray;
        /// <summary>
        /// Reads data in a quiz including whether or not it's timed
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void ReadQuiz(string path)
        {
            
            List<TermGroup> termGroupList = new List<TermGroup>();
            var file = File.ReadAllLines(path);

            int count = 0;
            foreach (var s in file)
            {
                // could be a do while
                // if count <= 1 (the number of config settings in the file.
                ++count;
                if (count <= 1)
                {
                    var delayTime = s.Split('&');
                    _WorkingTermSet.TimeDelay = int.Parse(delayTime[1]);
                    // settings are split by & 
                }
                else
                {
                    var splitGroup = s.Split('_');
                    termGroupList.Add(new TermGroup()
                    {
                        Term = splitGroup[0],
                        Definition = splitGroup[1]
                    });
                }
           
            }
            correctAnswersArray = new int[termGroupList.Count];
            _WorkingTermSet.TermSetName = Path.GetFileName(path);
            _WorkingTermSet.Terms = termGroupList;
        }
        #endregion

        #region panel active quiz
        private void buttonQuizInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(_WorkingTermSet.ToString());
        }
        int questionLocation = 0, correctAnswers = 0;
        
        private void buttonSubmitAnswer_Click(object sender, EventArgs e)
        {
           

            MessageBox.Show(questionLocation.ToString());
            if (_WorkingTermSetDictionary.ContainsKey(textBoxActiveQuizTerm.Text))
            {
                string val;
                if(_WorkingTermSetDictionary.TryGetValue(textBoxActiveQuizTerm.Text, out val))
                {
                    if(val == textBoxActiveQuizDefinition.Text)
                    {
                        correctAnswers++;
                        correctAnswersArray[questionLocation] = 1;
                    }
                }
            }
            questionLocation++;
            
            textBoxActiveQuizDefinition.Text = _WorkingTermSetDictionary.Values.ElementAtOrDefault(questionLocation);
            if (questionLocation == _WorkingTermSet.Terms.Count)
            {
                labelQuizResultScore.Text = String.Format("{0:F0}", ((correctAnswers / (float)_WorkingTermSet.Terms.Count) * 100) );
                for (int i = 0; i < _WorkingTermSetDictionary.Count; i++)
                {
                    checkedListBoxCorrectAnswers.Items.Add(_WorkingTermSetDictionary.Keys.ElementAtOrDefault(i) +" --> "+ _WorkingTermSetDictionary.Values.ElementAtOrDefault(i));
                    if (correctAnswersArray[i] == 1)
                        checkedListBoxCorrectAnswers.SetItemCheckState(i, CheckState.Checked);
                }
                PanelVisibility.Show(panelGradeResultsMenu);

                _WorkingTermSetDictionary.Clear();
                correctAnswers = 0;
                questionLocation = 0;
                _WorkingPath = _DefaultPath;
                
            }
            // what is this garbage.. (can't think right now)
            var tempQuestionLocation = questionLocation + 1;
            labelQuestionProgression.Text = tempQuestionLocation.ToString() + "/" + _WorkingTermSet.Terms.Count;
            //
            textBoxActiveQuizTerm.Clear();
            
        }

        #region Grade result menu
        private void buttonCloseGradeResultMenu_Click(object sender, EventArgs e)
        {
            PanelVisibility.Show(panelMainMenu);
        }

        #region progress bar for timed quiz activated
        private void backgroundWorkerProgressBar_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = CalculateProgress(sender, e);
        }

        private void backgroundWorkerProgressBar_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorkerProgressBar_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        #endregion
        #endregion


        private object CalculateProgress(object sender, DoWorkEventArgs e)
        {
            int count = 0;
            while(count <= _WorkingTermSet.Terms.Count)
            {
                Thread.Sleep(_WorkingTermSet.TimeDelay);
            }

            return -1;
        }




        // when the panel is showing..
        private void PanelActiveQuiz_VisibleChanged(object sender, EventArgs e)
        {
            if(_WorkingTermSet.TimeDelay > 0)
            {
                progressBarTimedQuiz.Visible = true;
                // start background worker on the progressbar.
            }
            else
            {
                progressBarTimedQuiz.Visible = true;
            }
            if (panelActiveQuiz.Visible)
            {
                labelQuestionProgression.Text = $"1/{_WorkingTermSet.Terms.Count}";
                foreach (var pair in _WorkingTermSet.Terms)
                {
                    _WorkingTermSetDictionary.Add(pair.Term, pair.Definition);
                }

                // show first question in question list
                textBoxActiveQuizDefinition.Text = _WorkingTermSetDictionary.Values.ElementAtOrDefault(0);
               
            }
        }
        #endregion
    }
}
