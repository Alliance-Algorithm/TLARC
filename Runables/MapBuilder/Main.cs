using System;
using Gtk;
using Gdk;
using Window = Gtk.Window;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using System.Runtime.InteropServices;

class ThreeColumnApp
{
    // 原始和处理后 Pixbuf 缓存
    static Pixbuf? originalPixbuf;
    static Pixbuf? processedPixbuf;
    static Emgu.CV.Mat? originalMat;
    static Emgu.CV.Mat? processedMat;

    // 中心和右侧展示控件与容器
    static Image? centerImage;
    static Image? rightImage;
    static EventBox? centerContainer;
    static EventBox? rightContainer;

    // 平移 / 缩放 状态
    static int zoomCenter = 100, offsetXCenter = 0, offsetYCenter = 0;
    static int zoomRight = 100, offsetXRight = 0, offsetYRight = 0;

    // 操作复选框和参数控件
    static CheckButton? cbGaussianBlur;
    static HScale? scaleGaussianBlur;
    static CheckButton? cbBinary;
    static HScale? scaleBinary;
    static CheckButton? cbCanny;
    static HScale? scaleCanny;
    static CheckButton? cbDilate;
    static HScale? scaleDilate;
    static CheckButton? cbErode;
    static HScale? scaleErode;
    static CheckButton? cbDilate2;
    static HScale? scaleDilate2;
    static CheckButton? cbInvert;
    static CheckButton? cbResize;
    static SpinButton? spinWidth;
    static SpinButton? spinHeight;
    static CheckButton? cbRotate;
    static ComboBoxText? rotationCombo;
    static CheckButton? cbFlip;
    static ComboBoxText? flipCombo;

    [STAThread]
    [Obsolete]
    public static void Main()
    {
        Application.Init();
        var window = new Window("三栏可调节算法 + 缩放拖动 查看器");
        window.SetDefaultSize(1000, 600);
        window.Resizable = true;
        window.DeleteEvent += (o, e) => Application.Quit();


        // 横线

        // 左栏：垂直三栏布局
        VBox leftBox = new(false, 10);

        // 上方：图像处理选项
        VBox topOptions = new(false, 5);
        cbGaussianBlur = new CheckButton("高斯模糊");
        scaleGaussianBlur = new HScale(new Adjustment(2, 2, 10, 1, 1, 0))
        { Digits = 0, ValuePos = PositionType.Right };
        scaleGaussianBlur.ValueChanged += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbGaussianBlur, false, false, 0);
        topOptions.PackStart(scaleGaussianBlur, false, false, 0);

        cbBinary = new CheckButton("二值化");
        scaleBinary = new HScale(new Adjustment(1, 1, 255, 1, 1, 0))
        { Digits = 0, ValuePos = PositionType.Right };
        scaleBinary.ValueChanged += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbBinary, false, false, 0);
        topOptions.PackStart(scaleBinary, false, false, 0);

        cbCanny = new CheckButton("边缘检测");
        scaleCanny = new HScale(new Adjustment(1, 1, 120, 1, 1, 0))
        { Digits = 0, ValuePos = PositionType.Right };
        scaleCanny.ValueChanged += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbCanny, false, false, 0);
        topOptions.PackStart(scaleCanny, false, false, 0);

        cbDilate = new CheckButton("膨胀");
        scaleDilate = new HScale(new Adjustment(1, 1, 10, 1, 1, 0))
        { Digits = 0, ValuePos = PositionType.Right };
        scaleDilate.ValueChanged += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbDilate, false, false, 0);
        topOptions.PackStart(scaleDilate, false, false, 0);

        cbErode = new CheckButton("腐蚀");
        scaleErode = new HScale(new Adjustment(1, 1, 10, 1, 1, 0))
        { Digits = 0, ValuePos = PositionType.Right };
        scaleErode.ValueChanged += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbErode, false, false, 0);
        topOptions.PackStart(scaleErode, false, false, 0);

        cbDilate2 = new CheckButton("二次膨胀");
        scaleDilate2 = new HScale(new Adjustment(1, 1, 10, 1, 1, 0))
        { Digits = 0, ValuePos = PositionType.Right };
        scaleDilate2.ValueChanged += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbDilate2, false, false, 0);
        topOptions.PackStart(scaleDilate2, false, false, 0);

        cbInvert = new CheckButton("反相");
        cbInvert.Toggled += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbInvert, false, false, 0);

        cbResize = new CheckButton("调整大小");
        spinWidth = new SpinButton(new Adjustment(100, 1, 5000, 1, 10, 0), 1, 0);
        spinHeight = new SpinButton(new Adjustment(100, 1, 5000, 1, 10, 0), 1, 0);
        cbResize.Toggled += (s, e) => ApplyAlgorithm();
        spinWidth.ValueChanged += (s, e) => ApplyAlgorithm();
        spinHeight.ValueChanged += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbResize, false, false, 0);
        topOptions.PackStart(spinWidth, false, false, 0);
        topOptions.PackStart(spinHeight, false, false, 0);

        cbRotate = new CheckButton("旋转");
        rotationCombo = new ComboBoxText();
        rotationCombo.AppendText("无");
        rotationCombo.AppendText("顺时针 90°");
        rotationCombo.AppendText("逆时针 90°");
        rotationCombo.AppendText("180°");
        rotationCombo.Active = 0;
        rotationCombo.Changed += (s, e) => ApplyAlgorithm();
        cbRotate.Toggled += (s, e) => ApplyAlgorithm();
        rotationCombo.Changed += (s, e) => ApplyAlgorithm();
        topOptions.PackStart(cbRotate, false, false, 0);
        topOptions.PackStart(rotationCombo, false, false, 0);

        cbFlip = new CheckButton("翻转");
        cbFlip.Toggled += (s, e) => ApplyAlgorithm();
        flipCombo = new ComboBoxText();
        flipCombo.AppendText("无");
        flipCombo.AppendText("水平");
        flipCombo.AppendText("垂直");
        flipCombo.AppendText("中心");
        topOptions.PackStart(cbFlip, false, false, 0);
        topOptions.PackStart(flipCombo, false, false, 0);

        leftBox.PackStart(topOptions, false, false, 0);

        leftBox.PackStart(new Separator(Gtk.Orientation.Horizontal), false, false, 5);
        // 中间：占位空白
        leftBox.PackStart(new Label(), true, true, 0);

        leftBox.PackStart(new Separator(Gtk.Orientation.Horizontal), false, false, 5);
        // 下方：选择 & 保存 按钮
        HBox bottomButtons = new(false, 5);
        var loadButton = new Button("选择图片");
        loadButton.Clicked += OnLoadClicked;
        var saveButton = new Button("保存图片");
        saveButton.Clicked += OnSaveClicked;
        bottomButtons.PackStart(new Label(), false, false, 0);
        bottomButtons.PackStart(loadButton, false, false, 0);
        bottomButtons.PackStart(saveButton, false, false, 0);
        leftBox.PackStart(bottomButtons, false, false, 0);
        leftBox.PackStart(new Label(), true, true, 0);
        leftBox.PackStart(new Separator(Gtk.Orientation.Horizontal), false, false, 5);


        // 中栏：原始图片
        centerContainer = new EventBox();
        centerImage = new Image();
        centerContainer.Add(centerImage);
        SetupPanZoom(centerContainer,
            () => zoomCenter, z => zoomCenter = z,
            () => offsetXCenter, x => offsetXCenter = x,
            () => offsetYCenter, y => offsetYCenter = y,
            () => DrawPixbuf(originalPixbuf!, zoomCenter, offsetXCenter, offsetYCenter, centerContainer, centerImage));
        var centerFrame = new Frame("原始图片");
        centerFrame.Add(centerContainer);

        // 右栏：处理后图片
        rightContainer = new EventBox();
        rightImage = new Image();
        rightContainer.Add(rightImage);
        SetupPanZoom(rightContainer,
            () => zoomRight, z => zoomRight = z,
            () => offsetXRight, x => offsetXRight = x,
            () => offsetYRight, y => offsetYRight = y,
            () => DrawPixbuf(processedPixbuf!, zoomRight, offsetXRight, offsetYRight, rightContainer, rightImage));
        var rightFrame = new Frame("处理后图片");
        rightFrame.Add(rightContainer);
        var table = new Table(1, 5, true); // 1 行，5 列

        // 左栏占 1 列
        table.Attach(leftBox, 0, 1, 0, 1,
            AttachOptions.Expand | AttachOptions.Fill,
            AttachOptions.Expand | AttachOptions.Fill,
            5, 5);

        // 中栏占 2 列
        table.Attach(centerFrame, 1, 3, 0, 1,
            AttachOptions.Expand | AttachOptions.Fill,
            AttachOptions.Expand | AttachOptions.Fill,
            5, 5);

        // 右栏占 2 列
        table.Attach(rightFrame, 3, 5, 0, 1,
            AttachOptions.Expand | AttachOptions.Fill,
            AttachOptions.Expand | AttachOptions.Fill,
            5, 5);
        window.Add(table);
        window.ShowAll();
        Application.Run();
    }

    // 选择图片
    static void OnLoadClicked(object? sender, EventArgs e)
    {
        using var chooser = new FileChooserDialog(
            "选择图片", null, FileChooserAction.Open,
            "取消", ResponseType.Cancel,
            "打开", ResponseType.Accept);
        if (chooser.Run() == (int)ResponseType.Accept)
        {
            originalMat = CvInvoke.Imread(chooser.Filename);
            processedMat = new();
            originalPixbuf = new Pixbuf(chooser.Filename).RotateSimple(PixbufRotation.Counterclockwise).Flip(true);
            zoomCenter = 100; offsetXCenter = 0; offsetYCenter = 0;
            DrawPixbuf(originalPixbuf, zoomCenter, offsetXCenter, offsetYCenter, centerContainer!, centerImage!);
            ApplyAlgorithm();
        }
    }

    // 保存图片
    static void OnSaveClicked(object? sender, EventArgs e)
    {
        if (processedPixbuf == null) return;
        using var chooser = new FileChooserDialog(
            "保存图片", null, FileChooserAction.Save,
            "取消", ResponseType.Cancel,
            "保存", ResponseType.Accept);
        chooser.CurrentName = "processed.png";
        var filter = new FileFilter();
        filter.AddPattern("*.png");
        filter.Name = "PNG 图像";
        chooser.AddFilter(filter);
        if (chooser.Run() == (int)ResponseType.Accept)
        {
            var fn = chooser.Filename;
            if (!fn.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                fn += ".png";
            CvInvoke.Imwrite(fn, processedMat);
        }
    }

    // 应用算法，不重置缩放
    static void ApplyAlgorithm()
    {
        if (originalPixbuf == null) return;
        CvInvoke.CvtColor(originalMat, processedMat, ColorConversion.Bgr2Gray);

        if (cbGaussianBlur!.Active)
            CvInvoke.GaussianBlur(processedMat, processedMat, new((int)scaleGaussianBlur!.Value, (int)scaleGaussianBlur!.Value), scaleGaussianBlur!.Value / 3);
        if (cbBinary!.Active)
            CvInvoke.Threshold(processedMat, processedMat, (int)scaleBinary!.Value, 255, ThresholdType.Binary);
        if (cbCanny!.Active)
            CvInvoke.Canny(processedMat, processedMat, scaleCanny!.Value, scaleCanny!.Value * 2);
        if (cbDilate!.Active)
            CvInvoke.Dilate(processedMat, processedMat,
                CvInvoke.GetStructuringElement(MorphShapes.Rectangle, new((int)scaleDilate!.Value, (int)scaleDilate!.Value), new(-1, -1)),
                new(-1, -1), 1, BorderType.Default, new MCvScalar(0));
        if (cbErode!.Active)
            CvInvoke.Erode(processedMat, processedMat,
                CvInvoke.GetStructuringElement(MorphShapes.Rectangle, new((int)scaleErode!.Value, (int)scaleErode!.Value), new(-1, -1)),
                new(-1, -1), 1, BorderType.Default, new MCvScalar(0));
        if (cbDilate2!.Active)
            CvInvoke.Dilate(processedMat, processedMat,
                CvInvoke.GetStructuringElement(MorphShapes.Rectangle, new((int)scaleDilate2!.Value, (int)scaleDilate2!.Value), new(-1, -1)),
                new(-1, -1), 1, BorderType.Default, new MCvScalar(0));
        if (cbInvert!.Active) CvInvoke.BitwiseNot(processedMat, processedMat);
        CvInvoke.Threshold(processedMat, processedMat, 50, 100, ThresholdType.Binary);
        var pix = new Pixbuf(
                        Colorspace.Rgb,      // 色彩空间
                        false,               // 是否有 alpha 通道
                        8,                   // 每个颜色通道的位数
                        processedMat!.Width, processedMat!.Height
                        );
        unsafe
        {
            var bytes = (byte*)pix.Pixels;
            var span = (byte*)processedMat.DataPointer;
            for (int i = 0; i < processedMat!.Width; i++)
                for (int j = 0; j < processedMat!.Height; j++)
                {
                    var index = j * processedMat!.Width * 3 + i * 3;
                    bytes[index] =
                    span[j * processedMat!.Width + i];
                    bytes[index + 1] =
                    span[j * processedMat!.Width + i];
                    bytes[index + 2] =
                    span[j * processedMat!.Width + i];
                }
        }
        if (cbResize!.Active) pix = pix.ScaleSimple(spinWidth!.ValueAsInt, spinHeight!.ValueAsInt, InterpType.Bilinear);
        if (cbRotate!.Active) pix = rotationCombo!.ActiveText switch
        {
            "顺时针 90°" =>
                pix.RotateSimple(PixbufRotation.Clockwise),
            "逆时针 90°" =>
                pix.RotateSimple(PixbufRotation.Counterclockwise),
            "180°" =>
                pix.RotateSimple(PixbufRotation.Upsidedown),
            _ => pix,
        };
        if (cbFlip!.Active) pix = flipCombo!.ActiveText switch
        {
            "水平" =>
               pix.Flip(true),
            "垂直" =>
                 pix.Flip(false),
            "中心" =>
               pix.Flip(true).Flip(false),
            _ => pix
        };
        processedPixbuf?.Dispose();
        processedPixbuf = pix.RotateSimple(PixbufRotation.Counterclockwise).Flip(true);
        DrawPixbuf(processedPixbuf, zoomRight, offsetXRight, offsetYRight, rightContainer!, rightImage!);
    }

    // 算法 stub
    static Pixbuf MorphDilate(Pixbuf src, uint size) => src.Copy();
    static Pixbuf MorphErode(Pixbuf src, uint size) => src.Copy();
    static Pixbuf Invert(Pixbuf src)
    {
        var dest = src.Copy();
        int w = src.Width, h = src.Height, stride = src.Rowstride, chans = src.NChannels;
        var sp = src.PixelBytes.Data; var dp = dest.PixelBytes.Data;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int idx = y * stride + x * chans;
                dp[idx] = (byte)(255 - sp[idx]);
                dp[idx + 1] = (byte)(255 - sp[idx + 1]);
                dp[idx + 2] = (byte)(255 - sp[idx + 2]);
                if (src.HasAlpha) dp[idx + 3] = sp[idx + 3];
            }
        return dest;
    }
    static Pixbuf RotateArbitrary(Pixbuf src, float angle) => src.Copy();

    // 平移/缩放事件设置
    static void SetupPanZoom(
        EventBox container,
        Func<int> getZoom, Action<int> setZoom,
        Func<int> getOffsetX, Action<int> setOffsetX,
        Func<int> getOffsetY, Action<int> setOffsetY,
        System.Action redraw)
    {
        container.Events |= EventMask.ButtonPressMask
                          | EventMask.ButtonReleaseMask
                          | EventMask.PointerMotionMask
                          | EventMask.ScrollMask;
        int dragStartX = 0, dragStartY = 0;
        bool dragging = false;
        container.ButtonPressEvent += (o, args) =>
        {
            if (args.Event.Button == 1)
            {
                dragging = true;
                dragStartX = (int)args.Event.X;
                dragStartY = (int)args.Event.Y;
            }
        };
        container.ButtonReleaseEvent += (o, args) =>
        {
            if (args.Event.Button == 1) dragging = false;
        };
        container.MotionNotifyEvent += (o, args) =>
        {
            if (!dragging) return;
            int dx = (int)args.Event.X - dragStartX;
            int dy = (int)args.Event.Y - dragStartY;
            setOffsetX(getOffsetX() + dx);
            setOffsetY(getOffsetY() + dy);
            dragStartX = (int)args.Event.X;
            dragStartY = (int)args.Event.Y;
            redraw();
        };
        container.ScrollEvent += (o, args) =>
        {
            bool ctrl = (args.Event.State & ModifierType.ControlMask) != 0;
            if (!ctrl) return;
            if (args.Event.Direction == ScrollDirection.Up)
                setZoom(getZoom() + 10);
            else if (args.Event.Direction == ScrollDirection.Down)
                setZoom(Math.Max(10, getZoom() - 10));
            redraw();
        };
    }

    // 缩放/平移/裁剪并显示 Pixbuf
    static void DrawPixbuf(
        Pixbuf src,
        int zoom, int offsetX, int offsetY,
        EventBox container, Image widget)
    {
        if (src == null) return;
        int w = container.Allocation.Width;
        int h = container.Allocation.Height;
        int newW = src.Width * zoom / 100;
        int newH = src.Height * zoom / 100;
        var scaled = src.ScaleSimple(newW, newH, InterpType.Bilinear);
        var canvas = new Pixbuf(Colorspace.Rgb, true, 8, w, h);
        canvas.Fill(0xffffff00);
        int srcX = Math.Max(0, -offsetX);
        int srcY = Math.Max(0, -offsetY);
        int dstX = Math.Max(0, offsetX);
        int dstY = Math.Max(0, offsetY);
        int copyW = Math.Min(scaled.Width - srcX, w - dstX);
        int copyH = Math.Min(scaled.Height - srcY, h - dstY);
        if (copyW > 0 && copyH > 0)
            scaled.CopyArea(srcX, srcY, copyW, copyH, canvas, dstX, dstY);
        widget.Pixbuf = canvas;
    }
}
