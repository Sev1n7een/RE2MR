// ======================================================================
// RE2鼠标复活计划 — 深色游戏驱动风格 UI v2
// ======================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RE2MouseRevive
{
    internal static class Lang
    {
        public const string Zh = "zh", Ja = "ja", En = "en";
        static string cur = Zh;
        public static string Current { get { return cur; } }
        public static void Set(string c) { if (c == Zh || c == Ja || c == En) cur = c; }
        static readonly Dictionary<string, string[]> dict = Build();
        static Dictionary<string, string[]> Build()
        {
            var d = new Dictionary<string, string[]>();
            d.Add("AppTitle", new[] { "RE2鼠标复活计划", "RE2 Mouse Revival", "RE2マウス復活計画" });
            d.Add("Sub", new[] { "右键消抖 · 驱动层", "Right debounce · driver", "右デバウンス・ドライバ" });
            d.Add("Enable", new[] { "启用消抖", "Debounce", "デバウンス" });
            d.Add("DriverOk", new[] { "● 驱动已连接", "● Driver connected", "● ドライバ接続済" });
            d.Add("NeedAdmin", new[] { "● 需要管理员权限", "● Admin required", "● 管理者必要" });
            d.Add("Profile", new[] { "配置文件", "Profile", "プロファイル" });
            d.Add("New", new[] { "新建", "New", "新規" });
            d.Add("Rename", new[] { "重命名", "Rename", "名前変更" });
            d.Add("Delete", new[] { "删除", "Del", "削除" });
            d.Add("Params", new[] { "右键参数", "Right Button", "右ボタン" });
            d.Add("Press", new[] { "按下防抖 P", "Press Debounce P", "押しデバウンス P" });
            d.Add("Release", new[] { "抬起防抖 R", "Release Debounce R", "離しデバウンス R" });
            d.Add("Options", new[] { "选项", "Options", "オプション" });
            d.Add("AutoStart", new[] { "开机自启", "Start at boot", "スタートアップ" });
            d.Add("StartMin", new[] { "启动最小化", "Start minimized", "起動時最小化" });
            d.Add("MinTray", new[] { "关闭到托盘", "Close to tray", "トレイへ" });
            d.Add("OpenCfg", new[] { "配置目录", "Config", "設定" });
            d.Add("Footer", new[] { "内核反作弊网游可能检测此驱动；单机安全", "Kernel AC may detect; single-player safe", "カーネルAC検出の可能性あり。シングルは安全" });
            d.Add("TrayOpen", new[] { "打开主界面", "Open", "開く" });
            d.Add("TrayExit", new[] { "退出", "Exit", "終了" });
            d.Add("Balloon", new[] { "后台运行中", "Running in background", "バックグラウンド実行中" });
            d.Add("NewP", new[] { "新配置名称：", "New profile name:", "新規プロファイル名:" });
            d.Add("RenP", new[] { "重命名为：", "Rename to:", "新しい名前:" });
            d.Add("ConfirmDel", new[] { "删除配置「{0}」？", "Delete \"{0}\"?", "「{0}」を削除？" });
            d.Add("LastOne", new[] { "至少保留一个配置", "Need at least one profile", "プロファイル必須" });
            d.Add("Analyzer", new[] { "点击分析器", "Click Analyzer", "クリック分析" });
            d.Add("StartMeas", new[] { "开始测量", "Start", "計測開始" });
            d.Add("StopMeas", new[] { "停止测量", "Stop", "計測停止" });
            d.Add("Count", new[] { "次数", "Count", "回数" });
            d.Add("Avg", new[] { "平均", "Avg", "平均" });
            d.Add("Min", new[] { "最短", "Min", "最短" });
            d.Add("Max", new[] { "最长", "Max", "最長" });
            d.Add("Gap", new[] { "本次按下时长", "Press duration", "押下時間" });
            d.Add("MeasHint", new[] { "测量中…正常点右键", "Measuring… click right normally", "計測中…右クリック" });
            d.Add("NoData", new[] { "—", "—", "—" });
            d.Add("Stability", new[] { "120帧掉拍打率", "120Hz Drop", "120Hzズレ率" });
            d.Add("Drop60", new[] { "60帧掉拍打率", "60Hz Drop", "60Hzズレ率" });
            d.Add("Appearance", new[] { "外观", "Appearance", "外観" });
            d.Add("ThemeColor", new[] { "主色调", "Accent color", "アクセント" });
            d.Add("FontFamily", new[] { "字体", "Font", "フォント" });
            d.Add("FontSize", new[] { "字号", "Size", "サイズ" });
            d.Add("Apply", new[] { "应用", "Apply", "適用" });
            return d;
        }
        public static string T(string k) { string[] a; return dict.TryGetValue(k, out a) ? a[cur == Zh ? 0 : cur == En ? 1 : 2] : k; }
        public static string T(string k, params object[] args) { return string.Format(T(k), args); }
    }

    [DataContract] internal class Profile { [DataMember] public string Name; [DataMember] public int PressMs; [DataMember] public int ReleaseMs; }
    [DataContract]
    internal class AppConfig
    {
        [DataMember] public bool MasterEnabled { get; set; }
        [DataMember] public bool AutoStart { get; set; }
        [DataMember] public bool StartMinimized { get; set; }
        [DataMember] public bool MinimizeToTray { get; set; }
        [DataMember] public string Language { get; set; }
        [DataMember] public string CurrentProfile { get; set; }
        [DataMember] public int ThemeColor { get; set; }
        [DataMember] public string FontName { get; set; }
        [DataMember] public float FontScale { get; set; }
        [DataMember] public List<Profile> Profiles { get; set; }
        public AppConfig() { MasterEnabled = true; MinimizeToTray = true; Language = Lang.Zh; CurrentProfile = "Default"; ThemeColor = 0x007AFF; FontName = "Segoe UI"; FontScale = 1.0f; Profiles = new List<Profile>(); }
    }

    internal static class ConfigStore
    {
        public static string Dir { get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); } }
        public static string FilePath { get { return System.IO.Path.Combine(Dir, "config.json"); } }
        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var s = new DataContractJsonSerializer(typeof(AppConfig));
                    using (var fs = File.OpenRead(FilePath)) { var c = (AppConfig)s.ReadObject(fs); if (c != null && c.Profiles != null && c.Profiles.Count > 0) return c; }
                }
            }
            catch { }
            var def = new AppConfig(); def.Profiles.Add(new Profile { Name = "Default", PressMs = 4, ReleaseMs = 8 }); return def;
        }
        public static void Save(AppConfig c) { try { Directory.CreateDirectory(Dir); using (var fs = File.Create(FilePath)) new DataContractJsonSerializer(typeof(AppConfig)).WriteObject(fs, c); } catch { } }
    }

    internal static class AutoStart
    {
        const string K = @"Software\Microsoft\Windows\CurrentVersion\Run", N = "RE2MouseRevive";
        public static bool On() { try { using (var k = Registry.CurrentUser.OpenSubKey(K, false)) return k != null && k.GetValue(N) != null; } catch { return false; } }
        public static void Set(bool e, bool min)
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(K, true))
                {
                    if (k == null) return;
                    if (e) { var cmd = Application.ExecutablePath; if (min) cmd += " --minimized"; k.SetValue(N, cmd); }
                    else if (k.GetValue(N) != null) k.DeleteValue(N, false);
                }
            }
            catch { }
        }
    }

    internal interface IScheduler { void Schedule(int ms, TimerCallback cb); void Cancel(); }
    internal sealed class VSched : IScheduler { public long Now; long due = long.MaxValue; TimerCallback cb; public void Schedule(int d, TimerCallback c) { due = Now + d; cb = c; } public void Cancel() { due = long.MaxValue; cb = null; } public void AdvanceTo(long t) { Now = t; if (cb != null && t >= due) { var c = cb; Cancel(); c(null); } } }
    internal sealed class MSched : IScheduler
    {
        delegate void Proc(uint id, uint msg, IntPtr u, IntPtr a, IntPtr b);
        [DllImport("winmm.dll")] static extern uint timeSetEvent(uint d, uint r, Proc cb, IntPtr u, uint f);
        [DllImport("winmm.dll")] static extern uint timeKillEvent(uint id);
        readonly object g = new object(); readonly Proc p; uint t; TimerCallback pend;
        public MSched() { p = new Proc(OnTick); }
        public void Schedule(int ms, TimerCallback cb) { lock (g) { CancelL(); pend = cb; if (ms < 1) ms = 1; t = timeSetEvent((uint)ms, 1, p, IntPtr.Zero, 0); } }
        public void Cancel() { lock (g) CancelL(); }
        void CancelL() { if (t != 0) { timeKillEvent(t); t = 0; } pend = null; }
        void OnTick(uint id, uint m, IntPtr u, IntPtr a, IntPtr b) { TimerCallback c; lock (g) { c = pend; pend = null; t = 0; } if (c != null) try { c(null); } catch { } }
    }

    internal enum St { Up, PD, Down, PU }
    internal sealed class DebounceSM
    {
        readonly object gate = new object(); St state = St.Up; bool lvl;
        readonly IScheduler ps, rs; readonly double t2ms; long dt, ut;
        public Profile Cur; public Action<bool> Inject;
        public DebounceSM(IScheduler p, IScheduler r, double t) { ps = p; rs = r; t2ms = t; }
        public bool OnRaw(bool down, long ts)
        {
            lock (gate)
            {
                if (Cur == null) { state = St.Up; return true; }
                int p = Cur.PressMs, r = Cur.ReleaseMs;
                switch (state)
                {
                    case St.Up: if (down) { lvl = true; dt = ts; if (p <= 0) { state = St.Down; return true; } state = St.PD; ps.Schedule(p, PC); return false; } return true;
                    case St.PD: lvl = down; return false;
                    case St.Down: if (!down) { ut = ts; lvl = false; if (r <= 0) { state = St.Up; return true; } state = St.PU; rs.Schedule(r, RC); } return false;
                    case St.PU: lvl = down; return false;
                }
                return true;
            }
        }
        void PC(object s)
        {
            bool inj = false, rel = false, up = false; int r;
            lock (gate) { if (state != St.PD) return; inj = true; r = Cur.ReleaseMs; if (lvl) state = St.Down; else { if (r <= 0) { state = St.Up; up = true; } else { state = St.PU; rel = true; } } }
            if (inj && Inject != null) Inject(true);
            if (rel) rs.Schedule(r, RC); else if (up && Inject != null) Inject(false);
        }
        void RC(object s)
        {
            bool np = false; int p;
            lock (gate) { if (state != St.PU) return; state = St.Up; if (lvl) { state = St.PD; np = true; } p = Cur.PressMs; }
            if (Inject != null) Inject(false);
            if (np) { if (p <= 0) { if (Inject != null) Inject(true); lock (gate) state = St.Down; } else ps.Schedule(p, PC); }
        }
        public void Flush() { bool u = false; lock (gate) { ps.Cancel(); rs.Cancel(); if (state == St.Down || state == St.PU) u = true; state = St.Up; } if (u && Inject != null) Inject(false); }
        public void Reset() { lock (gate) { ps.Cancel(); rs.Cancel(); state = St.Up; } }
    }

    internal sealed class DriverBackend : IDisposable
    {
        const ushort ALL = 0xFFFF, RD = 0x4, RU = 0x8, MASK = RD | RU;
        [DllImport("interception.dll")] static extern IntPtr interception_create_context();
        [DllImport("interception.dll")] static extern void interception_destroy_context(IntPtr c);
        [DllImport("interception.dll")] static extern void interception_set_filter(IntPtr c, MPred p, ushort f);
        [DllImport("interception.dll")] static extern int interception_wait_with_timeout(IntPtr c, uint ms);
        [DllImport("interception.dll")] static extern int interception_send(IntPtr c, int d, ref MS s, uint n);
        [DllImport("interception.dll")] static extern int interception_receive(IntPtr c, int d, ref MS s, uint n);
        [DllImport("interception.dll")] static extern int interception_is_mouse(int d);
        delegate int MPred(int d);
        [StructLayout(LayoutKind.Sequential)] struct MS { public ushort state, flags; public short roll; public int x, y; public uint info; }
        struct Inj { public bool Down; public int Dev; }
        public readonly DebounceSM SM;
        IntPtr ctx; MPred pred; Thread worker; volatile bool run;
        readonly object qg = new object(); readonly List<Inj> q = new List<Inj>(); int dev = -1;
        public volatile bool Measuring; public event Action<double> OnPressDur;
        long outPressTs; readonly double freq;
        void RecordOut(bool down, long ts) {
            if (!Measuring) return;
            if (down) { outPressTs = ts; }
            else {
                if (outPressTs != 0) {
                    double dur = (ts - outPressTs) * 1000.0 / freq;
                    outPressTs = 0;
                    if (OnPressDur != null) { try { OnPressDur(dur); } catch { } }
                }
            }
        }
        public DriverBackend(AppConfig c)
        {
            SM = new DebounceSM(new MSched(), new MSched(), 1000.0 / Stopwatch.Frequency);
            freq = Stopwatch.Frequency;
            SM.Inject = delegate(bool d) { lock (qg) q.Add(new Inj { Down = d, Dev = dev }); };
            pred = new MPred(interception_is_mouse);
        }
        public void Start() { ctx = interception_create_context(); if (ctx == IntPtr.Zero) throw new Exception("No driver"); interception_set_filter(ctx, pred, ALL); run = true; worker = new Thread(Loop) { IsBackground = true }; worker.Start(); }
        public void SetActive(bool a) { if (!a) SM.Flush(); else SM.Reset(); }
        public void ResetMeas() { outPressTs = 0; }
        void Loop()
        {
            while (run)
            {
                Drain();
                int d = interception_wait_with_timeout(ctx, 2); if (d <= 0) continue;
                MS s = new MS();
                while (interception_receive(ctx, d, ref s, 1) > 0)
                {
                    if ((s.state & MASK) == 0) { interception_send(ctx, d, ref s, 1); continue; }
                    bool kd = true, ku = true;
                    if ((s.state & RD) != 0) {
                        dev = d;
                        long now = Stopwatch.GetTimestamp();
                        if (!SM.OnRaw(true, now)) kd = false;
                    }
                    if ((s.state & RU) != 0) {
                        dev = d;
                        long now = Stopwatch.GetTimestamp();
                        if (!SM.OnRaw(false, now)) ku = false;
                    }
                    if (!kd) s.state = (ushort)(s.state & ~RD);
                    if (!ku) s.state = (ushort)(s.state & ~RU);
                    // 记录最终发给系统的右键事件（消抖后）
                    bool outRD = (s.state & RD) != 0;
                    bool outRU = (s.state & RU) != 0;
                    if (outRD || outRU) {
                        long ts = Stopwatch.GetTimestamp();
                        if (outRD) RecordOut(true, ts);
                        if (outRU) RecordOut(false, ts);
                    }
                    interception_send(ctx, d, ref s, 1);
                }
            }
            Drain();
        }
        void Drain()
        {
            for (; ; ) { Inj it; lock (qg) { if (q.Count == 0) return; it = q[0]; q.RemoveAt(0); } int dv = it.Dev; if (dv < 0) dv = 11; MS s = new MS(); s.state = (ushort)(it.Down ? RD : RU); RecordOut(it.Down, Stopwatch.GetTimestamp()); interception_send(ctx, dv, ref s, 1); }
        }
        public void Dispose() { run = false; try { worker.Join(3000); } catch { } try { SM.Flush(); } catch { } Drain(); if (ctx != IntPtr.Zero) { interception_destroy_context(ctx); ctx = IntPtr.Zero; } }
    }

    // ==============================================================
    // 自绘控件
    // ==============================================================
    internal static class DrawUtil
    {
        public static readonly Color Accent = Color.FromArgb(0, 122, 255);
        public static readonly FontFamily UiFam = new FontFamily("Segoe UI");
        public static float UiScale = 1.0f;
        public static Font F(float s) { return new Font(UiFam, s * UiScale); }
        public static Font F(float s, FontStyle st) { return new Font(UiFam, s * UiScale, st); }
        public static GraphicsPath Rr(Rectangle r, int rad) { int d = rad * 2; var p = new GraphicsPath(); p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90); p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p; }
        public static GraphicsPath RrF(RectangleF r, float rad) { float d = rad * 2; var p = new GraphicsPath(); p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90); p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p; }
    }

    internal sealed class DarkToggle : Control
    {
        bool on;
        public bool On { get { return on; } set { if (on != value) { on = value; Invalidate(); if (Changed != null) Changed(this, EventArgs.Empty); } } }
        public event EventHandler Changed;
        public DarkToggle() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); Size = new Size(52, 28); Cursor = Cursors.Hand; }
        protected override void OnClick(EventArgs e) { On = !On; base.OnClick(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var track = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var br = new SolidBrush(on ? DrawUtil.Accent : Color.FromArgb(45, 45, 55)))
            using (var path = DrawUtil.Rr(track, Height / 2)) g.FillPath(br, path);
            int kd = Height - 8, kx = on ? Width - kd - 4 : 4;
            using (var kb = new SolidBrush(on ? Color.FromArgb(20, 20, 25) : Color.FromArgb(120, 120, 130)))
                g.FillEllipse(kb, kx, 4, kd, kd);
        }
    }

    internal sealed class DarkSlider : Control
    {
        int min, max = 50, val;
        public int Min { get { return min; } set { min = value; Invalidate(); } }
        public int Max { get { return max; } set { max = value; Invalidate(); } }
        public int Value { get { return val; } set { if (value < min) value = min; if (value > max) value = max; if (val != value) { val = value; Invalidate(); if (Changed != null) Changed(this, EventArgs.Empty); } } }
        public event EventHandler Changed;
        public DarkSlider() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); Height = 36; Cursor = Cursors.Hand; }
        protected override void OnMouseDown(MouseEventArgs e) { UpdateFromX(e.X); base.OnMouseDown(e); }
        protected override void OnMouseMove(MouseEventArgs e) { if (e.Button == MouseButtons.Left) UpdateFromX(e.X); base.OnMouseMove(e); }
        void UpdateFromX(int x) { float pad = 10; float t = (x - pad) / (Width - pad * 2); if (t < 0) t = 0; if (t > 1) t = 1; Value = (int)Math.Round(min + t * (max - min)); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            float pad = 10, cy = Height / 2f, h = 8;
            var track = new RectangleF(pad, cy - h / 2, Width - pad * 2, h);
            using (var br = new SolidBrush(Color.FromArgb(38, 38, 48)))
            using (var path = DrawUtil.RrF(track, h / 2)) g.FillPath(br, path);
            float t = max == min ? 0 : (float)(val - min) / (max - min);
            if (t > 0)
            {
                var fill = new RectangleF(pad, cy - h / 2, (Width - pad * 2) * t, h);
                using (var br = new LinearGradientBrush(fill, ControlPaint.Light(DrawUtil.Accent, 0.35f), DrawUtil.Accent, LinearGradientMode.Horizontal))
                using (var path = DrawUtil.RrF(fill, h / 2)) g.FillPath(br, path);
            }
            float kx = pad + (Width - pad * 2) * t, kr = 11;
            g.FillEllipse(Brushes.White, kx - kr, cy - kr, kr * 2, kr * 2);
            g.DrawEllipse(new Pen(Color.FromArgb(20, 20, 25), 2), kx - kr, cy - kr, kr * 2, kr * 2);
        }
    }

    // 圆角按钮
    internal sealed class RoundBtn : Control
    {
        public string BtnText;
        public bool AccentStyle;
        public RoundBtn() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); Cursor = Cursors.Hand; Size = new Size(60, 28); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = DrawUtil.Rr(r, Height / 2))
            using (var br = new SolidBrush(AccentStyle ? DrawUtil.Accent : Color.FromArgb(45, 45, 55)))
            {
                g.FillPath(br, path);
                var tf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var f = DrawUtil.F(9F, AccentStyle ? FontStyle.Bold : FontStyle.Regular))
                using (var tb = new SolidBrush(AccentStyle ? Color.White : Color.FromArgb(220, 220, 228)))
                    g.DrawString(BtnText ?? "", f, tb, r, tf);
            }
        }
    }

    // 胶囊语言按钮组
    internal sealed class LangPillGroup : Control
    {
        public int SelectedIndex;
        public event EventHandler SelectedChanged;
        readonly string[] items = { "中", "EN", "日" };
        public LangPillGroup() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); Size = new Size(120, 30); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            // 底槽
            var bg = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = DrawUtil.Rr(bg, Height / 2))
            using (var br = new SolidBrush(Color.FromArgb(38, 38, 48))) g.FillPath(br, path);
            // 选中滑块
            float sw = Width / 3f;
            var sel = new RectangleF(SelectedIndex * sw + 2, 2, sw - 4, Height - 5);
            using (var path = DrawUtil.RrF(sel, sel.Height / 2))
            using (var br = new SolidBrush(DrawUtil.Accent)) g.FillPath(br, path);
            // 文字
            var tf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            for (int i = 0; i < 3; i++)
            {
                var rr = new RectangleF(i * sw, 0, sw, Height);
                using (var f = DrawUtil.F(8.5F, i == SelectedIndex ? FontStyle.Bold : FontStyle.Regular))
                using (var tb = new SolidBrush(i == SelectedIndex ? Color.White : Color.FromArgb(150, 150, 160)))
                    g.DrawString(items[i], f, tb, rr, tf);
            }
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            float sw = Width / 3f;
            int idx = (int)(e.X / sw);
            if (idx < 0) idx = 0; if (idx > 2) idx = 2;
            if (idx != SelectedIndex) { SelectedIndex = idx; Invalidate(); if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty); }
            base.OnMouseClick(e);
        }
    }

    // 现代化圆角下拉框
    internal sealed class ModernCombo : Control
    {
        public List<string> Items = new List<string>();
        int sel = -1;
        public int SelectedIndex { get { return sel; } set { if (sel != value) { sel = value; Invalidate(); if (SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty); } } }
        public string SelectedItem { get { return sel >= 0 && sel < Items.Count ? Items[sel] : null; } set { int i = Items.IndexOf(value); if (i >= 0) SelectedIndex = i; } }
        public event EventHandler SelectedIndexChanged;
        public ModernCombo() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); Size = new Size(200, 32); Cursor = Cursors.Hand; }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = DrawUtil.Rr(r, 8))
            using (var br = new SolidBrush(Color.FromArgb(38, 38, 48))) g.FillPath(br, path);
            string txt = SelectedItem ?? "";
            using (var f = DrawUtil.F(9.5F))
            using (var tb = new SolidBrush(Color.FromArgb(230, 230, 235)))
                g.DrawString(txt, f, tb, new RectangleF(12, 0, Width - 40, Height), new StringFormat { LineAlignment = StringAlignment.Center });
            // 下拉箭头
            using (var pen = new Pen(Color.FromArgb(150, 150, 160), 1.5F))
            {
                int ax = Width - 20, ay = Height / 2 - 2;
                g.DrawLine(pen, ax - 4, ay, ax, ay + 4);
                g.DrawLine(pen, ax + 4, ay, ax, ay + 4);
            }
        }
        protected override void OnClick(EventArgs e)
        {
            var menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(38, 38, 48);
            menu.ForeColor = Color.FromArgb(230, 230, 235);
            menu.Font = DrawUtil.F(9.5F);
            foreach (var item in Items)
            {
                var mi = new ToolStripMenuItem(item);
                mi.Click += delegate { SelectedItem = mi.Text; };
                if (item == SelectedItem) mi.BackColor = DrawUtil.Accent;
                menu.Items.Add(mi);
            }
            menu.Show(this, new Point(0, Height));
            base.OnClick(e);
        }
    }

    internal sealed class Card : Panel
    {
        public Card() { BackColor = Color.FromArgb(26, 26, 32); SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); Resize += delegate { using (var p = DrawUtil.Rr(new Rectangle(0, 0, Width, Height), 14)) Region = new Region(p); }; }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var br = new SolidBrush(Color.FromArgb(26, 26, 32)))
            using (var path = DrawUtil.Rr(r, 14)) { g.FillPath(br, path); g.DrawPath(new Pen(Color.FromArgb(40, 40, 50)), path); }
        }
    }

    // ==============================================================
    internal sealed class MainForm : Form
    {
        const int Max = 50;
        static readonly Color Bg = Color.FromArgb(15, 15, 19);
        static readonly Color TextP = Color.FromArgb(235, 235, 240);
        static readonly Color TextS = Color.FromArgb(130, 130, 145);

        readonly AppConfig cfg; readonly DriverBackend be; readonly bool startMin;
        NotifyIcon tray; ToolStripMenuItem trayTog;
        DarkToggle masterTog, autoTog, minTog, optTrayTog;
        ModernCombo profileCmb;
        RoundBtn btnNew, btnRen, btnDel;
        [DllImport("winmm.dll")] static extern long mciSendString(string cmd, StringBuilder ret, int retLen, IntPtr hwnd);
        int konamiIdx = 0;
        static readonly Keys[] Konami = { Keys.Up, Keys.Up, Keys.Down, Keys.Down, Keys.Left, Keys.Right, Keys.Left, Keys.Right, Keys.B, Keys.A, Keys.B, Keys.A };
        LangPillGroup langPill;
        DarkSlider pressSlider, relSlider;
        Label titleLbl, subLbl, statusLbl, pressLbl, relLbl, pressVal, relVal, autoLbl, minLbl, trayLbl;
        Card masterCard, profCard, paramCard, optCard, measCard;
        RoundBtn measBtn;
        Label measLastLbl, measLastVal, measCountVal, measAvgVal, measMinVal, measMaxVal, measStabLbl, measStabVal, measStab60Lbl, measStab60Val;
        int mCount; double mSum, mSumSq, mMin, mMax, mDropSum, mDrop60Sum; bool mOn;
        bool suppress, realExit;

        public MainForm(AppConfig c, DriverBackend b, bool sm)
        {
            cfg = c; be = b; startMin = sm; Lang.Set(c.Language);
            SM.Cur = FindProf(c.CurrentProfile);
            Text = Lang.T("AppTitle");
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = DrawUtil.F(9F);
            ClientSize = new Size(700, 540);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
            BackColor = Bg; ForeColor = TextP; Icon = LoadIcon();
            KeyPreview = true;
            be.OnPressDur += OnMeasDur;
            Build(); LoadCfg(); BuildTray(); ApplyLang();
        }

        DebounceSM SM { get { return be.SM; } }
        Profile CurProf { get { return SM.Cur; } }
        Profile FindProf(string n) { foreach (var p in cfg.Profiles) if (p.Name == n) return p; if (cfg.Profiles.Count > 0) return cfg.Profiles[0]; var np = new Profile { Name = "Default", PressMs = 4, ReleaseMs = 8 }; cfg.Profiles.Add(np); return np; }

        void Build()
        {
            titleLbl = new Label { Font = DrawUtil.F(15F, FontStyle.Bold), ForeColor = TextP, AutoSize = true, Location = new Point(24, 22), BackColor = Bg };
            Controls.Add(titleLbl);
            subLbl = new Label { Font = DrawUtil.F(8.5F), ForeColor = TextS, AutoSize = true, Location = new Point(26, 52), BackColor = Bg };
            Controls.Add(subLbl);
            langPill = new LangPillGroup { Location = new Point(560, 22) };
            langPill.SelectedChanged += delegate
            {
                if (suppress) return;
                string code = langPill.SelectedIndex == 0 ? Lang.Zh : langPill.SelectedIndex == 1 ? Lang.En : Lang.Ja;
                cfg.Language = code; ConfigStore.Save(cfg); Lang.Set(code); ApplyLang(); RefreshProfiles();
            };
            Controls.Add(langPill);

            statusLbl = new Label { AutoSize = true, Location = new Point(26, 80), ForeColor = Color.FromArgb(80, 200, 120), BackColor = Bg, Font = DrawUtil.F(9F) };
            Controls.Add(statusLbl);

            masterCard = new Card { Location = new Point(20, 104), Size = new Size(320, 60), Anchor = AnchorStyles.Top | AnchorStyles.Left };
            Controls.Add(masterCard);
            var ml = new Label { Font = DrawUtil.F(11F, FontStyle.Bold), ForeColor = TextP, AutoSize = true, Location = new Point(20, 20), BackColor = Color.FromArgb(26, 26, 32) };
            masterCard.Controls.Add(ml);
            masterTog = new DarkToggle { Location = new Point(256, 17) };
            masterTog.Changed += delegate { if (suppress) return; cfg.MasterEnabled = masterTog.On; be.SetActive(masterTog.On); if (trayTog != null) trayTog.Checked = masterTog.On; ConfigStore.Save(cfg); };
            masterCard.Controls.Add(masterTog);

            profCard = new Card { Location = new Point(360, 104), Size = new Size(320, 60), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(profCard);
            profCard.Controls.Add(CardTitle(Lang.T("Profile"), 12, 8));
            profileCmb = new ModernCombo { Location = new Point(12, 30), Size = new Size(140, 24) };
            profileCmb.SelectedIndexChanged += delegate { if (suppress || profileCmb.SelectedItem == null) return; cfg.CurrentProfile = profileCmb.SelectedItem; SM.Cur = FindProf(cfg.CurrentProfile); LoadProf(); ConfigStore.Save(cfg); };
            profCard.Controls.Add(profileCmb);
            btnNew = new RoundBtn { Location = new Point(158, 31), Size = new Size(48, 24) }; btnNew.Click += delegate { NewProf(); }; profCard.Controls.Add(btnNew);
            btnRen = new RoundBtn { Location = new Point(210, 31), Size = new Size(48, 24) }; btnRen.Click += delegate { RenProf(); }; profCard.Controls.Add(btnRen);
            btnDel = new RoundBtn { Location = new Point(262, 31), Size = new Size(48, 24) }; btnDel.Click += delegate { DelProf(); }; profCard.Controls.Add(btnDel);

            paramCard = new Card { Location = new Point(20, 176), Size = new Size(660, 140), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(paramCard);
            paramCard.Controls.Add(CardTitle(Lang.T("Params"), 20, 10));

            pressLbl = new Label { Location = new Point(20, 34), Size = new Size(200, 18), ForeColor = TextS, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(9F) };
            paramCard.Controls.Add(pressLbl);
            pressVal = new Label { Location = new Point(560, 32), Size = new Size(80, 22), ForeColor = DrawUtil.Accent, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(12F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            paramCard.Controls.Add(pressVal);
            pressSlider = new DarkSlider { Location = new Point(20, 56), Size = new Size(620, 26), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pressSlider.Changed += delegate { pressVal.Text = pressSlider.Value + " ms"; if (!suppress) { CurProf.PressMs = pressSlider.Value; ConfigStore.Save(cfg); } };
            paramCard.Controls.Add(pressSlider);

            relLbl = new Label { Location = new Point(20, 90), Size = new Size(200, 18), ForeColor = TextS, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(9F) };
            paramCard.Controls.Add(relLbl);
            relVal = new Label { Location = new Point(560, 88), Size = new Size(80, 22), ForeColor = DrawUtil.Accent, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(12F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            paramCard.Controls.Add(relVal);
            relSlider = new DarkSlider { Location = new Point(20, 110), Size = new Size(620, 26), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            relSlider.Changed += delegate { relVal.Text = relSlider.Value + " ms"; if (!suppress) { CurProf.ReleaseMs = relSlider.Value; ConfigStore.Save(cfg); } };
            paramCard.Controls.Add(relSlider);

            // 点击分析器卡片
            measCard = new Card { Location = new Point(20, 332), Size = new Size(320, 180), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            Controls.Add(measCard);
            measCard.Controls.Add(CardTitle(Lang.T("Analyzer"), 12, 10));

            measBtn = new RoundBtn { Location = new Point(200, 8), Size = new Size(100, 24), AccentStyle = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            measBtn.Click += delegate { ToggleMeas(); };
            measCard.Controls.Add(measBtn);

            measLastLbl = new Label { Location = new Point(12, 40), Size = new Size(90, 18), ForeColor = TextS, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(9F) };
            measCard.Controls.Add(measLastLbl);
            measLastVal = new Label { Location = new Point(100, 34), Size = new Size(208, 26), ForeColor = DrawUtil.Accent, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            measCard.Controls.Add(measLastVal);

            measCountVal = MeasStat(12, 70, Lang.T("Count")); measCard.Controls.Add(measCountVal);
            measAvgVal = MeasStat(88, 70, Lang.T("Avg"));   measCard.Controls.Add(measAvgVal);
            measMinVal  = MeasStat(164, 70, Lang.T("Min"));   measCard.Controls.Add(measMinVal);
            measMaxVal  = MeasStat(240, 70, Lang.T("Max"));   measCard.Controls.Add(measMaxVal);

            measStabLbl = new Label { Location = new Point(12, 108), Size = new Size(90, 18), ForeColor = TextS, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(9F) };
            measCard.Controls.Add(measStabLbl);
            measStabVal = new Label { Location = new Point(100, 102), Size = new Size(208, 24), ForeColor = DrawUtil.Accent, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(12F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            measCard.Controls.Add(measStabVal);

            measStab60Lbl = new Label { Location = new Point(12, 138), Size = new Size(90, 18), ForeColor = TextS, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(9F) };
            measCard.Controls.Add(measStab60Lbl);
            measStab60Val = new Label { Location = new Point(100, 132), Size = new Size(208, 24), ForeColor = DrawUtil.Accent, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(12F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            measCard.Controls.Add(measStab60Val);
            measBtn.BtnText = Lang.T("StartMeas"); measBtn.AccentStyle = true;
            measLastLbl.Text = Lang.T("Gap");
            measStabLbl.Text = Lang.T("Stability");
            measStab60Lbl.Text = Lang.T("Drop60");
            measLastVal.Text = "—";
            measCountVal.Text = Lang.T("Count") + "\n0";
            measAvgVal.Text = Lang.T("Avg") + "\n—";
            measMinVal.Text = Lang.T("Min") + "\n—";
            measMaxVal.Text = Lang.T("Max") + "\n—";

            optCard = new Card { Location = new Point(360, 332), Size = new Size(320, 180), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            Controls.Add(optCard);
            optCard.Controls.Add(CardTitle(Lang.T("Options"), 12, 10));

            autoLbl = OptLabel(12, 42); optCard.Controls.Add(autoLbl);
            autoTog = new DarkToggle { Location = new Point(256, 38), Size = new Size(48, 26) };
            autoTog.Changed += delegate { if (suppress) return; cfg.AutoStart = autoTog.On; AutoStart.Set(autoTog.On, minTog.On); ConfigStore.Save(cfg); };
            optCard.Controls.Add(autoTog);

            minLbl = OptLabel(12, 80); optCard.Controls.Add(minLbl);
            minTog = new DarkToggle { Location = new Point(256, 76), Size = new Size(48, 26) };
            minTog.Changed += delegate { if (suppress) return; cfg.StartMinimized = minTog.On; if (autoTog.On) AutoStart.Set(true, minTog.On); ConfigStore.Save(cfg); };
            optCard.Controls.Add(minTog);

            trayLbl = OptLabel(12, 118); optCard.Controls.Add(trayLbl);
            optTrayTog = new DarkToggle { Location = new Point(256, 114), Size = new Size(48, 26) };
            optTrayTog.Changed += delegate { if (suppress) return; cfg.MinimizeToTray = optTrayTog.On; ConfigStore.Save(cfg); };
            optCard.Controls.Add(optTrayTog);
        }

        static Label CardTitle(string t, int x, int y) { return new Label { Text = t, Font = DrawUtil.F(8.5F, FontStyle.Bold), ForeColor = TextS, AutoSize = true, Location = new Point(x, y), BackColor = Color.FromArgb(26, 26, 32) }; }
        static Label OptLabel(int x, int y) { return new Label { Location = new Point(x, y), Size = new Size(240, 22), ForeColor = TextS, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(9F), TextAlign = ContentAlignment.MiddleLeft }; }
        static Label MeasStat(int x, int y, string label) { return new Label { Location = new Point(x, y), Size = new Size(78, 32), ForeColor = TextP, BackColor = Color.FromArgb(26, 26, 32), Font = DrawUtil.F(8.5F), TextAlign = ContentAlignment.TopLeft }; }

        void RefreshProfiles()
        {
            suppress = true;
            string cur = profileCmb.SelectedItem;
            profileCmb.Items.Clear(); foreach (var p in cfg.Profiles) profileCmb.Items.Add(p.Name);
            if (cur != null && profileCmb.Items.Contains(cur)) profileCmb.SelectedItem = cur;
            else if (profileCmb.Items.Count > 0) profileCmb.SelectedIndex = 0;
            suppress = false;
        }
        void NewProf()
        {
            var n = InBox(Lang.T("NewP"), Lang.T("New"), "Default " + (cfg.Profiles.Count + 1));
            if (string.IsNullOrWhiteSpace(n)) return; n = n.Trim();
            foreach (var p in cfg.Profiles) if (p.Name == n) return;
            var np = new Profile { Name = n, PressMs = CurProf.PressMs, ReleaseMs = CurProf.ReleaseMs };
            cfg.Profiles.Add(np); cfg.CurrentProfile = n; SM.Cur = np;
            RefreshProfiles(); profileCmb.SelectedItem = n; LoadProf(); ConfigStore.Save(cfg);
        }
        void RenProf()
        {
            if (profileCmb.SelectedItem == null) return; var old = profileCmb.SelectedItem.ToString();
            var n = InBox(Lang.T("RenP"), Lang.T("Rename"), old);
            if (string.IsNullOrWhiteSpace(n)) return; n = n.Trim();
            foreach (var p in cfg.Profiles) if (p.Name == n && p.Name != old) return;
            FindProf(old).Name = n; cfg.CurrentProfile = n;
            RefreshProfiles(); profileCmb.SelectedItem = n; ConfigStore.Save(cfg);
        }
        void DelProf()
        {
            if (cfg.Profiles.Count <= 1) { MessageBox.Show(Lang.T("LastOne")); return; }
            if (profileCmb.SelectedItem == null) return; var n = profileCmb.SelectedItem.ToString();
            if (MessageBox.Show(Lang.T("ConfirmDel", n), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            var p = FindProf(n); cfg.Profiles.Remove(p);
            if (SM.Cur == p) { SM.Cur = cfg.Profiles[0]; cfg.CurrentProfile = cfg.Profiles[0].Name; }
            RefreshProfiles(); LoadProf(); ConfigStore.Save(cfg);
        }
        void LoadProf()
        {
            suppress = true;
            pressSlider.Value = Clamp(CurProf.PressMs); relSlider.Value = Clamp(CurProf.ReleaseMs);
            pressVal.Text = pressSlider.Value + " ms"; relVal.Text = relSlider.Value + " ms";
            suppress = false;
        }
        void ToggleMeas()
        {
            mOn = !mOn;
            be.Measuring = mOn;
            if (mOn) { be.ResetMeas(); ResetMeasUI(); }
            measBtn.BtnText = Lang.T(mOn ? "StopMeas" : "StartMeas");
            measBtn.AccentStyle = !mOn;
            measBtn.Invalidate();
        }
        void ResetMeasUI()
        {
            mCount = 0; mSum = 0; mSumSq = 0; mMin = 0; mMax = 0; mDropSum = 0; mDrop60Sum = 0;
            measLastVal.Text = "—";
            measStabVal.Text = "—"; measStabVal.ForeColor = DrawUtil.Accent;
            measStab60Val.Text = "—";
            measCountVal.Text = Lang.T("Count") + "\n0";
            measAvgVal.Text = Lang.T("Avg") + "\n—";
            measMinVal.Text = Lang.T("Min") + "\n—";
            measMaxVal.Text = Lang.T("Max") + "\n—";
        }
        void OnMeasDur(double dur)
        {
            if (IsDisposed) return;
            try {
                Invoke((MethodInvoker)delegate {
                    mCount++; mSum += dur; mSumSq += dur * dur;
                    if (mCount == 1 || dur < mMin) mMin = dur;
                    if (dur > mMax) mMax = dur;
                    measLastVal.Text = dur.ToString("0.00") + " ms";
                    measCountVal.Text = Lang.T("Count") + "\n" + mCount;
                    measAvgVal.Text = Lang.T("Avg") + "\n" + (mSum / mCount).ToString("0.00") + " ms";
                    measMinVal.Text = Lang.T("Min") + "\n" + mMin.ToString("0.00") + " ms";
                    measMaxVal.Text = Lang.T("Max") + "\n" + mMax.ToString("0.00") + " ms";
                    // 掉拍打率：单次 = max(0, 1 - dur/tick)，总体 = 平均
                    {
                        double one120 = 1.0 - dur / 8.33; if (one120 < 0) one120 = 0;
                        mDropSum += one120;
                        measStabVal.Text = (mDropSum / mCount * 100).ToString("0.0") + "%";
                        double one60 = 1.0 - dur / 16.66; if (one60 < 0) one60 = 0;
                        mDrop60Sum += one60;
                        measStab60Val.Text = (mDrop60Sum / mCount * 100).ToString("0.0") + "%";
                    }
                });
            } catch { }
        }
        void LoadCfg()
        {
            suppress = true;
            masterTog.On = cfg.MasterEnabled;
            autoTog.On = AutoStart.On() || cfg.AutoStart;
            minTog.On = cfg.StartMinimized;
            optTrayTog.On = cfg.MinimizeToTray;
            RefreshProfiles(); profileCmb.SelectedItem = cfg.CurrentProfile;
            if (profileCmb.SelectedItem == null && profileCmb.Items.Count > 0) profileCmb.SelectedIndex = 0;
            LoadProf();
            suppress = false;
        }
        static int Clamp(int v) { return v < 0 ? 0 : v > Max ? Max : v; }

        void ApplyLang()
        {
            suppress = true;
            Text = Lang.T("AppTitle");
            titleLbl.Text = Lang.T("AppTitle"); subLbl.Text = Lang.T("Sub");
            statusLbl.Text = IsAdmin() ? Lang.T("DriverOk") : Lang.T("NeedAdmin");
            statusLbl.ForeColor = IsAdmin() ? Color.FromArgb(80, 200, 120) : Color.FromArgb(230, 100, 100);
            ((Label)masterCard.Controls[0]).Text = Lang.T("Enable");
            ((Label)profCard.Controls[0]).Text = Lang.T("Profile");
            ((Label)paramCard.Controls[0]).Text = Lang.T("Params");
            ((Label)optCard.Controls[0]).Text = Lang.T("Options");
            ((Label)measCard.Controls[0]).Text = Lang.T("Analyzer");
            pressLbl.Text = Lang.T("Press"); relLbl.Text = Lang.T("Release");
            autoLbl.Text = Lang.T("AutoStart"); minLbl.Text = Lang.T("StartMin"); trayLbl.Text = Lang.T("MinTray");
            measLastLbl.Text = Lang.T("Gap");
            measStabLbl.Text = Lang.T("Stability");
            measStab60Lbl.Text = Lang.T("Drop60");
            measBtn.BtnText = Lang.T(mOn ? "StopMeas" : "StartMeas"); measBtn.Invalidate();
            measCountVal.Text = Lang.T("Count") + "\n" + mCount;
            measAvgVal.Text = Lang.T("Avg") + "\n" + (mCount > 0 ? (mSum / mCount).ToString("0.00") + " ms" : "—");
            measMinVal.Text = Lang.T("Min") + "\n" + (mCount > 0 ? mMin.ToString("0.00") + " ms" : "—");
            measMaxVal.Text = Lang.T("Max") + "\n" + (mCount > 0 ? mMax.ToString("0.00") + " ms" : "—");
            btnNew.BtnText = Lang.T("New"); btnRen.BtnText = Lang.T("Rename"); btnDel.BtnText = Lang.T("Delete");
            btnNew.Invalidate(); btnRen.Invalidate(); btnDel.Invalidate();
            langPill.SelectedIndex = Lang.Current == Lang.Zh ? 0 : Lang.Current == Lang.En ? 1 : 2;
            if (trayTog != null) { trayTog.Text = Lang.T("Enable"); tray.Text = Lang.T("AppTitle"); }
            suppress = false;
        }

        void BuildTray()
        {
            var m = new ContextMenuStrip();
            trayTog = new ToolStripMenuItem { Checked = cfg.MasterEnabled, CheckOnClick = true };
            trayTog.CheckedChanged += delegate { if (suppress) return; suppress = true; masterTog.On = trayTog.Checked; suppress = false; cfg.MasterEnabled = trayTog.Checked; be.SetActive(trayTog.Checked); ConfigStore.Save(cfg); };
            m.Items.Add(trayTog);
            var s = new ToolStripMenuItem(Lang.T("TrayOpen")); s.Click += delegate { ShowWin(); }; m.Items.Add(s);
            m.Items.Add(new ToolStripSeparator());
            var e = new ToolStripMenuItem(Lang.T("TrayExit")); e.Click += delegate { ExitApp(); }; m.Items.Add(e);
            tray = new NotifyIcon { Icon = Icon, Text = Lang.T("AppTitle"), ContextMenuStrip = m, Visible = true };
            tray.DoubleClick += delegate { ShowWin(); };
        }
        void ShowWin() { Show(); WindowState = FormWindowState.Normal; ShowInTaskbar = true; Activate(); }
        void ExitApp() { realExit = true; if (tray != null) tray.Visible = false; Application.Exit(); }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }
        protected override void OnFormClosing(FormClosingEventArgs e) { if (!realExit && cfg.MinimizeToTray && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); ShowInTaskbar = false; tray.ShowBalloonTip(1200, Lang.T("AppTitle"), Lang.T("Balloon"), ToolTipIcon.Info); return; } base.OnFormClosing(e); }
        protected override void OnLoad(EventArgs e) { base.OnLoad(e); if (startMin) { Hide(); ShowInTaskbar = false; } }
        protected override void Dispose(bool d) { if (d) { try { be.Dispose(); } catch { } try { tray.Dispose(); } catch { } } base.Dispose(d); }
        static bool IsAdmin() { try { using (var id = WindowsIdentity.GetCurrent()) return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator); } catch { return false; } }
        Icon LoadIcon() { try { var p = System.IO.Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "app.ico"); if (File.Exists(p)) return new Icon(p); } catch { } return SystemIcons.Application; }

        internal static string InBox(string prompt, string title, string def)
        {
            var f = new Form { Width = 380, Height = 160, Text = title, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.FromArgb(26, 26, 32), ForeColor = TextP };
            var l = new Label { Left = 16, Top = 16, Width = 340, Text = prompt, ForeColor = TextP };
            var t = new TextBox { Left = 16, Top = 44, Width = 340, Text = def, BackColor = Color.FromArgb(38, 38, 48), ForeColor = TextP, BorderStyle = BorderStyle.FixedSingle };
            var ok = new Button { Text = "OK", Left = 200, Width = 70, Top = 80, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(45, 45, 55), ForeColor = TextP };
            var cn = new Button { Text = "Cancel", Left = 280, Width = 70, Top = 80, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(45, 45, 55), ForeColor = TextP };
            f.Controls.AddRange(new Control[] { l, t, ok, cn }); f.AcceptButton = ok; f.CancelButton = cn;
            return f.ShowDialog() == DialogResult.OK ? t.Text : null;
        }
    }

    internal sealed class KonamiFilter : IMessageFilter
    {
        int idx = 0;
        static readonly Keys[] Seq = { Keys.Up, Keys.Up, Keys.Down, Keys.Down, Keys.Left, Keys.Right, Keys.Left, Keys.Right, Keys.B, Keys.A, Keys.B, Keys.A };
        [DllImport("winmm.dll")] static extern long mciSendString(string cmd, StringBuilder ret, int retLen, IntPtr hwnd);
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x0100) { // WM_KEYDOWN
                Keys k = (Keys)m.WParam & Keys.KeyCode;
                if (k == Seq[idx]) { idx++; if (idx >= Seq.Length) { idx = 0; try { string mp3 = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "木子魔了.mp3"); mciSendString("close zimu", null, 0, IntPtr.Zero); mciSendString("open \"" + mp3 + "\" type mpegvideo alias zimu", null, 0, IntPtr.Zero); mciSendString("play zimu", null, 0, IntPtr.Zero); } catch { } } }
                else idx = (k == Seq[0]) ? 1 : 0;
            }
            return false;
        }
    }
    internal static class Pgm
    {
        [STAThread]
        static int Main(string[] a)
        {
            bool min = false; foreach (string x in a) if (x == "--minimized") min = true;
            bool cn = true; Mutex m = null; try { m = new Mutex(true, "Local\\RE2MR4", out cn); } catch { cn = true; } if (!cn) return 0;
            var c = ConfigStore.Load(); var b = new DriverBackend(c);
            try { b.Start(); } catch (Exception ex) { MessageBox.Show("Error:\n" + ex.Message, "RE2", MessageBoxButtons.OK, MessageBoxIcon.Warning); return 1; }
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new KonamiFilter());
            try { Application.Run(new MainForm(c, b, min)); } catch (Exception ex) { System.IO.File.WriteAllText("crash.txt", ex.ToString()); MessageBox.Show("Error:\n" + ex, "RE2", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            GC.KeepAlive(m); return 0;
        }
    }
}
