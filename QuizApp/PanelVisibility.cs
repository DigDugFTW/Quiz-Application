using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuizApp
{
    struct PanelVisibility
    {

        private static List<Panel> _panelList = new List<Panel>();

        //private PanelVisibility() { }

        // initialize the panel list to reference
        public static List<Panel> PanelList
        {
            set
            {
                _panelList = value;
            }
            get
            {
                return _panelList;
            }
        }

        // get panel list count
        public static int Count
        {
            get
            {
                return _panelList.Count;
            }
        }

        // show the specified panel from the panel list overload for panel
        public static void Show(Panel panel)
        {
            foreach (var p in _panelList)
                p.Hide();

            panel.Show();
        }
        public static void Show(params Panel[] panels)
        {
            foreach (var p in _panelList)
                p.Hide();
            foreach (var p in panels)
                p.Show();
        }
        public static void ShowWith(Panel panelBackground, Panel panelForeground)
        {
            foreach (var p in _panelList)
                p.Hide();

            panelBackground.Show();
            panelForeground.Show();
            panelForeground.BringToFront();
        }

        // show the specified panel from the panel list overload for index
        public static void Show(int index)
        {
            foreach (var p in _panelList)
                p.Hide();

            _panelList[index].Show();
        }

        // add panel to panel list
        public static void Add(Panel panel)
        {
            _panelList.Add(panel);
        }

        // remove panel from panel list
        public static void Remove(Panel panel)
        {
            _panelList.Remove(panel);
        }
    }
}
