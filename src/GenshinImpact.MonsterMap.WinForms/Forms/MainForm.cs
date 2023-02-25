using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.Domain;
using GenshinImpact.MonsterMap.Script;
using static GenshinImpact.MonsterMap.Script.InfoModel;

namespace GenshinImpact.MonsterMap.Forms
{
    public partial class MainForm : Form
    {
        private readonly FileSystemBias _bias;
        static bool isMapFormOpen;
        MapForm mapForm;
        
        public MainForm()
        {
            InitializeComponent();
            this.Text = "Genshin Radar Filter v3.0";
            _bias = new FileSystemBias("config/bias.txt");
            Load += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Win32Api.SetProcessDPIAware();
            DataInfo.LoadData();
            var items = DataInfo.GetAllPos.Select(icon => icon.Name).Distinct().ToArray();
            checkedListBox1.Items.AddRange(items);
            DataInfo.sampleImage = pictureSample;
            DataInfo.pointImage = picturePoint;
            //Control map calibration factor
            U0.Text = _bias.PixelPerIng;
            V0.Text = _bias.PixelPerLat;
            U1.Text = _bias.IngBias;
            V1.Text = _bias.LatBias;
            
            InputListenerr.GetMouseEvent((key) =>
            {
                Console.WriteLine(key);
                if (key=="513")
                {
                    DataInfo.isPauseShowIcon = true;
                }
                if (key == "514")
                {
                    DataInfo.isPauseShowIcon = false;
                }
                DataInfo.isDetection = true;
            });
            InputListenerr.GetKeyDownEvent((key) =>
            {
                if (key == "M")
                {
                    if (isMapFormOpen)
                    {
                        btn_Close_Click(null, null);
                    }
                    else
                    {
                        btn_Open_Click(null, null);
                    }
                }
                if (key == "esc") btn_Close_Click(null, null);
                DataInfo.isDetection = true;
            });
        }

        private void btn_Open_Click(object sender, EventArgs e)
        {
            if (DataInfo.GenshinProcess != null || DataInfo.isUseFakePicture)
            {
                isMapFormOpen = true;
                mapForm = new MapForm(_bias);
                mapForm.Show();
            }
            else
            {
                MessageBox.Show("please open the game first");
            }
        }
        private void btn_Close_Click(object sender, EventArgs e)
        {
            if (mapForm != null)
            {
                mapForm.isJumpOutOfTask = true;
                mapForm.Close();
                mapForm.Dispose();
                isMapFormOpen = false;
            }
        }
        private void btn_update_Click(object sender, EventArgs e) => DataInfo.UpdateData();
        private void btn__Boss_Click(object sender, EventArgs e) => Enumerable.Range(0, 8).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
        private void btnMonster_Click(object sender, EventArgs e) => Enumerable.Range(8, 15).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
        private void btn_collection_Click(object sender, EventArgs e) => Enumerable.Range(22, 19).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
        private void btn_All_Click(object sender, EventArgs e) => Enumerable.Range(0, checkedListBox1.Items.Count).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
        private void btn_None_Click(object sender, EventArgs e) => Enumerable.Range(0, checkedListBox1.Items.Count).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, false));
        private void btn_github_Click(object sender, EventArgs e) => Process.Start("https://github.com/red-gezi/GenshinImpact_MonsterMap");
        private void button1_Click(object sender, EventArgs e) => Process.Start("https://wiki.biligame.com/ys/%E5%8E%9F%E7%A5%9E%E5%9C%B0%E5%9B%BE%E5%B7%A5%E5%85%B7_%E5%85%A8%E5%9C%B0%E6%A0%87%E4%BD%8D%E7%BD%AE%E7%82%B9");
        private void btn_SetRect_Click(object sender, EventArgs e)
        {
            DataInfo.width = int.Parse(game_width.Text);
            DataInfo.height = int.Parse(game_height.Text);
        }

        RECT rect = new RECT();
        private void timer1_Tick(object sender, EventArgs e)
        {
            DataInfo.isShowLine = cb_ShowLine.Checked;
            DataInfo.selectTags.Clear();
            foreach (var item in checkedListBox1.CheckedItems)
            {
                DataInfo.selectTags.Add(item.ToString());
            };
            
            if (DataInfo.GenshinProcess != null && cb_AutoLoadScreen.Checked)
            {
                Win32Api.GetClientRect(DataInfo.GenshinProcess.MainWindowHandle, out rect);
                DataInfo.width = rect.Right;
                DataInfo.height = rect.Bottom;
                game_width.Text = rect.Right + "";
                game_height.Text = rect.Bottom + "";
            }
        }

        private void ValueChange(object sender, EventArgs e)
        {
            /* TODO
            if (U0.Value + V0.Value + U1.Value + V1.Value == 0) 
                return;
            Console.WriteLine("Fix mapping parameters");
            
            _bias.PixelPerIng = (float)U0.Value;
            _bias.PixelPerLat = (float)V0.Value;
            _bias.IngBias = (float)U1.Value;
            _bias.LatBias = (float)V1.Value;
            */
        }
    }
}
