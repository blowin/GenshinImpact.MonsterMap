using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.Domain;
using GenshinImpact.MonsterMap.Domain.Api.Loaders;
using GenshinImpact.MonsterMap.Domain.Icons;
using GenshinImpact.MonsterMap.Script;
using SharpHook;
using SharpHook.Native;

namespace GenshinImpact.MonsterMap.Forms;

public partial class MainForm : Form
{
    private readonly FileSystemBias _bias;
    private readonly TaskPoolGlobalHook _hooks;
    private readonly IconPositionProvider _iconPositionProvider;
    private MapForm _mapForm;
    
    // required!
    private RECT rect = new();

    public MainForm()
    {
        InitializeComponent();
        Text = "Genshin Radar Filter v3.0";
        _hooks = new TaskPoolGlobalHook();
        _bias = new FileSystemBias("config/bias.txt");
        _iconPositionProvider = new IconPositionProvider(new PreparedApiDataLoader(), "config/IconPosition.txt");
        _hooks.MousePressed += HooksOnMousePressed;
        _hooks.MouseReleased += HooksOnMouseReleased;
        _hooks.KeyPressed += HooksOnKeyPressed;
        Load += OnLoad;
        Closed += OnClosed;
    }

    private void OnClosed(object sender, EventArgs e)
    {
        _hooks.Dispose();
        btn_Close_Click(this, EventArgs.Empty);
    }

    private void HooksOnKeyPressed(object sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode == KeyCode.VcM)
        {
            if (_mapForm != null)
            {
                btn_Close_Click(null, null);
            }
            else
            {
                btn_Open_Click(null, null);
            }
        }

        if (e.Data.KeyCode == KeyCode.VcEscape)
        {
            btn_Close_Click(null, null);
        }
        DataInfo.isDetection = true;
    }

    private void HooksOnMousePressed(object sender, MouseHookEventArgs e)
    {
        DataInfo.isPauseShowIcon = true;
        DataInfo.isDetection = true;
    }

    private void HooksOnMouseReleased(object sender, MouseHookEventArgs e)
    {
        DataInfo.isPauseShowIcon = false;
        DataInfo.isDetection = true;
    }
        
    private void OnLoad(object sender, EventArgs e)
    {
        _hooks.RunAsync();
        Win32Api.SetProcessDPIAware();
        DataInfo.LoadData();
        var items = _iconPositionProvider.GetIconNames();
        checkedListBox1.Items.AddRange(items);
        DataInfo.sampleImage = pictureSample;
        DataInfo.pointImage = picturePoint;
        //Control map calibration factor
        U0.Text = _bias.PixelPerIng;
        V0.Text = _bias.PixelPerLat;
        U1.Text = _bias.IngBias;
        V1.Text = _bias.LatBias;
    }

    private void OpenUrl(string url)
    {
        using var process = new Process();
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = url;
        process.Start();
    }

    private void btn_Open_Click(object sender, EventArgs e)
    {
        if (DataInfo.GenshinProcess != null)
        {
            if(_mapForm != null)
                return;
            
            _mapForm = new MapForm(_bias, _iconPositionProvider);
            _mapForm.Show();
        }
        else
        {
            MessageBox.Show("please open the game first");
        }
    }
    private void btn_Close_Click(object sender, EventArgs e)
    {
        if (_mapForm == null) 
            return;
        
        _mapForm.Close();
        _mapForm.Dispose();
        _mapForm = null;
    }
    
    private void btn_update_Click(object sender, EventArgs e) => _iconPositionProvider.UpdateData();
    private void btn__Boss_Click(object sender, EventArgs e) => Enumerable.Range(0, 8).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
    private void btnMonster_Click(object sender, EventArgs e) => Enumerable.Range(8, 15).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
    private void btn_collection_Click(object sender, EventArgs e) => Enumerable.Range(22, 19).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
    private void btn_All_Click(object sender, EventArgs e) => Enumerable.Range(0, checkedListBox1.Items.Count).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, true));
    private void btn_None_Click(object sender, EventArgs e) => Enumerable.Range(0, checkedListBox1.Items.Count).ToList().ForEach(num => checkedListBox1.SetItemChecked(num, false));

    private void btn_github_Click(object sender, EventArgs e) => OpenUrl("https://github.com/blowin/GenshinImpact.MonsterMap");
    private void button1_Click(object sender, EventArgs e) => OpenUrl("https://wiki.biligame.com/ys/%E5%8E%9F%E7%A5%9E%E5%9C%B0%E5%9B%BE%E5%B7%A5%E5%85%B7_%E5%85%A8%E5%9C%B0%E6%A0%87%E4%BD%8D%E7%BD%AE%E7%82%B9");
    private void btn_SetRect_Click(object sender, EventArgs e)
    {
        DataInfo.width = int.Parse(game_width.Text);
        DataInfo.height = int.Parse(game_height.Text);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        DataInfo.isShowLine = cb_ShowLine.Checked;
        DataInfo.selectTags.Clear();
        foreach (var item in checkedListBox1.CheckedItems)
        {
            DataInfo.selectTags.Add(item.ToString());
        };

        var handle = DataInfo.mainHandle ?? IntPtr.Zero;
        if (handle != IntPtr.Zero && cb_AutoLoadScreen.Checked)
        {
            Win32Api.GetClientRect(handle, out rect);
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