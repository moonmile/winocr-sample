using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace winocr
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            var path = textBox2.Text;
            pictureBox1.Image = Bitmap.FromFile(path);

            var sb = await LoadImage(path);
            var result = await detect(sb);
            textBox1.Text =  result.Text;
        }


        public async Task<OcrResult> detect(SoftwareBitmap bitmap)
        {
            var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            var ocrResult = await ocrEngine.RecognizeAsync(bitmap);
            return ocrResult;
        }

        /// <summary>
        /// ファイルパスを指定して SoftwareBitmap を取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<SoftwareBitmap> LoadImage(string path)
        {
            var fs = System.IO.File.OpenRead(path);
            var buf = new byte[fs.Length];
            fs.Read(buf, 0, (int)fs.Length);
            var mem = new MemoryStream(buf);
            mem.Position = 0;

            var stream = await ConvertToRandomAccessStream(mem);
            var bitmap = await LoadImage(stream);
            return bitmap;
        }
        /// <summary>
        /// IRandomAccessStream から SoftwareBitmap を取得
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task<SoftwareBitmap> LoadImage(IRandomAccessStream stream)
        {
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            return bitmap;
        }
        /// <summary>
        /// MemoryStream から IRandomAccessStream へ変換
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <returns></returns>
        public async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = new Task(() => dw.WriteBytes(memoryStream.ToArray()));
            task.Start();
            await task;
            await dw.StoreAsync();
            await outputStream.FlushAsync();
            return randomAccessStream;
        }


    }
}